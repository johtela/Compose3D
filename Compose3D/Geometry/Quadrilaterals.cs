namespace Compose3D.Geometry
{
    using Arithmetics;
    using OpenTK;
    using System;
    using System.Collections.Generic;

	[Flags]
	public enum CubeFaces
	{
		Front = 1,
		Back = 2,
		Top = 4,
		Bottom = 8,
		Left = 16,
		Right = 32,
		All = 63
	}
	
	public static class Quadrilaterals
	{
		private static IEnumerable<Geometry<V>> GetFaces<V> (float width, float height, float depth, CubeFaces faces) 
			where V : struct, IVertex
		{
			var right = width / 2f;
			var left = -right;
			var top = height / 2f;
			var bottom = -top;
			var front = depth / 2f;
			var back = -front;

			if (faces.HasFlag (CubeFaces.Front)) yield return FrontFace<V> (width, height, front);
			if (faces.HasFlag (CubeFaces.Back)) yield return BackFace<V> (width, height, back);
			if (faces.HasFlag (CubeFaces.Top)) yield return TopFace<V> (width, depth, top);
			if (faces.HasFlag (CubeFaces.Bottom)) yield return BottomFace<V> (width, depth, bottom);
			if (faces.HasFlag (CubeFaces.Left)) yield return LeftFace<V> (depth, height, left);
			if (faces.HasFlag (CubeFaces.Right)) yield return RightFace<V> (depth, height, right);
		}

		public static Geometry<V> Cube<V> (float width, float height, float depth, CubeFaces faces) 
			where V : struct, IVertex
		{
			return Composite.Create<V> (GetFaces<V> (width, height, depth, faces));
		}

		public static Geometry<V> Cube<V> (float width, float height, float depth) where V : struct, IVertex
		{
			return Cube<V> (width, height, depth, CubeFaces.All);
		}

		public static Geometry<V> FrontFace<V> (float width, float height, float offset) where V : struct, IVertex
		{
			return Geometry.Rectangle<V> (width, height).Transform (Mat.Translation<Mat4> (0f, 0f, offset));
		}

		public static Geometry<V> BackFace<V> (float width, float height, float offset) where V : struct, IVertex
		{
            return Geometry.Rectangle<V> (width, height).Transform (Mat.Translation<Mat4> (0f, 0f, offset) *
				Mat.RotationX<Mat4> (MathHelper.Pi));
		}

		public static Geometry<V> TopFace<V> (float width, float depth, float offset) where V : struct, IVertex
		{
			return Geometry.Rectangle<V> (width, depth).Transform (Mat.Translation<Mat4> (0f, offset, 0f) *
                Mat.RotationX<Mat4> (-MathHelper.PiOver2));
		}

		public static Geometry<V> BottomFace<V> (float width, float depth, float offset) where V : struct, IVertex
		{
			return Geometry.Rectangle<V> (width, depth).Transform (Mat.Translation<Mat4> (0f, offset, 0f) *
                Mat.RotationX<Mat4> (MathHelper.PiOver2));
		}

		public static Geometry<V> LeftFace<V> (float depth, float height, float offset) where V : struct, IVertex
		{
			return Geometry.Rectangle<V> (depth, height).Transform (Mat.Translation<Mat4> (offset, 0f, 0f) *
                Mat.RotationY<Mat4> (-MathHelper.PiOver2));
		}
		
		public static Geometry<V> RightFace<V> (float depth, float height, float offset) where V : struct, IVertex
		{
			return Geometry.Rectangle<V> (depth, height).Transform (Mat.Translation<Mat4> (offset, 0f, 0f) *
                Mat.RotationY<Mat4> (MathHelper.PiOver2));
		}
	}
}
