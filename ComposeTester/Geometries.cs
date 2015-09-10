namespace ComposeTester
{
	using Compose3D.Arithmetics;
	using Compose3D.Geometry;
	using Compose3D.GLTypes;
	using OpenTK;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class Geometries
	{
		private static IMaterial NewMat ()
		{
			return Material.RepeatColors (Color.Random);
		}

		public static Geometry<Vertex> Hammer ()
		{
			var cube1 = Volume.Cube<Vertex> (1f, 1.5f, 2f, NewMat ()).Rotate (0f, MathHelper.PiOver2, 0f);
			var cube2 = Volume.Cube<Vertex> (1f, 1f, 1f, NewMat ()).Scale (0.8f, 0.8f, 0.8f);
			var cube3 = Volume.Cube<Vertex> (1f, 1f, 2f, NewMat ());
			return Composite.Create (Stacking.StackRight (cube1, cube2, cube3)
				.Align (Alignment.None, Alignment.Center, Alignment.Center)).Center ();
		}

		private static Geometry<Vertex> Roof (out int tag)
		{
			var trapezoid = Quadrilateral<Vertex>.Trapezoid (20f, 1f, 0f, 1f, NewMat ());
			tag = trapezoid.TagVertex (trapezoid.Vertices.Bottommost ().Rightmost ().Single ());
			var leftPane = trapezoid.Extrude (30f).Rotate (0f, 0f, MathHelper.PiOver4);
			var rightPane = leftPane.ReflectX ();
			return Composite.Create (Stacking.StackRight (leftPane, rightPane));
		}

		private static Geometry<Vertex> Gables (Geometry<Vertex> roof, out int tag)
		{
			var gableHeight = roof.BoundingBox.Size.Y * 0.85f;
			var frontGable = Triangle<Vertex>.Isosceles (2 * gableHeight, gableHeight, NewMat ());
			tag = frontGable.TagVertex (frontGable.Vertices.Topmost ().Single ());
			var backGable = frontGable.ReflectZ ().Translate (0f, 0f, -roof.BoundingBox.Size.Z * 0.9f);
			return Composite.Create (frontGable, backGable);
		}

		private static Geometry<Vertex> WallsAndGables (Geometry<Vertex> roof, Geometry<Vertex> gables, 
			int roofSnapTag, int gableTopTag)
		{
			var walls = Quadrilateral<Vertex>.Rectangle (gables.BoundingBox.Size.X, gables.BoundingBox.Size.Z, NewMat ())
				.Extrude (12f, false, false).Rotate (MathHelper.PiOver2, 0f, 0f);
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
				res = Mat.RotationX<Mat4> (10f.ToRadians ()) * Mat.Translation<Mat4> (0f, 0f, -10f) * res;
				yield return res;
			}
		}

		public static Geometry<Vertex> Tube ()
		{
			return Circular<Vertex>.Pie (10f, 10f, 10f.ToRadians (), 0f, 270f.ToRadians (), NewMat ())
				.Stretch (10, true, true, true, TubeTransforms ()).Center ();
		}

		public static Geometry<Vertex> Arrow ()
		{
			return Circular<Vertex>.Circle (10f, 10f.ToRadians (), NewMat ())
				.Stretch (1, false, false, true, 
					new Mat4[] { Mat.Scaling<Mat4> (0.1f, 0.1f, 1f, 1f) * Mat.Translation<Mat4> (0f, 0f, -30f) })
				.Center ();
		}

		public static Geometry<Vertex> Pipe ()
		{
			return Circular<Vertex>.Circle (10f, 10f.ToRadians (), NewMat ())
				.Hollow (1.2f, 1.2f)
				.Extrude (10f, true, true)
				.Center ();
		}
	}
}

