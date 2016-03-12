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
	using System.Linq;
	using System.Runtime.InteropServices;
	using Extensions;

	[StructLayout (LayoutKind.Sequential, Pack=4)]
	public struct TerrainVertex : IVertex, ITextured
	{
		internal Vec3 position;
		internal Vec3 normal;
		internal Vec2 texturePos;
		[OmitInGlsl]
		internal int tag;

		Vec3 IPositional<Vec3>.Position
		{
			get { return position; }
			set { position = value; }
		}

		Vec3 IPlanar<Vec3>.Normal
		{
			get { return normal; }
			set
			{
				if (float.IsNaN (value.X) || float.IsNaN (value.Y) || float.IsNaN (value.Z))
					throw new ArgumentException ("Normal component NaN");
				normal = value;
			}
		}
		
		Vec2 ITextured.TexturePos
		{
			get { return new Vec2 (); }
			set {  }
		}

		int IVertex.Tag
		{
			get { return 0; }
			set { }
		}

		public override string ToString ()
		{
			return string.Format ("[Vertex: Position={0}, Normal={3}, Tag={4}]",
				position, normal, 0);
		}
	}

	public class Terrain
	{
		public class TerrainFragment : DiffuseFragment
		{
			public float visibility;
			public Vec2 vertexTexPos;
			public float height;
			public float slope;
		}

		public class TerrainUniforms : BasicUniforms
		{
			public Uniform<Vec3> skyColor;
			public Uniform<Sampler2D> sandSampler;
			public Uniform<Sampler2D> rockSampler;
			public Uniform<Sampler2D> grassSampler;

			public void Initialize (Program program, SceneGraph scene, Vec3 skyCol)
			{
				base.Initialize (program, scene);
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
						{ SamplerParameterName.TextureMagFilter, All.Linear },
						{ SamplerParameterName.TextureMinFilter, All.Linear },
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

		public Terrain ()
		{
			TerrainShader = new Program (VertexShader (), FragmentShader ());
			TerrainShader.InitializeUniforms (Uniforms = new TerrainUniforms ());
		}

		private Texture LoadTexture (string name)
		{
			return Texture.FromFile (string.Format ("Textures/{0}.png", name), 
				new TextureParams ()
				{
					{ TextureParameterName.TextureBaseLevel, 0 },
					{ TextureParameterName.TextureMaxLevel, 0 }
				});
		}
		
		public SceneNode CreateScene (SceneGraph sceneGraph)
		{
			_sandTexture = LoadTexture ("Sand");
			_rockTexture = LoadTexture ("Rock");
			_grassTexture = LoadTexture ("Grass");
			return new SceneGroup (sceneGraph,
				from x in EnumerableExt.Range (0, 5000, 58)
				from y in EnumerableExt.Range (0, 5000, 58)
				select new TerrainMesh<TerrainVertex> (sceneGraph, new Vec2i (x, y), new Vec2i (64, 64),
					20f, 0.039999f, 3, 5f, 4f))
				.OffsetOrientAndScale (new Vec3 (-5000f, -10f, -5000f), new Vec3 (0f), new Vec3 (2f));
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
						Uniforms.worldMatrix &= worldToCamera * mesh.Transform;
						Uniforms.normalMatrix &= new Mat3 (mesh.Transform).Inverse.Transposed;
						var distance = -(worldToCamera * mesh.BoundingBox).Front;
						var lod = distance < 150 ? 0 :
								  distance < 250 ? 1 :
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
				Uniforms.perspectiveMatrix &= matrix;
		}

		public static GLShader VertexShader ()
		{
			Lighting.Use ();
			return GLShader.Create
			(
				ShaderType.VertexShader, () =>

				from v in Shader.Inputs<TerrainVertex> ()
				from u in Shader.Uniforms<TerrainUniforms> ()
				let viewPos = !u.worldMatrix * new Vec4 (v.position, 1f)
				let vertPos = viewPos[Coord.x, Coord.y, Coord.z]
				select new TerrainFragment ()
				{
					gl_Position = !u.perspectiveMatrix * viewPos,
					vertexPosition = vertPos,
					vertexNormal = (!u.normalMatrix * v.normal).Normalized,
					vertexDiffuse = new Vec3 (1f),
					visibility = Lighting.FogVisibility (vertPos.Z, 0.004f, 3f),
					height = v.position.Y,
					slope = v.normal.Dot (new Vec3 (0f, 1f, 0f)),
//					vertexTexPos = v.texturePos
				}
			);
		}

		public static GLShader FragmentShader ()
		{
			Lighting.Use ();
			FragmentShaders.Use ();

			return GLShader.Create
			(
				ShaderType.FragmentShader, () =>

				from f in Shader.Inputs<TerrainFragment> ()
				from u in Shader.Uniforms<TerrainUniforms> ()
//				let rockColor = FragmentShaders.TextureColor (!u.rockSampler, f.vertexTexPos)
//				let grassColor = FragmentShaders.TextureColor (!u.grassSampler, f.vertexTexPos)
//				let sandColor = FragmentShaders.TextureColor (!u.sandSampler, f.vertexTexPos)
				let blend = GLMath.SmoothStep (0.9f, 0.99f, f.slope)
				let rockColor = new Vec3 (0.3f, 0.4f, 0.5f)
				let grassColor = new Vec3 (0.3f, 1f, 0.2f)
				let sandColor = new Vec3 (1f, 1f, 1f)
				let textureColor = f.height < -2f ? 
					rockColor.Mix (grassColor, blend) : 
					rockColor.Mix (sandColor, blend)
				let diffuse = Lighting.DirectionalLightIntensity (!u.directionalLight, f.vertexNormal) * textureColor
				select new
				{
					outputColor = Lighting.GlobalLightIntensity (!u.globalLighting, diffuse * 3f, new Vec3 (0f))
						.Mix (!u.skyColor, f.visibility)
				}
			);
		}
	}
}