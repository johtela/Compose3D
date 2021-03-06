﻿namespace Compose3D.SceneGraph
{
	using Maths;

	public interface ITransformNode
	{
		Vec3 Offset { get; set; }
		Vec3 Orientation { get; set; }
		Vec3 Scale { get; set; }
	}

	public class TransformNode<T> : NodeWrapper<T>, ITransformNode
		where T : SceneNode
	{
		private Mat4 _transform;
		private Vec3 _offset;
		private Vec3 _orientation;
		private Vec3 _scale;

		public TransformNode (SceneGraph graph, T node, Vec3 offset, Vec3 orientation, Vec3 scale)
			: base (graph, node)
		{
			_offset = offset;
			_orientation = orientation;
			_scale = scale;
			UpdateTransform ();
		}

		private void UpdateTransform ()
		{
			var attachedToRoot = Graph.Root == Root;

			if (attachedToRoot)
				foreach (var node in Traverse ())
					node.RemoveFromIndex ();
			_transform = Mat.Translation<Mat4> (Offset.X, Offset.Y, Offset.Z) *
				Mat.Scaling<Mat4> (Scale.X, Scale.Y, Scale.Z) *
				Mat.RotationZ<Mat4> (Orientation.Z) *
				Mat.RotationY<Mat4> (Orientation.Y) *
				Mat.RotationX<Mat4> (Orientation.X);
			if (attachedToRoot)
				foreach (var node in Traverse ())
					node.AddToIndex ();
		}

		public override Mat4 Transform
		{
			get { return base.Transform * _transform; }
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
	}
}