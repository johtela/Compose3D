namespace Compose3D.SceneGraph
{
	using DataStructures;
	using Maths;
	using Extensions;

	/// <summary>
	/// Global lighting parameters affecting the whole scene.
	/// </summary>
	public class GlobalLighting : SceneNode
	{
		public Vec3 AmbientLightIntensity;
		public float MaxIntensity;
		public float GammaCorrection;

		public GlobalLighting (SceneGraph graph, Vec3 ambientLightIntensity, float maxIntensity, float gammaCorrection)
			: base (graph)
		{
			AmbientLightIntensity = ambientLightIntensity;
			MaxIntensity = maxIntensity;
			GammaCorrection = gammaCorrection;
		}

		public override Aabb<Vec3> BoundingBox
		{
			get { return null; }
		}
	}

	/// <summary>
	/// Base class for non-ambient light sources.
	/// </summary>
	public abstract class Light : SceneNode
	{
		public Vec3 Intensity;

		public Light (SceneGraph graph) : base (graph) { }

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
		public Vec3 Direction;
		public float Distance;
		
		public DirectionalLight (SceneGraph graph, Vec3 intensity, Vec3 direction, float distance)
			: base (graph)
		{
			Intensity = intensity;
			Distance = distance;
			Direction = direction;
		}

		public Mat4 WorldToLight
		{
			get 
			{
				return Mat.Translation<Mat4> (0f, 0f, -Distance) *
					Mat.RotationY<Mat4> (-Direction.YRotation ()) *
					Mat.RotationX<Mat4> (-Direction.XRotation ());
			}
		}
		
		public ViewingFrustum ShadowFrustum (Camera camera)
		{
			var cameraToLight = WorldToLight * camera.WorldToCamera.Inverse;
			var bbox = Aabb<Vec3>.FromPositions (camera.Frustum.Corners.Map (c => cameraToLight.Transform (c)));
			return new ViewingFrustum (FrustumKind.Orthographic, bbox.Left, bbox.Right, bbox.Bottom, bbox.Top,
				1f, bbox.Size.Z + Distance);
		}
	}

	/// <summary>
	/// Point light source emitting light to all directions equally.
	/// </summary>
	public class PointLight : Light
	{
		public Vec3 Position;
		public float LinearAttenuation;
		public float QuadraticAttenuation;

		public PointLight (SceneGraph graph, Vec3 intensity, Vec3 position, float linearAttenuation, 
			float quadraticAttenuation) : base (graph)
		{
			Intensity = intensity;
			Position = position;
			LinearAttenuation = linearAttenuation;
			QuadraticAttenuation = quadraticAttenuation;
		}
	}
}