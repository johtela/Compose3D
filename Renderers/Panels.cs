﻿namespace Compose3D.Renderers
{
	using System.Linq;
	using GLTypes;
	using Maths;
	using Geometry;
	using Reactive;
	using SceneGraph;
	using Shaders;
	using Textures;
	using OpenTK.Graphics.OpenGL4;
	
	public class Panels
	{
		public class PanelFragment : Fragment, IFragmentTexture<Vec2>
		{
			public Vec2 fragTexturePos { get; set; }
		}

		private TextureUniforms texture;
		private TransformUniforms transform;

		private static GLProgram _panelShader;
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
			_panelShader = new GLProgram (
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
			var renderedPanels =
				from p in _scene.Root.Traverse ().OfType<Panel<TexturedVertex>> ()
				where p.Renderer == PanelRenderer.Standard
				select p;

			foreach (var panel in renderedPanels)
			{
				(!texture.textureMap).Bind (panel.Texture);
				transform.modelViewMatrix &= panel.GetModelViewMatrix (viewportSize);
				_panelShader.DrawElements (PrimitiveType.Triangles, panel.VertexBuffer, panel.IndexBuffer);
				(!texture.textureMap).Unbind (panel.Texture);
			}
		}
	}
}