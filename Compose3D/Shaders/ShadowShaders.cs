namespace Compose3D.Shaders
{
	using System;
	using Compiler;
	using Maths;
	using GLTypes;
	using SceneGraph;
	using Textures;

	public class ShadowUniforms : Uniforms
	{
		public Uniform<Mat4> lightSpaceMatrix;
		public Uniform<Sampler2D> shadowMap;

		public ShadowUniforms (GLProgram program) : base (program) { }

		public ShadowUniforms (GLProgram program, Sampler2D sampler) : base (program)
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

		[FixedArray (MapCount)]
		public Uniform<Mat4[]> viewLightMatrices;
		public Uniform<Sampler2DArray> csmShadowMap;

		public CascadedShadowUniforms (GLProgram program) : base (program) { }

		public CascadedShadowUniforms (GLProgram program, Sampler2DArray sampler) : base (program)
		{
			using (program.Scope ())
				csmShadowMap &= sampler;
		}

		public void UpdateLightSpaceMatrices (Camera camera, DirectionalLight light)
		{
			viewLightMatrices &= light.CameraToCsmProjections (camera, MapCount);
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
					let result = Control<float>.For (0, con.kernel.Length, 0f,
						(int i, float sum) =>
							(from point in con.kernel[i].ToShader ()
							 let sampleCoords = texCoords[Coord.x, Coord.y] + (point * texelSize)
							 let depth = shadowMap.Texture (sampleCoords).X
							 select sum + (currentDepth < depth ? 1f : 0.1f))
						.Evaluate ())
					select result / 9f)
				.Evaluate ());

		public static readonly Func<Vec3, float, float, bool> Between =
			GLShader.Function (() => Between,
				(Vec3 texCoords, float low, float high) =>
				texCoords.X >= low && texCoords.Y >= low && texCoords.Z >= low &&
				texCoords.X <= high && texCoords.Y <= high && texCoords.Z <= high
			);

		public static readonly Func<Vec4, Vec4> ClampToNearPlane =
			GLShader.Function (() => ClampToNearPlane,
				(Vec4 ndcPos) => new Vec4 (ndcPos.X, ndcPos.Y, Math.Max (-1f, ndcPos.Z), ndcPos.W)
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
					let p = FMath.Step (currentDepth, moments.X)
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
						   new Vec2 (0.95581f, -0.18159f), new Vec2 (0.50147f, -0.35807f), new Vec2 (0.69607f, 0.35559f),
						   new Vec2 (-0.0036825f, -0.59150f),  new Vec2 (0.15930f, 0.089750f), new Vec2 (-0.65031f, 0.058189f),
						   new Vec2 (0.11915f, 0.78449f),  new Vec2 (-0.34296f, 0.51575f), new Vec2 (-0.60380f, -0.41527f)
						}
					})
					let closestDepth = shadowMap.Texture (new Vec3 (texCoords.X, texCoords.Y, mapIndex)).X
					let currentDepth = texCoords.Z - bias
					let mapSize = shadowMap.Size (0)
					let texelSize = new Vec2 (1f / mapSize.X, 1f / mapSize.Y)
					let result = Control<float>.For (0, con.kernel.Length, 0f,
						(int i, float sum) =>
							(from point in con.kernel[i].ToShader ()
							 let sampleCoords = texCoords[Coord.x, Coord.y] + (point * 2f * texelSize)
							 let depth = shadowMap.Texture (new Vec3 (sampleCoords.X, sampleCoords.Y, mapIndex)).X
							 select sum + (currentDepth < depth ? 1f : 0.1f))
						 .Evaluate ()
					)
					select result / con.kernel.Length)
				.Evaluate ());

		public static readonly Func<Vec4, float, float> CascadedShadowMapFactor =
			GLShader.Function (() => CascadedShadowMapFactor,
				(Vec4 posInViewSpace, float bias) =>
				(
					from u in Shader.Uniforms<CascadedShadowUniforms> ()
					let mapIndex = Control<int>.DoUntilChanges (0, (!u.viewLightMatrices).Length, -1,
						(int i, int best) =>
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
					select //Between (texCoords, 0f, 1f) ?
						csmPCFiltering (!u.csmShadowMap, texCoords, mapIndex, bias) //: 1f
				)
				.Evaluate ());
	}
}