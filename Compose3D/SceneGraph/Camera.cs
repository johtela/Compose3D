namespace Compose3D.SceneGraph
{
	using System;
	using Maths;

	public class Camera : SceneNode
	{
		private Mat4 _transform;
		private Vec3 _position;
		private Vec3 _target;
		private Vec3 _upDirection;
		
		public Camera (Vec3 position, Vec3 target, Vec3 upDirection)
		{
			_position = position;
			_target = target;
			_upDirection = upDirection;
			UpdateTransform ();
		}		
		
		private void UpdateTransform ()
		{
			_transform = Mat.LookAt (_position, _target, _upDirection);
		}
		
		public override void Traverse<T> (Action<T, Mat4> action, Mat4 transform)
		{
			base.Traverse<T> (action, transform * _transform);			
		}
		
		public Vec3 Position 
		{ 
			get { return _position; } 
			set 
			{ 
				_position = value; 
				UpdateTransform ();
			}
		}

		public Vec3 Target
		{ 
			get { return _target; } 
			set 
			{ 
				_target = value; 
				UpdateTransform ();
			}
		}

		public Vec3 UpDirection
		{ 
			get { return _upDirection; } 
			set 
			{ 
				_upDirection = value; 
				UpdateTransform ();
			}
		}
	}
}