namespace Compose3D.Renderers
{
	using System.Linq;
	using GLTypes;
	using Maths;
	using Reactive;
	using SceneGraph;
	using Shaders;
	using Textures;
	using OpenTK.Graphics.OpenGL4;
	
	public class MaterialPanels : Uniforms
	{
		public class PanelFragment : Fragment, IFragmentTexture<Vec2>
		{
			public Vec2 fragTexturePos { get; set; }
		}

		public TextureUniforms Texture;
		public TransformUniforms Transform;
		public Uniform<int> ColorOutput;

		private static GLProgram _panelShader;
		private static MaterialPanels _panels;
		private static SceneGraph _scene;

		private MaterialPanels (GLProgram program)
			: base (program)
		{
			Texture = new TextureUniforms (_panelShader, new Sampler2D (0).NearestColor ());
			Transform = new TransformUniforms (_panelShader);
		}

		public static Reaction<Vec2i> Renderer (SceneGraph scene)
		{
			_panelShader = new GLProgram (
				VertexShaders.TransformedTexture<TexturedVertex, PanelFragment, TransformUniforms> (),
				FragmentShader ());
			_panels = new MaterialPanels (_panelShader);
			_scene = scene;

			return React.By<Vec2i> (_panels.Render)
				.Blending ()
				.Culling ()
				.Program (_panelShader);
		}

		private void Render (Vec2i viewportSize)
		{
			Transform.perspectiveMatrix &= new Mat4 (1f);
			var renderedPanels =
				from p in _scene.Root.Traverse ().OfType<MaterialPanel<TexturedVertex>> ()
				where p.Texture != null
				select p;

			foreach (var panel in renderedPanels)
			{
				(!Texture.textureMap).Bind (panel.Texture);
				Transform.modelViewMatrix &= panel.GetModelViewMatrix (viewportSize);
				ColorOutput &= panel.Texture.PixelFormat == PixelFormat.Rgba ? 1 : 0;
				_panelShader.DrawElements (PrimitiveType.Triangles, panel.VertexBuffer, panel.IndexBuffer);
				(!Texture.textureMap).Unbind (panel.Texture);
			}
		}

		public static GLShader FragmentShader ()
		{
			return GLShader.Create (ShaderType.FragmentShader, () =>
				from f in Shader.Inputs<PanelFragment> ()
				from t in Shader.Uniforms<TextureUniforms> ()
				from u in Shader.Uniforms<MaterialPanels> ()
				let col = (!t.textureMap).Texture (f.fragTexturePos)
				select new
				{
					outputColor = !u.ColorOutput != 0 ? col : 
						new Vec4 (col[Coord.x, Coord.x, Coord.x], 1f)
				});
		}
	}
}