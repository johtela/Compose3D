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
					from viewDir in position.Normalized.ToShader ()
					let reflectDir = viewDir.Reflect<Vec3, float> (normal)
					select environmentMap.Texture (reflectDir)[Coord.x, Coord.y, Coord.z]
				)
				.Evaluate ()
			);

		public static readonly Func<Sampler2D, Vec3, float, float> PercentageCloserFiltering =
			GLShader.Function
			(
				() => PercentageCloserFiltering,
				(Sampler2D shadowMap, Vec3 texCoords, float bias) =>
				(
					from con in Shader.Constants (new
					{
						kernel = new Vec2[] 
						{
							new Vec2 (-1f, -1f), new Vec2 (-1f, 0f), new Vec2 (-1f, 1f),
 							new Vec2 (0f, -1f), new Vec2 (0f, 0f), new Vec2 (0f, 1f),
							new Vec2 (1f, -1f), new Vec2 (1f, 0f), new Vec2 (1f, 1f)
						}
					})
					let closestDepth = shadowMap.Texture (texCoords[Coord.x, Coord.y]).X
					let currentDepth = texCoords.Z - bias
					let mapSize = shadowMap.Size (0)
					let texelSize = new Vec2 (1f / mapSize.X, 1f / mapSize.Y)
					let pcfShadow = (from point in con.kernel
									 let sampleCoords = texCoords[Coord.x, Coord.y] + (point * texelSize)
									 select shadowMap.Texture (sampleCoords).X)
									.Aggregate (0f, (sum, depth) => sum + (currentDepth < depth ? 1f : 0.1f))
					select pcfShadow / 9f
				)
				.Evaluate ()
			);
		
		public static readonly Func<Vec3, float, float, bool> Between =
			GLShader.Function
			(
				() => Between,
				(Vec3 texCoords, float low, float high) =>
				texCoords.X >= low && texCoords.Y >= low && texCoords.Z >= low &&
				texCoords.X <= high && texCoords.Y <= high && texCoords.Z <= high
			);
		

		public static readonly Func<Sampler2D, Vec4, float, float> PcfShadowMapFactor =
			GLShader.Function
			(
				() => PcfShadowMapFactor,
				(Sampler2D shadowMap, Vec4 posInLightSpace, float bias) =>
				(
					from projCoords in (posInLightSpace[Coord.x, Coord.y, Coord.z] / posInLightSpace.W).ToShader ()
					let texCoords = projCoords * 0.5f + new Vec3 (0.5f)
					select Between (texCoords, 0f, 1f) ? 
						PercentageCloserFiltering (shadowMap, texCoords, bias) : 1f
				)
				.Evaluate ()
			);
		
		
		public static readonly Func<float, float, float, float> LinearStep = 
			GLShader.Function
			(
				() => LinearStep,
				(float value, float low, float high) =>
					((value - low) / (high - low)).Clamp (0f, 1f)
			);
		
		public static readonly Func<Sampler2D, Vec3, float> SummedAreaVariance =
			GLShader.Function
			(
				() => SummedAreaVariance,
				(Sampler2D shadowMap, Vec3 texCoords) =>
				(
					from currentDepth in texCoords.Z.ToShader ()
					let moments = shadowMap.Texture (texCoords[Coord.x, Coord.y])[Coord.x, Coord.y]
					let p = GLMath.Step (currentDepth, moments.X)
					let variance = Math.Max (moments.Y - (moments.X * moments.X), 0.0000001f)
					let d = currentDepth - moments.X
					let pmax = LinearStep (variance / (variance + d * d), 0f, 1f)
					select Math.Min (Math.Max (p, pmax), 1f)
				)
				.Evaluate ()
			);
					
		public static readonly Func<Sampler2D, Vec4, float> VarianceShadowMapFactor =
			GLShader.Function
			(
				() => VarianceShadowMapFactor,
				(Sampler2D shadowMap, Vec4 posInLightSpace) =>
				(
					from projCoords in (posInLightSpace[Coord.x, Coord.y, Coord.z] / posInLightSpace.W).ToShader ()
					let texCoords = projCoords * 0.5f + new Vec3 (0.5f)
					select Between (texCoords, 0f, 0.9f) ? SummedAreaVariance (shadowMap, texCoords) : 1f
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