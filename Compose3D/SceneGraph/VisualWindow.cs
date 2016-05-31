namespace Compose3D.SceneGraph
{
	using OpenTK.Graphics.OpenGL;
	using GLTypes;
	using Geometry;
	using Maths;
	using Textures;
	using DataStructures;
	using Visuals;
	using System.Drawing;

	public class VisualWindow<V> : Window<V>
		where V : struct, IVertex, ITextured
	{
		private Visual _visual;

		public VisualWindow (SceneGraph graph, Visual visual, Vec2i size)
			: base (graph, true)
		{
			_visual = visual;
			Texture = Texture.FromBitmap (
				visual.ToBitmap (new Size (size.X, size.Y), 
				System.Drawing.Imaging.PixelFormat.Canonical));
		}

		public Visual Visual
		{
			get { return _visual; }
			set
			{
				_visual = value;
				Texture.UpdateBitmap (_visual.ToBitmap (
					new Size (Texture.Size.X, Texture.Size.Y),
					System.Drawing.Imaging.PixelFormat.Canonical),
					TextureTarget.Texture2D, 0);
			}
		}

	}
}
