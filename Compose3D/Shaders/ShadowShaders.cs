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

		public void UpdateLightSpaceMatrix (Mat4 lightSpace)
		{
			lightSpaceMatrix &= lightSpace;
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
							new Vec2 (-1f, -1f), new Vec2 (-1f, 0f), new Vec2 (-1f, 1f),
 							new Vec2 (0f, -1f), new Vec2 (0f, 0f), new Vec2 (0f, 1f),
							new Vec2 (1f, -1f), new Vec2 (1f, 0f), new Vec2 (1f, 1f)
						}
					})
					let closestDepth = shadowMap.Texture (new Vec3 (texCoords.X, texCoords.Y, mapIndex)).X
					let currentDepth = texCoords.Z - bias
					let mapSize = shadowMap.Size (0)
					let texelSize = new Vec2 (1f / mapSize.X, 1f / mapSize.Y)
					select (from point in con.kernel
							let sampleCoords = texCoords[Coord.x, Coord.y] + (point * texelSize)
							select shadowMap.Texture (new Vec3 (sampleCoords.X, sampleCoords.Y, mapIndex)).X)
							.Aggregate (0f, (sum, depth) => sum + (currentDepth < depth ? 1f : 0.1f)) / 9f
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