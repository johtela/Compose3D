namespace ComposeTester
{
	using System;
	using System.Linq;
	using System.Runtime.InteropServices;
	using Compose3D.GLTypes;
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.SceneGraph;
	using Compose3D.Shaders;
	using Compose3D.Textures;
	using OpenTK.Graphics.OpenGL;
	
	public class Windows
	{
		public class WindowFragment : Fragment, IFragmentTexture<Vec2>
		{
			public Vec2 fragTexturePos { get; set; }
		}

		public readonly Program WindowShader;
		public readonly TextureUniforms Texture;
		public readonly TransformUniforms Transform;

		public Windows ()
		{
			WindowShader = new Program (
				VertexShaders.TransformedTexture<TexturedVertex, WindowFragment, TransformUniforms> (), 
				FragmentShaders.TexturedOutput<WindowFragment, TextureUniforms> ());
			Texture = new TextureUniforms (WindowShader, new Sampler2D (0).NearestColor ()
				.ClampToEdges (Axes.X | Axes.Y));
			Transform = new TransformUniforms (WindowShader);
		}

		public void Render (SceneGraph scene, Vec2 viewportSize)
		{
			GL.Enable (EnableCap.CullFace);
			GL.CullFace (CullFaceMode.Back);
			GL.FrontFace (FrontFaceDirection.Cw);
			GL.Disable (EnableCap.DepthTest);
			GL.Enable (EnableCap.Blend);
			GL.BlendFunc (BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			using (WindowShader.Scope ())
				foreach (var window in scene.Root.Traverse ().OfType <Window<TexturedVertex>> ())
				{
					var texSize = window.Texture.Size * 2;
					var scalingMat = Mat.Scaling<Mat4> (texSize.X / viewportSize.X, texSize.Y / viewportSize.Y);

					(!Texture.textureMap).Bind (window.Texture);
					Transform.modelViewMatrix &= window.Transform * scalingMat;
					WindowShader.DrawElements (PrimitiveType.Triangles, window.VertexBuffer, window.IndexBuffer);
					(!Texture.textureMap).Unbind (window.Texture);
				}
		}
	}
}