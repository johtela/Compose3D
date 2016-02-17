namespace Compose3D.SceneGraph
{
	using System;
	using DataStructures;
	using Maths;

	public class Camera : SceneNode
	{
		private Mat4 _worldToCamera;
		private Mat4 _cameraToWorld;
		private Vec3 _position;
		private Vec3 _target;
		private Vec3 _upDirection;
		private ViewingFrustum _frustrum;
		
		public Camera (Vec3 position, Vec3 target, Vec3 upDirection, ViewingFrustum frustum, float aspectRatio)
		{
			_position = position;
			_target = target;
			_upDirection = upDirection;
			_frustrum = frustum;
			UpdateCameraTransform ();
		}

		private void UpdateCameraTransform ()
		{
			_worldToCamera = Mat.LookAt (_position, _target, _upDirection);
			_cameraToWorld = _worldToCamera.Inverse;
		}

		public override Aabb<Vec3> BoundingBox
		{
			get { return null; }
		}

		public Mat4 WorldToCamera 
		{
			get { return _worldToCamera; }
		}

		public Mat4 CameraToWorld
		{
			get { return _cameraToWorld; }
		}
		
		public Vec3 Position 
		{ 
			get { return _position; } 
			set 
			{ 
				_position = value; 
				UpdateCameraTransform ();
			}
		}

		public Vec3 Target
		{ 
			get { return _target; } 
			set 
			{ 
				_target = value; 
				UpdateCameraTransform ();
			}
		}

		public Vec3 UpDirection
		{ 
			get { return _upDirection; } 
			set 
			{ 
				_upDirection = value; 
				UpdateCameraTransform ();
			}
		}

		public ViewingFrustum Frustum
		{
			get { return _frustrum; }
			set { _frustrum = value; }
		}
	}
}