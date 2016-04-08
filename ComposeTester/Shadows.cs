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

		private const int _textureSize = 4096;

		public Shadows (SceneGraph scene)
		{
			ShadowShader = new Program (VertexShader (), FragmentShader ());
			Uniforms = new ShadowUniforms (ShadowShader);
			DepthTexture = new Texture (TextureTarget.Texture2D, false, PixelInternalFormat.DepthComponent32f,
				_textureSize, _textureSize, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero,
				new TextureParams ()
				{
					{ TextureParameterName.TextureMagFilter, TextureMagFilter.Linear  },
					{ TextureParameterName.TextureMinFilter, TextureMinFilter.Linear },
					{ TextureParameterName.TextureWrapS, TextureWrapMode.ClampToEdge },
					{ TextureParameterName.TextureWrapT, TextureWrapMode.ClampToEdge }
				});
			DepthFramebuffer = new Framebuffer (FramebufferTarget.Framebuffer);
			DepthFramebuffer.AddTexture (FramebufferAttachment.DepthAttachment, DepthTexture);
			scene.GlobalLighting.ShadowMap = DepthTexture;
		}

		public void Render (Camera camera, Mesh<EntityVertex>[] meshes)
		{
			using (DepthFramebuffer.Scope ())
			using (DepthTexture.Scope ())
			using (ShadowShader.Scope ())
			{
				GL.Viewport (new System.Drawing.Size (_textureSize, _textureSize));
				GL.Enable (EnableCap.CullFace);
				GL.CullFace (CullFaceMode.Back);
				GL.FrontFace (FrontFaceDirection.Cw);
				GL.Enable (EnableCap.DepthTest);
				GL.DepthMask (true);
				GL.DepthFunc (DepthFunction.Less);
				GL.Disable (EnableCap.Blend);
				GL.DrawBuffer (DrawBufferMode.None);
				GL.Clear (ClearBufferMask.DepthBufferBit);

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

		public static GLShader FragmentShader ()
		{
			return GLShader.Create (ShaderType.FragmentShader, () =>
				from f in Shader.Inputs<Fragment> ()
				select new
				{
					fragmentDepth = f.gl_FragCoord.Z
				}
			);
		}
	}
}