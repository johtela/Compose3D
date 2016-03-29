namespace Compose3D.SceneGraph
{
	using DataStructures;
	using Maths;
	using Extensions;
	using Textures;

	/// <summary>
	/// Global lighting parameters affecting the whole scene.
	/// </summary>
	public class GlobalLighting
	{
		public Vec3 AmbientLightIntensity;
		public float MaxIntensity;
		public float GammaCorrection;
		public Texture DiffuseMap;
	}

	/// <summary>
	/// Base class for non-ambient light sources.
	/// </summary>
	public abstract class Light : SceneNode
	{
		public Vec3 Intensity;

		public Light (SceneGraph graph) : base (graph) { }
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
			Direction = direction.Normalized;
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
			var cameraToLight = WorldToLight * camera.CameraToWorld;
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