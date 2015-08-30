namespace ComposeTester
{
	using Compose3D.Arithmetics;
	using Compose3D.Geometry;
	using Compose3D.GLTypes;
	using OpenTK;
	using System;
	using System.Linq;

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
			return Composite.Create (Stacking.StackRight (cube1, cube2, cube3)
				.Align (Alignment.None, Alignment.Center, Alignment.Center)).Center ();
		}

		public static Geometry<Vertex> Roof ()
		{
			var trapezoid = Quadrilateral<Vertex>.Trapezoid (20f, 1f, 0f, 1f, Mater ());
			var roofSnapTag = trapezoid.TagVertex (trapezoid.Vertices.Bottommost (). Rightmost().Single ());
			var leftPane = trapezoid.Extrude (30f, true).Rotate (0f, 0f, MathHelper.PiOver4);
			var rightPane = leftPane.ReflectX ();
			var roof = Composite.Create (Stacking.StackRight (leftPane, rightPane));
			var gableHeight = roof.BoundingBox.Size.Y * 0.85f;
			var frontGable = Triangle<Vertex>.Isosceles (2 * gableHeight, gableHeight, Mater ());
			var gableTopTag = frontGable.TagVertex (frontGable.Vertices.Topmost ().Single ());
			var backGable = frontGable.ReflectZ ().Translate (0f, 0f, -roof.BoundingBox.Size.Z * 0.85f);
			var gables = Composite.Create (frontGable, backGable);
			var walls = Quadrilateral<Vertex>.Rectangle (gables.BoundingBox.Size.X, gables.BoundingBox.Size.Z, Mater ())
				.Extrude (12f, false).Rotate (MathHelper.PiOver2, 0f, 0f);
			var wallsAndGables = Composite.Create (Stacking.StackUp (walls, gables)
				.Align (Alignment.Center, Alignment.None, Alignment.Center));
			wallsAndGables = wallsAndGables.SnapVertex (wallsAndGables.FindVertexByTag (gableTopTag), 
				roof.FindVertexByTag (roofSnapTag), Axes.All);
			return Composite.Create (
				Aligning.Align (Alignment.None, Alignment.None, Alignment.Center, roof, wallsAndGables)).Center ();
		}
	}
}

