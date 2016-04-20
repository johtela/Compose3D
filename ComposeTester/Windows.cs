namespace ComposeTester
{
	using System.Linq;
	using Compose3D.GLTypes;
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.Reactive;
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

		private TextureUniforms texture;
		private TransformUniforms transform;

		private static Program _windowShader;
		private static Windows _windows;
		private static SceneGraph _scene;

		private Windows ()
		{
			texture = new TextureUniforms (_windowShader, new Sampler2D (0).NearestColor ()
				.ClampToEdges (Axes.X | Axes.Y));
			transform = new TransformUniforms (_windowShader);
		}

		public static Reaction<Vec2> Renderer (SceneGraph scene)
		{
			_windowShader = new Program (
				VertexShaders.TransformedTexture<TexturedVertex, WindowFragment, TransformUniforms> (),
				FragmentShaders.TexturedOutput<WindowFragment, TextureUniforms> ());
			_windows = new Windows ();
			_scene = scene;

			return React.By<Vec2> (_windows.Render)
				.Blending ()
				.Culling ()
				.Program (_windowShader);
		}

		private void Render (Vec2 viewportSize)
		{
			foreach (var window in _scene.Root.Traverse ().OfType<Window<TexturedVertex>> ())
			{
				var texSize = window.Texture.Size * 2;
				var scalingMat = Mat.Scaling<Mat4> (texSize.X / viewportSize.X, texSize.Y / viewportSize.Y);

				(!texture.textureMap).Bind (window.Texture);
				transform.perspectiveMatrix &= new Mat4 (1f);
				transform.modelViewMatrix &= window.Transform * scalingMat;
				_windowShader.DrawElements (PrimitiveType.Triangles, window.VertexBuffer, window.IndexBuffer);
				(!texture.textureMap).Unbind (window.Texture);
			}
		}
	}
}