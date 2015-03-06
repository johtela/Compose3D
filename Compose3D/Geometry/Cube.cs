namespace Compose3D.Geometry
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;
    using Arithmetics;

	[Flags]
	public enum CubeFace
	{
		Front = 1,
		Back = 2,
		Top = 4,
		Bottom = 8,
		Left = 16,
		Right = 32,
		All = 63
	}
	
	public static class Cube
	{
		private static IEnumerable<Geometry<V>> GetFaces<V> (float width, float height, float depth, CubeFace faces) 
			where V : struct, IVertex
		{
			var right = width / 2f;
			var left = -right;
			var top = height / 2f;
			var bottom = -top;
			var front = depth / 2f;
			var back = -front;

			if (faces.HasFlag (CubeFace.Front)) yield return FrontFace<V> (width, height, front);
			if (faces.HasFlag (CubeFace.Back)) yield return BackFace<V> (width, height, back);
			if (faces.HasFlag (CubeFace.Top)) yield return TopFace<V> (width, depth, top);
			if (faces.HasFlag (CubeFace.Bottom)) yield return BottomFace<V> (width, depth, bottom);
			if (faces.HasFlag (CubeFace.Left)) yield return LeftFace<V> (depth, height, left);
			if (faces.HasFlag (CubeFace.Right)) yield return RightFace<V> (depth, height, right);
		}

		public static Geometry<V> Create<V> (float width, float height, float depth, CubeFace faces) 
			where V : struct, IVertex
		{
			return Composite.Create<V> (GetFaces<V> (width, height, depth, faces));
		}

		public static Geometry<V> Create<V> (float width, float height, float depth) where V : struct, IVertex
		{
			return Create<V> (width, height, depth, CubeFace.All);
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
