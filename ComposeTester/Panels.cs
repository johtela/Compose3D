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
	
	public class Panels
	{
		public class PanelFragment : Fragment, IFragmentTexture<Vec2>
		{
			public Vec2 fragTexturePos { get; set; }
		}

		private TextureUniforms texture;
		private TransformUniforms transform;

		private static Program _panelShader;
		private static Panels _panels;
		private static SceneGraph _scene;

		private Panels ()
		{
			texture = new TextureUniforms (_panelShader, new Sampler2D (0).NearestColor ()
				.ClampToEdges (Axes.X | Axes.Y));
			transform = new TransformUniforms (_panelShader);
		}

		public static Reaction<Vec2i> Renderer (SceneGraph scene)
		{
			_panelShader = new Program (
				VertexShaders.TransformedTexture<TexturedVertex, PanelFragment, TransformUniforms> (),
				FragmentShaders.TexturedOutput<PanelFragment, TextureUniforms> ());
			_panels = new Panels ();
			_scene = scene;

			return React.By<Vec2i> (_panels.Render)
				.Blending ()
				.Culling ()
				.Program (_panelShader);
		}

		private void Render (Vec2i viewportSize)
		{
			transform.perspectiveMatrix &= new Mat4 (1f);
			foreach (var panel in _scene.Root.Traverse ().OfType<Panel<TexturedVertex>> ())
			{
				(!texture.textureMap).Bind (panel.Texture);
				transform.modelViewMatrix &= panel.GetModelViewMatrix (viewportSize);
				_panelShader.DrawElements (BeginMode.Triangles, panel.VertexBuffer, panel.IndexBuffer);
				(!texture.textureMap).Unbind (panel.Texture);
			}
		}
	}
}