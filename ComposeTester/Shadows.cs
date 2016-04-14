namespace ComposeTester
{
	using System;
	using System.Linq;
	using System.Runtime.InteropServices;
	using Extensions;
	using Compose3D.GLTypes;
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.SceneGraph;
	using Compose3D.Shaders;
	using Compose3D.Textures;
	using OpenTK.Graphics.OpenGL;

	public enum ShadowMapType { Depth, Variance }
	
	public class Shadows
	{
		public class ShadowUniforms : Uniforms
		{
			public Uniform<Mat4> mvpMatrix;

			public ShadowUniforms (Program program) : base (program) { }
		}

		public readonly Program ShadowShader;
		public readonly ShadowUniforms Uniforms;
		public readonly Texture DepthTexture;
		public readonly Framebuffer DepthFramebuffer;

		private int _mapSize;
		private ShadowMapType _type;

		public Shadows (SceneGraph scene, int mapSize, ShadowMapType type)
		{
			_mapSize = mapSize;
			_type = type;
			DepthFramebuffer = new Framebuffer (FramebufferTarget.Framebuffer);
			ShadowShader = new Program (VertexShader (), 
				type == ShadowMapType.Depth ? DepthFragmentShader () : VarianceFragmentShader ());
			Uniforms = new ShadowUniforms (ShadowShader);
			if (type == ShadowMapType.Depth)
			{
				DepthTexture = new Texture (TextureTarget.Texture2D, PixelInternalFormat.DepthComponent16,
					_mapSize, _mapSize, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
				DepthFramebuffer.AddTexture (FramebufferAttachment.DepthAttachment, DepthTexture);
			}
			else
			{
				DepthTexture = new Texture (TextureTarget.Texture2D, PixelInternalFormat.Rg32f,
					_mapSize, _mapSize, PixelFormat.Rg, PixelType.Float, IntPtr.Zero);					
				DepthFramebuffer.AddTexture (FramebufferAttachment.ColorAttachment0, DepthTexture);
				DepthFramebuffer.AddRenderbuffer (FramebufferAttachment.DepthAttachment, 
					RenderbufferStorage.DepthComponent16, _mapSize, _mapSize);
			}
			scene.GlobalLighting.ShadowMap = DepthTexture;
		}

		public void Render (Camera camera, Mesh<EntityVertex>[] meshes)
		{
			using (DepthFramebuffer.Scope ())
			using (DepthTexture.Scope ())
			using (ShadowShader.Scope ())
			{
				GL.Viewport (new System.Drawing.Size (_mapSize, _mapSize));
				GL.Enable (EnableCap.CullFace);
				GL.CullFace (CullFaceMode.Back);
				GL.FrontFace (FrontFaceDirection.Cw);
				GL.Enable (EnableCap.DepthTest);
				GL.DepthMask (true);
				GL.DepthFunc (DepthFunction.Less);
				GL.Disable (EnableCap.Blend);
				GL.DrawBuffer (_type == ShadowMapType.Depth ? DrawBufferMode.None : DrawBufferMode.Front);
				GL.Clear (ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

				var light = camera.Graph.Root.Traverse ().OfType<DirectionalLight> ().First ();
				var shadowFrustum = light.ShadowFrustum (camera);
				var vpMatrix = shadowFrustum.CameraToScreen * light.CameraToLightSpace (camera) * 
					camera.WorldToCamera;

				foreach (var mesh in meshes)
				{
					Uniforms.mvpMatrix &= vpMatrix * mesh.Transform;
					ShadowShader.DrawElements (PrimitiveType.Triangles, mesh.VertexBuffer, mesh.IndexBuffer);
				}
			}
		}

		public static GLShader VertexShader ()
		{
			return GLShader.Create (ShaderType.VertexShader, () =>
				from v in Shader.Inputs<EntityVertex> ()
				from u in Shader.Uniforms<ShadowUniforms> ()
				select new Fragment ()
				{
					gl_Position = !u.mvpMatrix * new Vec4 (v.position, 1f)
				}
			);
		}

		public static GLShader DepthFragmentShader ()
		{
			return GLShader.Create (ShaderType.FragmentShader, () =>
				from f in Shader.Inputs<Fragment> ()
				select new
				{
					depth = f.gl_FragCoord.Z
				}
			);
		}

		public static GLShader VarianceFragmentShader ()
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