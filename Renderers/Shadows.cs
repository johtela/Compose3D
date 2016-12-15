namespace Compose3D.Renderers
{
	using System;
	using System.Linq;
	using Extensions;
	using Compose3D.GLTypes;
	using Compose3D.Imaging;
	using Compose3D.Maths;
	using Compose3D.Reactive;
	using Compose3D.SceneGraph;
	using Compose3D.Shaders;
	using Compose3D.Textures;
	using OpenTK.Graphics.OpenGL4;

	public enum ShadowMapType { Depth, Variance }
	
	public class Shadows : Uniforms
	{
		// Uniforms
		public Uniform<Mat4> modelViewMatrix;
		public ShadowUniforms shadowUniforms;
		public CascadedShadowUniforms csmUniforms;

		private static GLProgram _shadowShader;
		private static Shadows _instance;
		private bool _cascaded;

		public static Shadows Instance
		{
			get { return _instance; }
		}

		private Shadows (GLProgram program, bool cascaded) : base (program)
		{
			_cascaded = cascaded;
			if (_cascaded)
				csmUniforms = new CascadedShadowUniforms (program);
			else
			{
				shadowUniforms = new ShadowUniforms (program);
			}
		}

		public static Reaction<Camera> Renderer (SceneGraph scene,
			int mapSize, ShadowMapType type, bool cascaded)
		{
			var depthFramebuffer = new Framebuffer (FramebufferTarget.Framebuffer);
			_shadowShader = cascaded ?
				new GLProgram (
					VertexShaderCascaded (),
					GeometryShaderCascaded (),
					DepthFragmentShader ()) :
				new GLProgram (
					VertexShader (),
					type == ShadowMapType.Depth ? DepthFragmentShader () : VarianceFragmentShader ());

			_instance = new Shadows (_shadowShader, cascaded);

			Texture depthTexture;
			var render =React.By<Camera> (_instance.Render);
			if (type == ShadowMapType.Depth || cascaded)
			{
				depthTexture = cascaded ?
					new Texture (TextureTarget.Texture2DArray, PixelInternalFormat.DepthComponent16,
						mapSize, mapSize, CascadedShadowUniforms.MapCount, PixelFormat.DepthComponent, 
						PixelType.Float, IntPtr.Zero) :
					new Texture (TextureTarget.Texture2D, PixelInternalFormat.DepthComponent16,
						mapSize, mapSize, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
				depthFramebuffer.AddTexture (FramebufferAttachment.DepthAttachment, depthTexture);
			}
			else
			{
				depthTexture = new Texture (TextureTarget.Texture2D, PixelInternalFormat.Rg32f,
					mapSize, mapSize, PixelFormat.Rg, PixelType.Float, IntPtr.Zero);
				depthFramebuffer.AddTexture (FramebufferAttachment.ColorAttachment0, depthTexture);
				depthFramebuffer.AddRenderbuffer (FramebufferAttachment.DepthAttachment,
					RenderbufferStorage.DepthComponent16, mapSize, mapSize);
				var gaussTexture = new Texture (TextureTarget.Texture2D, PixelInternalFormat.Rg32f,
					mapSize / 2, mapSize / 2, PixelFormat.Rg, PixelType.Float, IntPtr.Zero);
				render = render.And (GaussianFilter.Both ().MapInput ((Camera cam) =>
					Tuple.Create (depthTexture, gaussTexture)));
			}
			scene.GlobalLighting.ShadowMap = depthTexture;

			return render
				.DrawBuffer (type == ShadowMapType.Depth ? DrawBufferMode.None : DrawBufferMode.Front)
				.DepthTest ()
				.Culling ()
				.Viewport (new Vec2i (mapSize, mapSize))
				.Program (_shadowShader)
				.Texture (depthTexture)
				.Framebuffer (depthFramebuffer);
		}

		private void Render (Camera camera)
		{
			GL.Clear (ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

			var light = camera.Graph.Root.Traverse ().OfType<DirectionalLight> ().First ();
			var worlToCamera = camera.WorldToCamera;
			if (_cascaded)
				csmUniforms.UpdateLightSpaceMatrices (camera, light);
			else
				shadowUniforms.UpdateLightSpaceMatrix (camera, light);

			foreach (var mesh in camera.NodesInView<Mesh<EntityVertex>> ())
			{
				modelViewMatrix &= worlToCamera * mesh.Transform;
				_shadowShader.DrawElements (PrimitiveType.Triangles, mesh.VertexBuffer, mesh.IndexBuffer);
			}
		}

		private static GLShader VertexShader ()
		{
			return GLShader.Create (ShaderType.VertexShader, () =>
				from v in Shader.Inputs<EntityVertex> ()
				from u in Shader.Uniforms<Shadows> ()
				from s in Shader.Uniforms<ShadowUniforms> ()
				select new Fragment ()
				{
					gl_Position = !s.lightSpaceMatrix * !u.modelViewMatrix * 
						new Vec4 (v.position, 1f)
				}
			);
		}

		private static GLShader VertexShaderCascaded ()
		{
			return GLShader.Create (ShaderType.VertexShader, () =>
				from v in Shader.Inputs<EntityVertex> ()
				from u in Shader.Uniforms<Shadows> ()
				select new Fragment ()
				{
					gl_Position = !u.modelViewMatrix * new Vec4 (v.position, 1f)
				}
			);
		}

		public static GLShader GeometryShaderCascaded ()
		{
			ShadowShaders.Use ();
			return GLShader.CreateGeometryShader<PerVertexOut> (3, 
				CascadedShadowUniforms.MapCount, PrimitiveType.Triangles, 
				PrimitiveType.TriangleStrip, () =>
				from p in Shader.Inputs<Primitive> ()
				from u in Shader.Uniforms<Shadows> ()
				from c in Shader.Uniforms<CascadedShadowUniforms> ()
				let viewLight = (!c.viewLightMatrices)[p.gl_InvocationID]
				select new PerVertexOut[3]
				{
					new PerVertexOut ()
					{
						gl_Position = ShadowShaders.ClampToNearPlane (viewLight * p.gl_in[0].gl_Position),
						gl_Layer = p.gl_InvocationID
					},
					new PerVertexOut ()
					{
						gl_Position = ShadowShaders.ClampToNearPlane (viewLight * p.gl_in[1].gl_Position),
						gl_Layer = p.gl_InvocationID
					},
					new PerVertexOut ()
					{
						gl_Position = ShadowShaders.ClampToNearPlane (viewLight * p.gl_in[2].gl_Position),
						gl_Layer = p.gl_InvocationID
					}
				}
			);
		}

		private static GLShader DepthFragmentShader ()
		{
			return GLShader.Create (ShaderType.FragmentShader, () =>
				from f in Shader.Inputs<Fragment> ()
				select new
				{
					depth = f.gl_FragCoord[2]
				}
			);
		}

		private static GLShader VarianceFragmentShader ()
		{
			return GLShader.Create (ShaderType.FragmentShader, () =>
				from f in Shader.Inputs<Fragment> ()
				let depth = f.gl_FragCoord.Z
				let dx = GLMath.dFdx (depth)
				let dy = GLMath.dFdy (depth)
				let moment2 = depth * depth + 0.25f * (dx * dx + dy * dy)
				select new
				{
					variance = new Vec2 (depth, moment2)
				}
			);
		}
	}
}