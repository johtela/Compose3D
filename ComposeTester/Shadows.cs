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

		private const int _textureSize = 256;

		public Shadows ()
		{
			ShadowShader = new Program (VertexShader (), FragmentShader ());
			Uniforms = new ShadowUniforms (ShadowShader);
			DepthTexture = new Texture (TextureTarget.Texture2D, false, PixelInternalFormat.DepthComponent16,
				_textureSize, _textureSize, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero,
				new TextureParams ()
				{
					{ TextureParameterName.TextureMagFilter, TextureMagFilter.Nearest },
					{ TextureParameterName.TextureMinFilter, TextureMinFilter.Nearest },
					{ TextureParameterName.TextureWrapS, TextureWrapMode.ClampToEdge },
					{ TextureParameterName.TextureWrapT, TextureWrapMode.ClampToEdge }
				});
			DepthFramebuffer = new Framebuffer (FramebufferTarget.Framebuffer);
			DepthFramebuffer.AddTexture (FramebufferAttachment.DepthAttachment, DepthTexture);
		}

		public void Render (Camera camera)
		{
			using (DepthFramebuffer.Scope ())
			using (DepthTexture.Scope ())
			using (ShadowShader.Scope ())
			{
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
				var vpMatrix = shadowFrustum.CameraToScreen * /* light.CameraToLight (camera) **/  camera.WorldToCamera;

				foreach (var mesh in camera.NodesInView<Mesh<EntityVertex>> ())
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