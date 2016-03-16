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


		public static readonly Func<Vec3, Vec3, Vec3, Vec3> LightDiffuseIntensity =
			GLShader.Function
			(
				() => LightDiffuseIntensity,
				(Vec3 lightDir, Vec3 intensity, Vec3 normal) =>
					intensity * normal.Dot (lightDir).Clamp (0f, 1f)
			);


		public static readonly Func<DirectionalLight, Vec3, Vec3> DirLightDiffuseIntensity =
			GLShader.Function
			(
				() => DirLightDiffuseIntensity,
				(DirectionalLight dirLight, Vec3 normal) => 
					LightDiffuseIntensity (dirLight.direction, dirLight.intensity, normal)
			);

		public static readonly Func<Vec3, Vec3, Vec3, Vec3, float, Vec3> LightSpecularIntensity =
			GLShader.Function
			(
				() => LightSpecularIntensity,
				(Vec3 lightDir, Vec3 intensity, Vec3 position, Vec3 normal, float shininess) =>
				Shader.Evaluate
				(
					from cosAngle in lightDir.Dot (normal).Clamp (0f, 1f).ToShader ()
					let viewDir = -position.Normalized
					let halfAngle = (lightDir + viewDir).Normalized
					let blinn = cosAngle == 0f ? 0f : normal.Dot (halfAngle).Clamp (0f, 1f).Pow (shininess)
					select intensity * blinn
				)
			);

		public static readonly Func<DirectionalLight, Vec3, Vec3, float, Vec3> DirLightSpecularIntensity =
			GLShader.Function
			(
				() => DirLightSpecularIntensity,
				(DirectionalLight dirLight, Vec3 position, Vec3 normal, float shininess) =>
					LightSpecularIntensity (dirLight.direction, dirLight.intensity, position, normal, shininess)
			);

		/// <summary>
		/// Calculate attenuation of a point light given the light source and distance
		/// to the vertex.
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
		/// Calculate intensity of a point light given the light source and the vertex position,
		/// normal, and color attributes. Uses Blinn shading for specular highlights.
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

		/// <summary>
		/// Calculate the global light intensity given the global lightin parameters
		/// and the diffuse and other color coefficents of a vertex.
		/// </summary>
		public static readonly Func<GlobalLight, Vec3, Vec3, Vec3> GlobalLightIntensity =
			GLShader.Function
			(
				() => GlobalLightIntensity,

				(GlobalLight globalLight, Vec3 diffuseColor, Vec3 directionalAndSpecular) =>
				Shader.Evaluate
				(
					from gamma in new Vec3 (globalLight.inverseGamma).ToShader ()
					let maxInten = globalLight.maxintensity
					let ambient = diffuseColor * globalLight.ambientLightIntensity
					select ((ambient + directionalAndSpecular).Pow (gamma) / maxInten).Clamp (0f, 1f)
				)
			);
		
		public static readonly Func<float, float, float, float> FogVisibility =
			GLShader.Function
			(
				() => FogVisibility,
				
				(float distance, float density, float gradient) =>
					1f - GLMath.Exp (-(distance * density).Pow (gradient))
			);
				
		/// <summary>
		/// Use this module. This function needs to be called once for static field initialization of
		/// this class.
		/// </summary>
		public static void Use () { }
	}
}
