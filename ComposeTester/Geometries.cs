namespace ComposeTester
{
	using Compose3D.Arithmetics;
	using Compose3D.Geometry;
	using Compose3D.GLTypes;
	using OpenTK;
	using System;

	public static class Geometries
	{
		private static IMaterial Mater ()
		{
			return Material.RepeatColors (Color.Random);
		}

		public static Geometry<Vertex> Hammer ()
		{
			var cube1 = Volume.Cube<Vertex> (1f, 1.5f, 2f, Mater ()).Rotate (0f, MathHelper.PiOver2, 0f);
			var cube2 = Volume.Cube<Vertex> (1f, 1f, 1f, Mater ()).Scale (0.8f, 0.8f, 0.8f);
			var cube3 = Volume.Cube<Vertex> (1f, 1f, 2f, Mater ());
			return Composite.StackRight (Align.Center, Align.Center, cube1, cube2, cube3).Center ();
		}

		public static Geometry<Vertex> Roof ()
		{
			var leftPane = Quadrilateral<Vertex>.Trapezoid (20f, 1f, 0f, 1f, Mater ())
				.Extrude (20f).Rotate (0f, 0f, MathHelper.PiOver4);
			var rightPane = leftPane.ReflectX ();
			return Composite.StackRight (Align.Negative, Align.Negative, leftPane, rightPane).Center ();
		}
	}
}

