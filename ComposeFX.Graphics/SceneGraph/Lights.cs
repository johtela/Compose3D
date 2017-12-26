namespace ComposeFX.Graphics.SceneGraph
{
	using DataStructures;
	using Maths;
	using Extensions;
	using Shaders;
	using Textures;
	using System.Linq;

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

		public Mat4 CameraToShadowProjection (Camera camera)
		{
			var extent = MaxShadowDepth * 0.5f;
			var lightLookAt = -DirectionInCameraSpace (camera);
			var camToLight = Mat.Translation<Mat4> (0f, 0f, -extent)
				* Mat.LookAt (lightLookAt, new Vec3 (0f, 1f, 0f))
				* Mat.Translation<Mat4> (0f, 0f, extent);
			var shadowFrustum = new ViewingFrustum (FrustumKind.Orthographic, MaxShadowDepth, MaxShadowDepth,
				0f, -MaxShadowDepth);
			return shadowFrustum.CameraToScreen * camToLight;
		}

		public Mat4[] CameraToCsmProjections (Camera camera, int count)
		{
			var camToLight = Mat.LookAt (-DirectionInCameraSpace (camera), new Vec3 (0f, 1f, 0f));
			return (from frustum in camera.SplitFrustumsForCascadedShadowMaps (count, 0.75f)
					let corners = frustum.Corners.Map (p => camToLight.Transform (p))
					let orthoBox = ViewingFrustum.FromBBox (Aabb<Vec3>.FromPositions (corners))
					select orthoBox.CameraToScreen * camToLight)
				   .ToArray ();
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