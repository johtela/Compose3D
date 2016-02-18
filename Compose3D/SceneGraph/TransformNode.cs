namespace Compose3D.SceneGraph
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Compose3D.Maths;
	using DataStructures;
	using Extensions;

	public class TransformNode : SceneNodeWrapper
	{
		private Mat4 _transform;
		private Vec3 _offset;
		private Vec3 _orientation;
		private Vec3 _scale;

		public TransformNode (SceneNode node, Vec3 offset, Vec3 orientation, Vec3 scale)
			: base (node)
		{
			_offset = offset;
			_orientation = orientation;
			_scale = scale;
			UpdateTransform ();
		}

		private void UpdateTransform ()
		{
			_transform = Mat.Translation<Mat4> (Offset.X, Offset.Y, Offset.Z) *
				Mat.Scaling<Mat4> (Scale.X, Scale.Y, Scale.Z) *
				Mat.RotationZ<Mat4> (Orientation.Z) *
				Mat.RotationY<Mat4> (Orientation.Y) *
				Mat.RotationX<Mat4> (Orientation.X);
		}
		
		public override IEnumerable<Tuple<SceneNode, Mat4>> Traverse (Func<SceneNode, Mat4, bool> predicate,
			Mat4 transform) 
		{
			transform *= _transform;
			var current = Tuple.Create ((SceneNode)this, transform);
			return predicate (current.Item1, current.Item2) ? 
				Node.Traverse (predicate, transform).Append (current) :
				Enumerable.Empty<Tuple<SceneNode, Mat4>> ();
		}
		
		public Vec3 Offset 
		{ 
			get { return _offset; }
			set
			{
				_offset = value;
				UpdateTransform ();
			}
		}
		public Vec3 Orientation
		{ 
			get { return _orientation; }
			set
			{
				_orientation = value;
				UpdateTransform ();
			}
		}

		public Vec3 Scale
		{ 
			get { return _scale; }
			set
			{
				_scale = value;
				UpdateTransform ();
			}
		}

		public override Aabb<Vec3> BoundingBox
		{
			get { return _transform * Node.BoundingBox; }
		}
	}
}