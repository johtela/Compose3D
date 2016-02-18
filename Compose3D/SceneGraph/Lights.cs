namespace Compose3D.SceneGraph
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DataStructures;
	using Maths;
	using Extensions;

	/// <summary>
	/// Global lighting parameters affecting the whole scene.
	/// </summary>
	public class GlobalLighting : SceneNode
	{
		public GlobalLighting (Vec3 ambientLightIntensity, float maxIntensity, float gammaCorrection)
		{
			AmbientLightIntensity = ambientLightIntensity;
			MaxIntensity = maxIntensity;
			GammaCorrection = gammaCorrection;
		}

		public override Aabb<Vec3> BoundingBox
		{
			get { return null; }
		}

		public Vec3 AmbientLightIntensity { get; set; }
		public float MaxIntensity { get; set; }
		public float GammaCorrection { get; set; }
	}

	/// <summary>
	/// Base class for non-ambient light sources.
	/// </summary>
	public abstract class Light : SceneNode
	{
		public Vec3 Intensity { get; set; }

		public override Aabb<Vec3> BoundingBox
		{
			get { return null; }
		}
	}

	/// <summary>
	/// Directional light that has no position.
	/// </summary>
	public class DirectionalLight : Light
	{
		private Vec3 _direction;
		private float _distance;
		private Mat4? _worldToLight;
		
		public DirectionalLight (Vec3 intensity, Vec3 direction, float distance)
		{
			Intensity = intensity;
			_direction = direction;
			_distance = distance;
		}
		
		public float Distance
		{ 
			get { return _distance; }
			set
			{
				_distance = value;
				_worldToLight = null;
			} 
		}

		public Vec3 Direction 
		{ 
			get { return _direction; } 
			set
			{
				_direction = value;
				_worldToLight = null;
			}
		}
		
		public Mat4 WorldToLight
		{
			get 
			{ 
				if (!_worldToLight.HasValue)
					_worldToLight = Mat.Translation<Mat4> (0f, 0f, -_distance) *
						Mat.RotationY<Mat4> (-_direction.YRotation ()) *
						Mat.RotationX<Mat4> (-_direction.XRotation ());
				return _worldToLight.Value; 
			}
		}
		
		public ViewingFrustum ShadowFrustum (Camera camera)
		{
			var cameraToLight = WorldToLight * camera.WorldToCamera.Inverse;
			var bbox = Aabb<Vec3>.FromPositions (camera.Frustum.Corners.Map (c => cameraToLight.Transform (c)));
			return new ViewingFrustum (FrustumKind.Orthographic, bbox.Left, bbox.Right, bbox.Bottom, bbox.Top,
				1f, bbox.Size.Z + _distance);
		}
	}

	/// <summary>
	/// Point light source emitting light to all directions equally.
	/// </summary>
	public class PointLight : Light
	{
		public PointLight (Vec3 intensity, Vec3 position, float linearAttenuation, 
			float quadraticAttenuation)
		{
			Intensity = intensity;
			Position = position;
			LinearAttenuation = linearAttenuation;
			QuadraticAttenuation = quadraticAttenuation;
		}
	
		public Vec3 Position { get; set; }
		public float LinearAttenuation { get; set; }
		public float QuadraticAttenuation { get; set; }
	}
}