namespace Compose3D.Renderers
{
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.GLTypes;
	using Compose3D.Reactive;
	using Compose3D.SceneGraph;
	using Compose3D.Shaders;
	using Compose3D.Textures;
	using OpenTK.Graphics.OpenGL4;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;

	[StructLayout (LayoutKind.Sequential, Pack=4)]
	public struct TerrainVertex : IVertex, ITextured
	{
		public Vec3 position;
		public Vec3 normal;
		public Vec2 texturePos;

		Vec3 IPositional<Vec3>.position
		{
			get { return position; }
			set { position = value; }
		}

		Vec3 IPlanar<Vec3>.normal
		{
			get { return normal; }
			set
			{
				if (value.IsNaN ())
					throw new ArgumentException ("Normal component NaN");
				normal = value;
			}
		}
		
		Vec2 ITextured.texturePos
		{
			get { return texturePos; }
			set { texturePos = value; }
		}

		public override string ToString ()
		{
			return string.Format ("[Vertex: position={0}, normal={1}, texturePos={2}]",
				position, normal, texturePos);
		}
	}

	public class Terrain : Uniforms
	{
		public class Scene
		{
			private TerrainMesh<TerrainVertex>[,] _meshes;
			private Mat4 _worldToModel;

			public SceneNode Root;
			
			public Scene (SceneGraph sceneGraph)
			{
				Root = new SceneGroup (sceneGraph, Meshes (sceneGraph))
					.OffsetOrientAndScale (new Vec3 (-5800f, -10f, -5800f) * 1.5f, new Vec3 (0f), new Vec3 (3f));
				_worldToModel = Root.Transform.Inverse;
			}

			private IEnumerable<TerrainMesh<TerrainVertex>> Meshes (SceneGraph sceneGraph)
			{
				_meshes = new TerrainMesh<TerrainVertex>[100, 100];
				for (int x = 0; x < 100; x++)
					for (int y = 0; y < 100; y++)
					{
						var mesh = new TerrainMesh<TerrainVertex> (sceneGraph, new Vec2i (x * _patchStep, y * _patchStep), 
							new Vec2i (_patchSize, _patchSize), 20f, 0.03999f, 3, 5f, 4f);
						_meshes[x, y] = mesh;
						yield return mesh;
					}
			}

			public float Height (Vec3 posInWorldSpace)
			{
				var coords = _worldToModel.Transform (posInWorldSpace)[Coord.x, Coord.z] / _patchStep;
				var x = (int)coords.X;
				var y = (int)coords.Y;
				if (x < 0f || y < 0f || x >= _meshes.GetLength (0) || y >= _meshes.GetLength (1))
					return 10f;
				var mesh = _meshes[x, y];
				var vertVec = coords.Fraction () * _patchStep;
				var vertIndex = ((int)vertVec.Y * _patchSize) + (int)vertVec.X;
				var vertices = mesh.Patch.Vertices;
				return vertices != null ? vertices[vertIndex].position.Y : 10f;
			}
		}

		public class TerrainFragment : Fragment, IFragmentTexture<Vec2>
		{
			public Vec3 vertexNormal { get; set; }
			public Vec3 vertexPos { get; set; }
			public float visibility { get; set; }
			public float height { get; set; }
			public float slope { get; set; }
			public Vec2 fragTexturePos { get; set; }
		}

		public TransformUniforms transforms;
		public LightingUniforms lighting;
		public CascadedShadowUniforms shadows;
		public Uniform<Vec3> skyColor;
		public Uniform<Sampler2D> sandSampler;
		public Uniform<Sampler2D> rockSampler;
		public Uniform<Sampler2D> grassSampler;

		private Terrain (GLProgram program, SceneGraph scene, Vec3 skyCol)
			: base (program)
		{
			transforms = new TransformUniforms (program);
			lighting = new LightingUniforms (program, scene);
			shadows = new CascadedShadowUniforms (program,
				new Sampler2DArray (0).LinearFiltering ().ClampToEdges (Axes.All));

			using (program.Scope ())
			{
				skyColor &= skyCol;
				sandSampler &= new Sampler2D (1);
				rockSampler &= new Sampler2D (2);
				grassSampler &= new Sampler2D (3);
			}
		}
		
		private static GLProgram _terrainShader;
		private static Terrain _terrain;
		
		private const int _patchSize = 64;
		private const int _patchStep = 58;

