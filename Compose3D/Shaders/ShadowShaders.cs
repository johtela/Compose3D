namespace Compose3D.Shaders
{
	using System;
	using System.Linq;
	using Maths;
	using GLTypes;
	using SceneGraph;
	using Textures;

	public class ShadowUniforms : Uniforms
	{
		public Uniform<Mat4> lightSpaceMatrix;
		public Uniform<Sampler2D> shadowMap;

		public ShadowUniforms (Program program) : base (program) { }

		public ShadowUniforms (Program program, Sampler2D sampler) : base (program)
		{
			using (program.Scope ())
				shadowMap &= sampler;
		}

		public void UpdateLightSpaceMatrix (Camera camera, DirectionalLight light)
		{
			lightSpaceMatrix &= light.CameraToShadowProjection (camera);
		}
	}

	public class CascadedShadowUniforms : Uniforms
	{
		public const int MapCount = 4;

		[GLArray (MapCount)]
		public Uniform<Mat4[]> viewLightMatrices;
		public Uniform<Sampler2DArray> csmShadowMap;

		public CascadedShadowUniforms (Program program) : base (program) { }

		public CascadedShadowUniforms (Program program, Sampler2DArray sampler) : base (program)
		{
			using (program.Scope ())
				csmShadowMap &= sampler;
		}

		public void UpdateLightSpaceMatrices (Camera camera, DirectionalLight light)
		{
			viewLightMatrices &= light.CascadedShadowFrustums (camera, MapCount);
		}
	}

	public static class ShadowShaders
	{
		public static readonly Func<Sampler2D, Vec3, float, float> PercentageCloserFiltering =
			GLShader.Function (() => PercentageCloserFiltering,
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
					select (from point in con.kernel
							let sampleCoords = texCoords[Coord.x, Coord.y] + (point * texelSize)
							select shadowMap.Texture (sampleCoords).X)
							.Aggregate (0f, (sum, depth) => sum + (currentDepth < depth ? 1f : 0.1f)) / 9f
				)
				.Evaluate ());

		public static readonly Func<Vec3, float, float, bool> Between =
			GLShader.Function (() => Between,
				(Vec3 texCoords, float low, float high) =>
				texCoords.X >= low && texCoords.Y >= low && texCoords.Z >= low &&
				texCoords.X <= high && texCoords.Y <= high && texCoords.Z <= high
			);


		public static readonly Func<Vec4, float, float> PcfShadowMapFactor =
			GLShader.Function (() => PcfShadowMapFactor,
				(Vec4 posInViewSpace, float bias) =>
				(
					from u in Shader.Uniforms<ShadowUniforms> ()
					let posInLightSpace = !u.lightSpaceMatrix * posInViewSpace
					let projCoords = posInLightSpace[Coord.x, Coord.y, Coord.z] / posInLightSpace.W
					let texCoords = projCoords * 0.5f + new Vec3 (0.5f)
					select Between (texCoords, 0f, 1f) ?
						PercentageCloserFiltering (!u.shadowMap, texCoords, bias) : 1f
				)
				.Evaluate ());

		public static readonly Func<float, float, float, float> LinearStep =
			GLShader.Function (() => LinearStep,
				(float value, float low, float high) =>
					((value - low) / (high - low)).Clamp (0f, 1f)
			);

		public static readonly Func<Sampler2D, Vec3, float> SummedAreaVariance =
			GLShader.Function (() => SummedAreaVariance,
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
				.Evaluate ());

		public static readonly Func<Vec4, float> VarianceShadowMapFactor =
			GLShader.Function (() => VarianceShadowMapFactor,
				(Vec4 posInViewSpace) =>
				(
					from u in Shader.Uniforms<ShadowUniforms> ()
					let posInLightSpace = !u.lightSpaceMatrix * posInViewSpace
					let projCoords = posInLightSpace[Coord.x, Coord.y, Coord.z] / posInLightSpace.W
					let texCoords = projCoords * 0.5f + new Vec3 (0.5f)
					select Between (texCoords, 0f, 0.9f) ? SummedAreaVariance (!u.shadowMap, texCoords) : 1f
				)
				.Evaluate ());

		public static readonly Func<Sampler2DArray, Vec3, float, float, float> csmPCFiltering =
			GLShader.Function (() => csmPCFiltering,
				(Sampler2DArray shadowMap, Vec3 texCoords, float mapIndex, float bias) =>
				(
					from con in Shader.Constants (new
					{
						kernel = new Vec2[]
						{
							//new Vec2 (0.3734659f, -0.8273796f),
							//new Vec2 (0.1914345f, -0.3285008f),
							//new Vec2 (-0.2934556f, -0.4006544f),
							//new Vec2 (0.6397942f, -0.2687152f),
							//new Vec2 (-0.02465286f, -0.7721488f),
							//new Vec2 (-0.5982372f, -0.7205318f),
							//new Vec2 (0.508617f, 0.2272395f),
							//new Vec2 (0.1029184f, 0.3121677f),
							//new Vec2 (-0.2912557f, 0.7276647f),
							//new Vec2 (-0.3520437f, 0.2009868f),
							//new Vec2 (0.5757321f, 0.636853f),
							//new Vec2 (0.1266643f, 0.743752f),
							//new Vec2 (-0.7601562f, 0.2416886f),
							//new Vec2 (0.9183863f, 0.1334105f),
							//new Vec2 (-0.7794592f, -0.2526905f)

						   new Vec2 (0.95581f, -0.18159f), new Vec2 (0.50147f, -0.35807f), new Vec2 (0.69607f, 0.35559f),
						   new Vec2 (-0.0036825f, -0.59150f),  new Vec2 (0.15930f, 0.089750f), new Vec2 (-0.65031f, 0.058189f),
						   new Vec2 (0.11915f, 0.78449f),  new Vec2 (-0.34296f, 0.51575f), new Vec2 (-0.60380f, -0.41527f)

							//new Vec2 (-1f, -1f), new Vec2 (-1f, 0f), new Vec2 (-1f, 1f),
							//new Vec2 (0f, -1f), new Vec2 (0f, 0f), new Vec2 (0f, 1f),
							//new Vec2 (1f, -1f), new Vec2 (1f, 0f), new Vec2 (1f, 1f),
						}
					})
					let closestDepth = shadowMap.Texture (new Vec3 (texCoords.X, texCoords.Y, mapIndex)).X
					let currentDepth = texCoords.Z - bias
					let mapSize = shadowMap.Size (0)
					let texelSize = new Vec2 (1f / mapSize.X, 1f / mapSize.Y)
					select (from point in con.kernel
							let sampleCoords = texCoords[Coord.x, Coord.y] + (point * 2f * texelSize)
							select shadowMap.Texture (new Vec3 (sampleCoords.X, sampleCoords.Y, mapIndex)).X)
							.Aggregate (0f, (sum, depth) => sum + (currentDepth < depth ? 1f : 0.1f)) / con.kernel.Length
				)
				.Evaluate ());

		public static readonly Func<Vec4, float, float> CascadedShadowMapFactor =
			GLShader.Function (() => CascadedShadowMapFactor,
				(Vec4 posInViewSpace, float bias) =>
				(
					from u in Shader.Uniforms<CascadedShadowUniforms> ()
					let mapIndex = Enumerable.Range (0, (!u.viewLightMatrices).Length)
						.Aggregate (-1, (int best, int i) => best < 0 &&
							Between (((!u.viewLightMatrices)[i] * posInViewSpace)[Coord.x, Coord.y, Coord.z], -1f, 1f) ?
							i : best)
					let posInLightSpace = (!u.viewLightMatrices)[mapIndex] * posInViewSpace
					let projCoords = posInLightSpace[Coord.x, Coord.y, Coord.z] / posInLightSpace.W
					let texCoords = projCoords * 0.5f + new Vec3 (0.5f)
					//let color =
					//	mapIndex == 0f ? new Vec3 (1f, 0f, 0f) :
					//	mapIndex == 1f ? new Vec3 (0f, 1f, 0f) :
					//	mapIndex == 2f ? new Vec3 (0f, 0f, 1f) :
					//	new Vec3 (1f)
					select Between (texCoords, 0f, 1f) ?
						csmPCFiltering (!u.csmShadowMap, texCoords, mapIndex, bias) : 1f
				)
				.Evaluate ());

		/// <summary>
		/// This function needs to be called once for static field initialization of this class.
		/// </summary>
		public static void Use () { }
	}
}