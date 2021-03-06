﻿namespace Compose3D.Geometry
{
    using Maths;

    public interface ITransformable<T, M>
		where T : ITransformable<T, M>
		where M : struct, ISquareMat<M, float>
	{
		T Transform (M matrix);
		T ReverseWinding ();
	}
	
	public static class Transformable
	{
		public static T Translate<T, M> (this ITransformable<T, M> transformable, params float[] offsets)
			where T : ITransformable<T, M>
			where M : struct, ISquareMat<M, float>
		{
			return transformable.Transform (Mat.Translation<M> (offsets));
		}

		public static T Scale<T, M> (this ITransformable<T, M> transformable, params float[] factors)
			where T : ITransformable<T, M>
			where M : struct, ISquareMat<M, float>
		{
			return transformable.Transform (Mat.Scaling<M> (factors));
		}

		public static T RotateX<T, M> (this ITransformable<T, M> transformable, float angle)
			where T : ITransformable<T, M>
			where M : struct, ISquareMat<M, float>
		{
			return transformable.Transform (Mat.RotationX<M> (angle));
		}

		public static T RotateY<T, M> (this ITransformable<T, M> transformable, float angle)
			where T : ITransformable<T, M>
			where M : struct, ISquareMat<M, float>
		{
			return transformable.Transform (Mat.RotationY<M> (angle));
		}
		
		public static T RotateZ<T, M> (this ITransformable<T, M> transformable, float angle)
			where T : ITransformable<T, M>
			where M : struct, ISquareMat<M, float>
		{
			return transformable.Transform (Mat.RotationZ<M> (angle));
		}

		public static T ReflectX<T, M> (this ITransformable<T, M> transformable)
			where T : ITransformable<T, M>
			where M : struct, ISquareMat<M, float>
		{
			return transformable.Scale (-1f, 1f, 1f).ReverseWinding ();
		}

		public static T ReflectY<T, M> (this ITransformable<T, M> transformable)
			where T : ITransformable<T, M>
			where M : struct, ISquareMat<M, float>
		{
			return transformable.Scale (1f, -1f, 1f).ReverseWinding ();
		}

		public static T ReflectZ<T, M> (this ITransformable<T, M> transformable)
			where T : ITransformable<T, M>
			where M : struct, ISquareMat<M, float>
		{
			return transformable.Scale (1f, 1f, -1f).ReverseWinding ();
		}

	}
}

