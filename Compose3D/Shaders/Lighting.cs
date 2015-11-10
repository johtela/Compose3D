namespace Compose3D.Shaders
{
	using System;
	using Compose3D.Maths;
	using Compose3D.GLTypes;

	public static class Lighting
	{
		[GLStruct ("DirectionalLight")]
		public struct DirectionalLight
		{
			public Vec3 intensity;
			public Vec3 direction;
		}

		[GLStruct ("PointLight")]
		public struct PointLight
		{
			public Vec3 position;
			public Vec3 intensity;
			public float linearAttenuation, quadraticAttenuation;
		}

		[GLStruct ("SpotLight")]
		public struct SpotLight
		{
			public PointLight pointLight;
			public Vec3 direction;
			public float cosSpotCutoff, spotExponent;
		}

		[GLStruct ("GlobalLight")]
		public struct GlobalLight
		{
			public Vec3 ambientLightIntensity;
			public float maxintensity;
			public float inverseGamma;
		}

		/// <summary>
		/// The calculate intensity of the directional light.
		/// </summary>
		public static readonly Func<DirectionalLight, Vec3, Vec3> DirectionalLightIntensity =
			GLShader.Function 
			(
				() => DirectionalLightIntensity,

				(DirectionalLight dirLight, Vec3 normal) => 
				(dirLight.intensity * normal.Dot (dirLight.direction)).Clamp (0f, 1f)
			);

		/// <summary>
		/// Calculate attenuation of a point light.
		/// </summary>
		public static readonly Func<PointLight, float, float> Attenuation = 
			GLShader.Function 
			(
				() => Attenuation, 

				(PointLight pointLight, float distance) => 
				(1f / ((pointLight.linearAttenuation * distance) + 
					(pointLight.quadraticAttenuation * distance * distance))).Clamp (0f, 1f)
			);

		/// <summary>
		/// Calculate intensity of a point light.
		/// </summary>
		public static readonly Func<PointLight, Vec3, Vec3, Vec3, Vec3, float, Vec3> PointLightIntensity =
			GLShader.Function
			(
				() => PointLightIntensity,

				(PointLight pointLight, Vec3 position, Vec3 normal, Vec3 diffuse, Vec3 specular, float shininess) =>
				Shader.Evaluate
				(
					from vecToLight in (pointLight.position - position).ToShader ()
					let lightVec = vecToLight.Normalized
					let cosAngle = lightVec.Dot (normal).Clamp (0f, 1f)
					let attenIntensity = Attenuation (pointLight, vecToLight.Length) * pointLight.intensity
					let viewDir = -position.Normalized
					let halfAngle = (lightVec + viewDir).Normalized
					let blinn = cosAngle == 0f ? 0f :
					normal.Dot (halfAngle).Clamp (0f, 1f).Pow (shininess)
					select (diffuse * attenIntensity * cosAngle) + (specular * attenIntensity * blinn)
				)
			);

		//		public static readonly Func<SpotLight, Vec3, Vec3> CalcSpotLight = 
		//			GLShader.Function (() => CalcSpotLight,
		//				(spotLight, position) => 
		//				(from vecToLight in (spotLight.pointLight.position - position).ToShader ()
		//				 let dist = vecToLight.Length
		//				 let lightDir = vecToLight.Normalized
		//				 let attenuation = CalcAttenuation (spotLight.pointLight, dist)
		//				 let cosAngle = (-lightDir).Dot (spotLight.direction)
		//				 select spotLight.pointLight.intensity *
		//				     (cosAngle < spotLight.cosSpotCutoff ? 0f : attenuation * cosAngle.Pow (spotLight.spotExponent)))
		//				.Evaluate ());

		public static readonly Func<GlobalLight, Vec3, Vec3, Vec3> GlobalLightIntensity =
			GLShader.Function
			(
				() => GlobalLightIntensity,

				(GlobalLight globalLight, Vec3 diffuse, Vec3 other) =>
				Shader.Evaluate
				(
					from gamma in new Vec3 (globalLight.inverseGamma).ToShader ()
					let maxInten = globalLight.maxintensity
					let ambient = diffuse * globalLight.ambientLightIntensity
					select ((ambient + other).Pow (gamma) / maxInten).Clamp (0f, 1f)
				)
			);

		public static void Use () { }
	}
}