		public static Reaction<Camera> Renderer (SceneGraph sceneGraph, Vec3 skyCol, 
			CascadedShadowUniforms shadowSource)
		{
			_terrainShader = new GLProgram (VertexShader (), FragmentShader ());
			_terrain = new Terrain (_terrainShader, sceneGraph, skyCol);
			var sandTexture = LoadTexture ("Sand");
			var rockTexture = LoadTexture ("Rock");
			var grassTexture = LoadTexture ("Grass");

			return React.By ((Camera cam) => _terrain.Render (cam, shadowSource))
				.BindSamplers (new Dictionary<Sampler, Texture> ()
				{
					{ !_terrain.shadows.csmShadowMap, sceneGraph.GlobalLighting.ShadowMap },
					{ !_terrain.sandSampler, sandTexture },
					{ !_terrain.rockSampler, rockTexture },
					{ !_terrain.grassSampler, grassTexture }
				})
				.DepthTest ()
				.Culling ()
				.Program (_terrainShader);
		}

		private static Texture LoadTexture (string name)
		{
			return Texture.FromFile (string.Format ("Textures/{0}.jpg", name)).Mipmapped ();
		}

		private void Render (Camera camera, CascadedShadowUniforms shadowSource)
		{
			var worldToCamera = camera.WorldToCamera;
			lighting.UpdateDirectionalLight (camera);
			shadows.viewLightMatrices &= !shadowSource.viewLightMatrices;
			//shadows.lightSpaceMatrix &= !shadowSource.lightSpaceMatrix;

			foreach (var mesh in camera.NodesInView<TerrainMesh<TerrainVertex>> ())
			{
				if (mesh.VertexBuffer != null && mesh.IndexBuffers != null)
				{
					transforms.UpdateModelViewAndNormalMatrices (worldToCamera * mesh.Transform);
					var distance = -(worldToCamera * mesh.BoundingBox).Front;
					var lod = distance < 100 ? 0 :
							  distance < 200 ? 1 :
							  2;
					_terrainShader.DrawElements (PrimitiveType.TriangleStrip, mesh.VertexBuffer,
						mesh.IndexBuffers[lod]);
				}
			}
		}

		public static Reaction<Mat4> UpdatePerspectiveMatrix ()
		{
			return React.By<Mat4> (matrix => _terrain.transforms.perspectiveMatrix &= matrix)
				.Program (_terrainShader);
		}

		private static GLShader VertexShader ()
		{
			return GLShader.Create (ShaderType.VertexShader, () =>
				from v in Shader.Inputs<TerrainVertex> ()
				from u in Shader.Uniforms<Terrain> ()
				from t in Shader.Uniforms<TransformUniforms> ()
				let viewPos = !u.transforms.modelViewMatrix * new Vec4 (v.position, 1f)
				select new TerrainFragment ()
				{
					gl_Position = !t.perspectiveMatrix * viewPos,
					vertexNormal = (!t.normalMatrix * v.normal).Normalized,
					vertexPos = viewPos[Coord.x, Coord.y, Coord.z],
					visibility = LightingShaders.FogVisibility (viewPos.Z, 400f, 2f),
					height = v.position.Y,
					slope = v.normal.Dot (new Vec3 (0f, 1f, 0f)),
					fragTexturePos = v.texturePos / 15f
				});
		}

		private static GLShader FragmentShader ()
		{
			return GLShader.Create (ShaderType.FragmentShader, () =>
				from f in Shader.Inputs<TerrainFragment> ()
				from u in Shader.Uniforms<Terrain> ()
				from l in Shader.Uniforms<LightingUniforms> ()
				from c in Shader.Uniforms<CascadedShadowUniforms> ()
				let rockColor = FragmentShaders.TextureColor (!u.rockSampler, f.fragTexturePos)
				let grassColor = FragmentShaders.TextureColor (!u.grassSampler, f.fragTexturePos)
				let sandColor = FragmentShaders.TextureColor (!u.sandSampler, f.fragTexturePos)
				let sandBlend = GLMath.SmoothStep (2f, 4f, f.height)
				let flatColor = grassColor.Mix (sandColor, sandBlend) 
				let rockBlend = GLMath.SmoothStep (0.8f, 0.9f, f.slope)
				let terrainColor = rockColor.Mix (flatColor, rockBlend)
				let diffuseLight = LightingShaders.LightDiffuseIntensity (
					(!l.directionalLight).direction,
					(!l.directionalLight).intensity, 
					f.vertexNormal)
				let ambient = (!l.globalLighting).ambientLightIntensity
				//let shadow = ShadowShaders.PcfShadowMapFactor (f.fragPositionLightSpace, 0.0015f)
				//let shadow = ShadowShaders.VarianceShadowMapFactor (new Vec4 (f.vertexPos, 1f))
				let shadow = ShadowShaders.CascadedShadowMapFactor (new Vec4 (f.vertexPos, 1f), 0.0015f)
				let litColor = LightingShaders.GlobalLightIntensity (!l.globalLighting, ambient, 
					diffuseLight * shadow, new Vec3 (0f), terrainColor, new Vec3 (0f))
				select new
				{
					outputColor = litColor.Mix (!u.skyColor, f.visibility)
				});
		}
	}
}