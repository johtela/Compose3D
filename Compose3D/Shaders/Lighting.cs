namespace Compose3D.Shaders
{
	using System;
	using System.Linq;
	using Compose3D.Maths;
	using Compose3D.GLTypes;
	using Textures;

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
				(
					from cosAngle in lightDir.Dot (normal).Clamp (0f, 1f).ToShader ()
					let viewDir = -position.Normalized
					let halfAngle = (lightDir + viewDir).Normalized
					let blinn = cosAngle == 0f ? 0f : normal.Dot (halfAngle).Clamp (0f, 1f).Pow (shininess)
					select intensity * blinn
				)
				.Evaluate ()
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
				(
					from vecToLight in (pointLight.position - position).ToShader ()
					let lightDir = vecToLight.Normalized
					let diffuse = LightDiffuseIntensity (lightDir, pointLight.intensity, normal)
					let specular = LightSpecularIntensity (lightDir, pointLight.intensity, position, normal, shininess)
					let attenuation = Attenuation (pointLight, vecToLight.Length)
					select new DiffuseAndSpecular (diffuse * attenuation, specular * attenuation)
				)
				.Evaluate ()
			);

		/// <summary>
		/// Calculate the global light intensity given the global lightin parameters
		/// and the diffuse and other color coefficents of a vertex.
		/// </summary>
		public static readonly Func<GlobalLight, Vec3, Vec3, Vec3, Vec3, Vec3, Vec3> GlobalLightIntensity =
			GLShader.Function
			(
				() => GlobalLightIntensity,
				(GlobalLight globalLighting, Vec3 ambientLight, Vec3 diffuseLight, Vec3 specularLight, Vec3 diffuseColor, Vec3 specularColor) =>
				(
					from gamma in new Vec3 (globalLighting.inverseGamma).ToShader ()
					let maxInten = globalLighting.maxintensity
					let ambient = diffuseColor * ambientLight
					select ((ambient + (diffuseLight * diffuseColor) + (specularLight * specularColor))
						.Pow (gamma) / maxInten).Clamp (0f, 1f)
				)
				.Evaluate ()
			);
		
		public static readonly Func<float, float, float, float> FogVisibility =
			GLShader.Function
			(
				() => FogVisibility,
				(float distance, float density, float gradient) =>
					1f - GLMath.Exp (-(distance * density).Pow (gradient))
			);
		
		public static readonly Func<SamplerCube, Vec3, Vec3, Vec3> ReflectedColor =
			GLShader.Function 
			(
				() => ReflectedColor,
				(SamplerCube environmentMap, Vec3 position, Vec3 normal) =>
				(	
					from viewDir in (-position.Normalized).ToShader ()
					let reflectDir = viewDir.Reflect<Vec3, float> (normal)
					select environmentMap.Texture (reflectDir)[Coord.x, Coord.y, Coord.z]
				)
				.Evaluate ()
			);

		public static readonly Func<Vec3, float, float> RandomAngle =
			GLShader.Function
			(
				() => RandomAngle,
				(Vec3 seed, float freq) =>
				(
					from dt in (seed * freq).Floor ().Dot (new Vec3 (53.1215f, 21.1352f, 9.1322f)).ToShader ()
					select dt.Sin ().Fraction () * 2105.2354f * 6.283285f
				)
				.Evaluate ()
			);



		public static readonly Func<Sampler2D, Vec4, float> ShadowFactor =
			GLShader.Function
			(
				() => ShadowFactor,
				(Sampler2D shadowMap, Vec4 posInLightSpace) =>
				(
					from con in Shader.Constants (new
					{
						kernel = new Vec2[] 
						{ 
							new Vec2 (0.95581f, -0.18159f), new Vec2(0.50147f, -0.35807f), new Vec2(0.69607f, 0.35559f),
							new Vec2 (-0.0036825f, -0.59150f), new Vec2 (0.15930f, 0.089750f), new Vec2 (-0.65031f, 0.058189f),
							new Vec2 (0.11915f, 0.78449f), new Vec2 (-0.34296f, 0.51575f), new Vec2 (-0.60380f, -0.41527f)
						}
					})
					from projCoords in (posInLightSpace[Coord.x, Coord.y, Coord.z] / posInLightSpace.W).ToShader ()
					let texCoords = projCoords * 0.5f + new Vec3 (0.5f)
					let closestDepth = shadowMap.Texture (texCoords[Coord.x, Coord.y]).Z
					let currentDepth = texCoords.Z - 0.001f
					let mapSize = shadowMap.Size (0)
					let texelSize = new Vec2 (2f / mapSize.X, 2f / mapSize.Y)
					let angle = RandomAngle (projCoords, 15f)
					let s = angle.Sin ()
					let c = angle.Cos ()
					let pcfShadow = (from point in con.kernel
									 let rotatedPoint = new Vec2 (point.X * c + point.Y * s,
										point.X * -s + point.Y * c)
									 let sampleCoords = texCoords[Coord.x, Coord.y] + (point * texelSize)
									 select shadowMap.Texture (sampleCoords).Z)
									.Aggregate (0f, (sum, depth) => sum + (currentDepth < depth ? 1f : 0.15f))
					select pcfShadow / 9f
				)
				.Evaluate ()
			);

		/// <summary>
		/// Use this module. This function needs to be called once for static field initialization of
		/// this class.
		/// </summary>
		public static void Use () { }
	}
}