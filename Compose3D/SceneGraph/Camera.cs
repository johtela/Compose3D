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
		private ViewFrustrum _frustrum;
		private float _aspectRatio;		// height / width
		private Mat4 _perspectiveTransform;
		
		public Camera (Vec3 position, Vec3 target, Vec3 upDirection, ViewFrustrum frustrum, float aspectRatio)
		{
			_position = position;
			_target = target;
			_upDirection = upDirection;
			UpdateViewTransform ();
			_frustrum = frustrum;
			_aspectRatio = aspectRatio;
			UpdatePerspectiveTransform ();
		}		
		
		private void UpdateViewTransform ()
		{
			_viewTransform = Mat.LookAt (_position, _target, _upDirection);
		}

		private void UpdatePerspectiveTransform ()
		{
			_perspectiveTransform = Mat.Scaling<Mat4> (_aspectRatio, 1f, 1f) *
				Mat.PerspectiveProjection (_frustrum.Left, _frustrum.Right, _frustrum.Bottom, _frustrum.Top, 
				_frustrum.Near, _frustrum.Far);
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

		public ViewFrustrum Frustrum
		{
			get { return _frustrum; }
			set
			{
				_frustrum = value;
				UpdatePerspectiveTransform ();
			}
		}

		public float AspectRatio
		{
			get { return _aspectRatio; }
			set
			{
				_aspectRatio = value;
				UpdatePerspectiveTransform ();
			}
		}

		public Mat4 PerspectiveTransform
		{
			get { return _perspectiveTransform; }
		}
	}
}