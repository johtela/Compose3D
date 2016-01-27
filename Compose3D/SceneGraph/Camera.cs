namespace Compose3D.SceneGraph
{
	using System;
	using Maths;

	public class Camera : SceneNode
	{
		private Mat4 _viewTransform;
		private Vec3 _position;
		private Vec3 _target;
		private Vec3 _upDirection;
		private ViewingFrustum _frustrum;
		private Mat4 _perspectiveTransform;
		
		public Camera (Vec3 position, Vec3 target, Vec3 upDirection, ViewingFrustum frustrum, float aspectRatio)
		{
			_position = position;
			_target = target;
			_upDirection = upDirection;
			UpdateViewTransform ();
			_frustrum = frustrum;
			UpdatePerspectiveTransform ();
		}		
		
		private void UpdateViewTransform ()
		{
			_viewTransform = Mat.LookAt (_position, _target, _upDirection);
		}

		private void UpdatePerspectiveTransform ()
		{
			_perspectiveTransform = Mat.PerspectiveProjection (-_frustrum.HalfWidth, _frustrum.HalfWidth, 
				-_frustrum.HalfHeight, _frustrum.HalfHeight, _frustrum.Near, _frustrum.Far);
		}
		
		public Mat4 Transform 
		{
			get { return _viewTransform; }
		}
		
		public Vec3 Position 
		{ 
			get { return _position; } 
			set 
			{ 
				_position = value; 
				UpdateViewTransform ();
			}
		}

		public Vec3 Target
		{ 
			get { return _target; } 
			set 
			{ 
				_target = value; 
				UpdateViewTransform ();
			}
		}

		public Vec3 UpDirection
		{ 
			get { return _upDirection; } 
			set 
			{ 
				_upDirection = value; 
				UpdateViewTransform ();
			}
		}

		public ViewingFrustum Frustrum
		{
			get { return _frustrum; }
			set
			{
				_frustrum = value;
				UpdatePerspectiveTransform ();
			}
		}

		public Mat4 PerspectiveTransform
		{
			get { return _perspectiveTransform; }
		}
	}
}