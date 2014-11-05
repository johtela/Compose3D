namespace Compose3D.Geometry
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;

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
			var right = width / 2.0f;
			var left = -right;
			var top = height / 2.0f;
			var bottom = -top;
			var front = depth / 2.0f;
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
			return Geometry.Rectangle<V> (width, height).Transform (Matrix4.CreateTranslation (0.0f, 0.0f, offset));
		}

		public static Geometry<V> BackFace<V> (float width, float height, float offset) where V : struct, IVertex
		{
			return Geometry.Rectangle<V> (width, height).Transform (Matrix4.CreateRotationX (MathHelper.Pi) *
				Matrix4.CreateTranslation (0.0f, 0.0f, offset));
		}

		public static Geometry<V> TopFace<V> (float width, float depth, float offset) where V : struct, IVertex
		{
			return Geometry.Rectangle<V> (width, depth).Transform (Matrix4.CreateRotationX (-MathHelper.PiOver2) *
				Matrix4.CreateTranslation (0.0f, offset, 0.0f));
		}

		public static Geometry<V> BottomFace<V> (float width, float depth, float offset) where V : struct, IVertex
		{
			return Geometry.Rectangle<V> (width, depth).Transform (Matrix4.CreateRotationX (MathHelper.PiOver2) *
				Matrix4.CreateTranslation (0.0f, offset, 0.0f));
		}

		public static Geometry<V> LeftFace<V> (float depth, float height, float offset) where V : struct, IVertex
		{
			return Geometry.Rectangle<V> (depth, height).Transform (Matrix4.CreateRotationY (-MathHelper.PiOver2) *
				Matrix4.CreateTranslation (offset, 0.0f, 0.0f));
		}
		
		public static Geometry<V> RightFace<V> (float depth, float height, float offset) where V : struct, IVertex
		{
			return Geometry.Rectangle<V> (depth, height).Transform (Matrix4.CreateRotationY (MathHelper.PiOver2) *
				Matrix4.CreateTranslation (offset, 0.0f, 0.0f));
		}
	}
}
