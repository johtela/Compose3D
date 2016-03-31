namespace ComposeTester
{
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.GLTypes;
	using Compose3D.SceneGraph;
	using Compose3D.Shaders;
	using Compose3D.Textures;
	using OpenTK.Graphics.OpenGL;
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
				if (value.IsNan ())
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

	public class Terrain
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
							new Vec2i (_patchSize, _patchSize), 20f, 0.039999f, 3, 5f, 4f);
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
		
		public class TerrainFragment : Fragment
		{
			public Vec3 vertexNormal;
			public float visibility;
			public Vec2 fragTexPos;
			public float height;
			public float slope;
		}

		public class TerrainUniforms : Uniforms
		{
			public TransformUniforms Transforms;
			public LightingUniforms Lighting;
			public Uniform<Vec3> skyColor;
			public Uniform<Sampler2D> sandSampler;
			public Uniform<Sampler2D> rockSampler;
			public Uniform<Sampler2D> grassSampler;

			public TerrainUniforms (Program program, SceneGraph scene, Vec3 skyCol)
				: base (program)
			{
				Transforms = new TransformUniforms (program);
				Lighting = new LightingUniforms (program, scene);

				using (program.Scope ())
				{
					skyColor &= skyCol;
					sandSampler &= CreateSampler (0);
					rockSampler &= CreateSampler (1);
					grassSampler &= CreateSampler (2);
				}
			}
			
			private Sampler2D CreateSampler (int index)
			{
				return new Sampler2D (index, new SamplerParams ()
				{
					{ SamplerParameterName.TextureMagFilter, All.Mipmap },
					{ SamplerParameterName.TextureMinFilter, All.Mipmap },
					{ SamplerParameterName.TextureWrapR, All.Repeat },
					{ SamplerParameterName.TextureWrapS, All.Repeat }
				});				
			}
		}
		
		public readonly Program TerrainShader;
		public readonly TerrainUniforms Uniforms;
		
		private Texture _sandTexture;
		private Texture _rockTexture;
		private Texture _grassTexture;

		private const int _patchSize = 64;
		private const int _patchStep = 58;

		public Terrain (SceneGraph sceneGraph, Vec3 skyCol)
		{
			TerrainShader = new Program (VertexShader (), FragmentShader ());
			Uniforms = new TerrainUniforms (TerrainShader, sceneGraph, skyCol);
			_sandTexture = LoadTexture ("Sand");
			_rockTexture = LoadTexture ("Rock");
			_grassTexture = LoadTexture ("Grass");
		}

		private static Texture LoadTexture (string name)
		{
			return Texture.FromFile (string.Format ("Textures/{0}.jpg", name), true,
				new TextureParams ()
				{
					{ TextureParameterName.TextureBaseLevel, 0 },
					{ TextureParameterName.TextureMaxLevel, 10 },
					{ TextureParameterName.TextureMinFilter, TextureMinFilter.LinearMipmapLinear }
				});
		}

		public void Render (Camera camera)
		{
			GL.Enable (EnableCap.CullFace);
			GL.CullFace (CullFaceMode.Back);
			GL.FrontFace (FrontFaceDirection.Cw);
			GL.Enable (EnableCap.DepthTest);
			GL.DepthMask (true);
			GL.DepthFunc (DepthFunction.Less);

			var worldToCamera = camera.WorldToCamera;
			var samplers = new Sampler [] { !Uniforms.sandSampler, !Uniforms.rockSampler, !Uniforms.grassSampler };
			var textures = new Texture [] { _sandTexture, _rockTexture, _grassTexture };
			
			using (TerrainShader.Scope ())
			{
				Sampler.Bind (samplers, textures);
				foreach (var mesh in camera.NodesInView<TerrainMesh<TerrainVertex>> ())
				{
					if (mesh.VertexBuffer != null && mesh.IndexBuffers != null)
					{
						Uniforms.Transforms.modelViewMatrix &= worldToCamera * mesh.Transform;
						Uniforms.Transforms.normalMatrix &= new Mat3 (mesh.Transform).Inverse.Transposed;
						var distance = -(worldToCamera * mesh.BoundingBox).Front;
						var lod = distance < 100 ? 0 :
								  distance < 200 ? 1 :
								  2;
						TerrainShader.DrawElements (PrimitiveType.TriangleStrip, mesh.VertexBuffer, 
							mesh.IndexBuffers [lod]);
					}
				}
				Sampler.Unbind (samplers, textures);
			}
		}

		public void UpdateViewMatrix (Mat4 matrix)
		{
			using (TerrainShader.Scope ())
				Uniforms.Transforms.perspectiveMatrix &= matrix;
		}

		public static GLShader VertexShader ()
		{
			Lighting.Use ();
			return GLShader.Create (ShaderType.VertexShader, () =>
				from v in Shader.Inputs<TerrainVertex> ()
				from u in Shader.Uniforms<TerrainUniforms> ()
				let viewPos = !u.Transforms.modelViewMatrix * new Vec4 (v.position, 1f)
				select new TerrainFragment ()
				{
					gl_Position = !u.Transforms.perspectiveMatrix * viewPos,
					vertexNormal = (!u.Transforms.normalMatrix * v.normal).Normalized,
					visibility = Lighting.FogVisibility (viewPos.Z, 0.003f, 3f),
					height = v.position.Y,
					slope = v.normal.Dot (new Vec3 (0f, 1f, 0f)),
					fragTexPos = v.texturePos / 15f
				});
		}

		public static GLShader FragmentShader ()
		{
			Lighting.Use ();
			FragmentShaders.Use ();
			return GLShader.Create (ShaderType.FragmentShader, () =>
				from f in Shader.Inputs<TerrainFragment> ()
				from u in Shader.Uniforms<TerrainUniforms> ()
				let rockColor = FragmentShaders.TextureColor (!u.rockSampler, f.fragTexPos)
				let grassColor = FragmentShaders.TextureColor (!u.grassSampler, f.fragTexPos)
				let sandColor = FragmentShaders.TextureColor (!u.sandSampler, f.fragTexPos)
				let sandBlend = GLMath.SmoothStep (2f, 4f, f.height)
				let flatColor = grassColor.Mix (sandColor, sandBlend) 
				let rockBlend = GLMath.SmoothStep (0.8f, 0.9f, f.slope)
				let terrainColor = rockColor.Mix (flatColor, rockBlend)
				let diffuseLight = Lighting.LightDiffuseIntensity (
					(!u.Lighting.directionalLight).direction,
					(!u.Lighting.directionalLight).intensity, 
					f.vertexNormal)
				let ambient = (!u.Lighting.globalLighting).ambientLightIntensity
				let litColor = Lighting.GlobalLightIntensity (
						!u.Lighting.globalLighting,
						ambient, diffuseLight,
						new Vec3 (0f), terrainColor, new Vec3 (0f))
				select new
				{
					outputColor = litColor.Mix (!u.skyColor, f.visibility)
				});
		}
	}
}