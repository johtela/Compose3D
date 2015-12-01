namespace ComposeTester
{
	using Compose3D;
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.GLTypes;
	using OpenTK;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class Geometries
	{
		public static Geometry<Vertex> Hammer ()
		{
			var cube1 = Solids.Cube<Vertex> (1f, 1.5f, 2f).RotateY (MathHelper.PiOver2);
			var cube2 = Solids.Cube<Vertex> (1f, 1f, 1f).Scale (0.8f, 0.8f, 0.8f);
			var cube3 = Solids.Cube<Vertex> (1f, 1f, 2f);
			return Composite.Create (Stacking.StackRight (cube1, cube2, cube3)
				.Align (Alignment.None, Alignment.Center, Alignment.Center)).Center ();
		}

		private static Geometry<Vertex> Roof (out int tag)
		{
			var trapezoid = Quadrilateral<Vertex>.Trapezoid (20f, 1f, 0f, 1f);
			tag = trapezoid.TagVertex (trapezoid.Vertices.Furthest (Dir3D.Down).Furthest (Dir3D.Right).Single ());
			var leftPane = trapezoid.Extrude (30f).RotateZ (MathHelper.PiOver4);
			var rightPane = leftPane.ReflectX ();
			return Composite.Create (Stacking.StackRight (leftPane, rightPane));
		}

		private static Geometry<Vertex> Gables (Geometry<Vertex> roof, out int tag)
		{
			var gableHeight = roof.BoundingBox.Size.Y * 0.85f;
			var frontGable = Triangle<Vertex>.Isosceles (2 * gableHeight, gableHeight);
			tag = frontGable.TagVertex (frontGable.Vertices.Furthest (Dir3D.Up).Single ());
			var backGable = frontGable.ReflectZ ().Translate (0f, 0f, -roof.BoundingBox.Size.Z * 0.9f);
			return Composite.Create (frontGable, backGable);
		}

		private static Geometry<Vertex> WallsAndGables (Geometry<Vertex> roof, Geometry<Vertex> gables, 
			int roofSnapTag, int gableTopTag)
		{
			var walls = Quadrilateral<Vertex>.Rectangle (gables.BoundingBox.Size.X, gables.BoundingBox.Size.Z)
				.Extrude (12f, false).RotateX (MathHelper.PiOver2);
			var wallsAndGables = Composite.Create (Stacking.StackUp (walls, gables)
				.Align (Alignment.Center, Alignment.None, Alignment.Center));
			return wallsAndGables.SnapVertex (wallsAndGables.FindVertexByTag (gableTopTag), 
				roof.FindVertexByTag (roofSnapTag), Axes.All);
		}

		public static Geometry<Vertex> House ()
		{
			int roofSnapTag, gableTopTag;
			var roof = Roof (out roofSnapTag);
			var gables = Gables (roof, out gableTopTag);
			var wallsAndGables = WallsAndGables (roof, gables, roofSnapTag, gableTopTag);
			return Composite.Create (Aligning.AlignZ (Alignment.Center, roof, wallsAndGables)).Center ();
		}

		private static IEnumerable<Mat4> TubeTransforms ()
		{
			var res = new Mat4 (1f);
			for (int i = 0; i < 10; i++)
			{
				res = Mat.RotationX<Mat4> (10f.Radians ()) * Mat.Translation<Mat4> (0f, 0f, -10f) * res;
				yield return res;
			}
		}

		public static Geometry<Vertex> Tube ()
		{
			return Circular<Vertex>.Circle (10f)
				.Stretch (TubeTransforms (), true, true)
				.Smoothen (0.9f)
				.Center ();
		}

		public static Geometry<Vertex> Arrow ()
		{
			return Circular<Vertex>.Circle (10f, 10f.Radians ())
				.Stretch (new Mat4[] { Mat.Scaling<Mat4> (0.01f, 0.01f) * Mat.Translation<Mat4> (0f, 0f, -30f) }, false, false)
				.Smoothen (0.9f)
				.Center ();
		}

		public static Geometry<Vertex> Pipe ()
		{
			return Circular<Vertex>.Pie (10f, 10f, 10f.Radians (), 0f, MathHelper.Pi)
                .Inset (1.2f, 1.2f)
                .Extrude (10f, true)
				.Smoothen (0.9f)
                .Center ();
		}

		public static Geometry<Vertex> SineS ()
		{
			var range = MathHelper.PiOver2;
			var step = MathHelper.Pi / 20f;
			var contour =
				(from x in Ext.Range (-range, range, step)
				 select new Vec2 (x, x.Sin () + 1f))
				.Concat (
				from x in Ext.Range (range, -range, -step)
				select new Vec2 (x, x.Sin () - 1f)).ToArray ();
			return Polygon<Vertex>.FromVec2s (contour)
				.Extrude (2f, true)
				.Smoothen (0.9f);
		}

		public static Path<PathNode, Vec3> NoseProfile ()
		{
			var spline = BSpline<Vec3>.FromControlPoints (2,
				new Vec3 (-1f, -0f, 0f),
				new Vec3 (0f, 0.5f, 0f),
				new Vec3 (1f, 0.6f, 0f) );
			return Path<PathNode, Vec3>.FromBSpline (spline, 8);
		}

		public static Path<PathNode, Vec3> HullCrossSection (Vec3 start, float top, int nodeCount)
		{
			start *= new Vec3 (2f, 1f, 1f);
			var cPoints = new Vec3[]
			{
				start, 
				new Vec3 (start.X, top * 0.4f, start.Z),
				new Vec3 (start.X * 0.5f, top * 0.8f, start.Z),
			};
			var spline = BSpline<Vec3>.FromControlPoints (2, 
				cPoints.Append (new Vec3 (0f, top * 1.5f, start.Z))
				.Concat (cPoints.Select (v => new Vec3 (-v.X, v.Y, v.Z)).Reverse ())
				.ToArray ());
			return Path<PathNode, Vec3>.FromBSpline (spline, nodeCount - 1);
		}
	}
}

