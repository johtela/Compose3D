﻿namespace Compose3D.SceneGraph
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Compose3D.Maths;
	using DataStructures;

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
		
		public override void Traverse<T> (Action<T, Mat4> action, Mat4 transform)
		{
			base.Traverse<T> (action, transform * _transform);
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