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

		[GLStruct ("DiffuseAndSpecular")]
		public struct DiffuseAndSpecular
		{
			public Vec3 diffuse;
			public Vec3 specular;

			[GLConstructor ("DiffuseAndSpecular ({0})")]
			public DiffuseAndSpecular (Vec3 diff, Vec3 spec)
			{
				diffuse = diff;
				specular = spec;
			}
		}

		public static readonly Func<Vec3, Vec3, Vec3, Vec3> LightDiffuseIntensity =
			GLShader.Function
			(
				() => LightDiffuseIntensity,
				(Vec3 lightDir, Vec3 intensity, Vec3 normal) =>
					intensity * normal.Dot (lightDir).Clamp (0f, 1f)
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

		public static readonly Func<DirectionalLight, Vec3, Vec3, float, DiffuseAndSpecular> DirLightIntensity =
			GLShader.Function
			(
				() => DirLightIntensity,
				(DirectionalLight dirLight, Vec3 position, Vec3 normal, float shininess) =>
					new DiffuseAndSpecular (
						LightDiffuseIntensity (dirLight.direction, dirLight.intensity, normal),
						LightSpecularIntensity (dirLight.direction, dirLight.intensity, position, normal, shininess))
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
		public static readonly Func<PointLight, Vec3, Vec3, float, DiffuseAndSpecular> PointLightIntensity =
			GLShader.Function
			(
				() => PointLightIntensity,
				(PointLight pointLight, Vec3 position, Vec3 normal, float shininess) =>
				Shader.Evaluate
				(
					from vecToLight in (pointLight.position - position).ToShader ()
					let lightDir = vecToLight.Normalized
					let diffuse = LightDiffuseIntensity (lightDir, pointLight.intensity, normal)
					let specular = LightSpecularIntensity (lightDir, pointLight.intensity, position, normal, shininess)
					let attenuation = Attenuation (pointLight, vecToLight.Length)
					select new DiffuseAndSpecular (diffuse * attenuation, specular * attenuation)
				)
			);

		/// <summary>
		/// Calculate the global light intensity given the global lightin parameters
		/// and the diffuse and other color coefficents of a vertex.
		/// </summary>
		public static readonly Func<GlobalLight, Vec3, Vec3, Vec3, Vec3, Vec3> GlobalLightIntensity =
			GLShader.Function
			(
				() => GlobalLightIntensity,

				(GlobalLight globalLighting, Vec3 diffuseLight, Vec3 specularLight, Vec3 diffuseColor, Vec3 specularColor) =>
				Shader.Evaluate
				(
					from gamma in new Vec3 (globalLighting.inverseGamma).ToShader ()
					let maxInten = globalLighting.maxintensity
					let ambient = diffuseColor * globalLighting.ambientLightIntensity
					select ((ambient + (diffuseLight * diffuseColor) + (specularLight * specularColor))
						.Pow (gamma) / maxInten).Clamp (0f, 1f)
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
