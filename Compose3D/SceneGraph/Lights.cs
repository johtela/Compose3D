namespace Compose3D.SceneGraph
{
	using Compose3D.Maths;
	using Geometry;
	using System;

	public class GlobalLighting : SceneNode
	{
		public GlobalLighting (Vec3 ambientLightIntensity, float maxIntensity, float gammaCorrection)
		{
			AmbientLightIntensity = ambientLightIntensity;
			MaxIntensity = maxIntensity;
			GammaCorrection = gammaCorrection;
		}

		public Vec3 AmbientLightIntensity { get; set; }
		public float MaxIntensity { get; set; }
		public float GammaCorrection { get; set; }
	}

	public abstract class Light : SceneNode
	{
		public Vec3 Intensity { get; set; }
	}

	public class DirectionalLight : Light
	{
		public DirectionalLight (Vec3 intensity, Vec3 direction)
		{
			Intensity = intensity;
			Direction = direction;
		}

		public Vec3 Direction { get; set; }
	}

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