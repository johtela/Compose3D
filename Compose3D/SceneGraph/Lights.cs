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
		public Texture ShadowMap;
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
		public float MaxShadowDepth;
		
		public DirectionalLight (SceneGraph graph, Vec3 intensity, Vec3 direction, float maxShadowDepth)
			: base (graph)
		{
			Intensity = intensity;
			MaxShadowDepth = maxShadowDepth;
			Direction = direction.Normalized;
		}

		public Vec3 DirectionInCameraSpace (Camera camera)
		{
			return new Mat3 (camera.WorldToCamera) * Direction;
		}

		public Mat4 CameraToLightSpace (Camera camera)
		{
			var cf = camera.Frustum;
			var extent = MaxShadowDepth / 2f;
			var target = new Vec3 ((cf.Right + cf.Left) / 2f, (cf.Top + cf.Bottom) / 2f, -extent);
			var camDir = DirectionInCameraSpace (camera);
			var eye = camDir * extent + target;
			return Mat.LookAt (eye, target, new Vec3 (0f, 1f, 0f));
		}

		public Mat4 CameraToShadowFrustum (Camera camera)
		{
			return ShadowFrustum (camera).CameraToScreen * CameraToLightSpace (camera);
		}
		
		public ViewingFrustum ShadowFrustum (Camera camera)
		{
			return new ViewingFrustum (FrustumKind.Orthographic, MaxShadowDepth, MaxShadowDepth, 0f, MaxShadowDepth);
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