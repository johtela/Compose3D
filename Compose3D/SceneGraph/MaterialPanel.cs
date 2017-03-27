namespace Compose3D.SceneGraph
{
	using Geometry;
	using Maths;
	using Textures;

	public class MaterialPanel<V> : Panel<V>
		where V : struct, IVertex, ITextured
	{
		private Vec2i _repeat;

		public MaterialPanel (SceneGraph graph, bool flipVertically, bool movable, Vec2i repeat)
			: base (graph, flipVertically, movable)
		{
			_repeat = repeat;
			_renderer = PanelRenderer.Custom;
		}

		protected override void SetTextureCoordinates (Geometry<V> geometry)
		{
			var fac = _repeat.Convert<Vec2i, Vec2> ();
			if (_flipVertically)
				geometry.ApplyTextureFront (1f, TexturePos.TopLeft * fac, TexturePos.BottomRight * fac);
			else
				geometry.ApplyTextureFront (1f, TexturePos.BottomLeft, TexturePos.TopRight * fac);
		}

		protected override Vec2i GetSize ()
		{
			return base.GetSize () * _repeat;
		}

		public static TransformNode Movable (SceneGraph graph, bool flipVertically, Vec2 pos, Vec2i repeat)
		{
			return new MaterialPanel<V> (graph, flipVertically, true, repeat)
				.Offset (new Vec3 (pos, 0f));
		}
	}
}
