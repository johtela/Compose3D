namespace ComposeTester
{
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.GLTypes;
	using Compose3D.SceneGraph;
	using Compose3D.Shaders;
	using Compose3D.Textures;
	using OpenTK.Graphics.OpenGL;
	using OpenTK;
	using System;
	using System.Linq;
	using System.Runtime.InteropServices;
	using LinqCheck;
	using Extensions;

	[StructLayout (LayoutKind.Sequential, Pack = 4)]
	public struct Vertex : IVertex, IVertexInitializer<Vertex>, IVertexColor<Vec3>, 
		ITextured, ITagged<Vertex>, IReflective
	{
		public Vec3 position;
		public Vec3 normal;
		public Vec3 diffuseColor;
		public Vec3 specularColor;
		public Vec2 texturePos;
		public float shininess;
		public float reflectivity;
		[OmitInGlsl]
		internal int tag;

		Vec3 IPositional<Vec3>.Position
		{
			get { return position; }
			set { position = value; }
		}

		Vec3 IDiffuseColor<Vec3>.Diffuse
		{
			get { return diffuseColor; }
			set { diffuseColor = value; }
		}

		Vec3 ISpecularColor<Vec3>.Specular
		{
			get { return specularColor; }
			set { specularColor = value; }
		}

		float ISpecularColor<Vec3>.Shininess
		{
			get { return shininess; }
			set { shininess = value; }
		}

		Vec3 IPlanar<Vec3>.Normal
		{
			get { return normal; }
			set
			{
				if (value.IsNan ())
					throw new ArgumentException ("Normal component NaN");
				normal = value;
			}
		}

		Vec2 ITextured.TexturePos
		{
			get { return texturePos; }
			set { texturePos = value; }
		}

		int ITagged<Vertex>.Tag
		{
			get { return tag; }
			set { tag = value; }
		}
		
		float IReflective.Reflectivity
		{
			get { return reflectivity; }
			set { reflectivity = value; }
		}

		void IVertexInitializer<Vertex>.Initialize (ref Vertex vertex)
		{
			vertex.texturePos = new Vec2 (float.PositiveInfinity);
		}

		public override string ToString ()
		{
			return string.Format ("[Vertex: Position={0}, DiffuseColor={1}, SpecularColor={2}, Normal={3}, Tag={4}]",
				position, diffuseColor, specularColor, normal, tag);
		}
	}
	
	public class EntityFragment : Fragment, IVertexFragment, IDiffuseFragment, ISpecularFragment, ITexturedFragment<Vec2>
	{
		public Vec3 vertexPosition { get; set; }
		public Vec3 vertexNormal { get; set; }
		public Vec3 vertexDiffuse { get; set; }
		public Vec3 vertexSpecular { get; set; }
		public float vertexShininess { get; set; }
		public float vertexReflectivity { get; set; }
		public Vec2 texturePosition { get; set; }
	}

	public class Entities
	{
		public class EntityUniforms : Uniforms
		{
			[GLArray (4)]
			public Uniform<Lighting.PointLight[]> pointLights;
			[GLArray (4)]
			public Uniform<Sampler2D[]> samplers;
			public Uniform<SamplerCube> diffuseMap;

			public EntityUniforms (Program program, SceneGraph scene)
				: base (program)
			{
				var numPointLights = 0;
				var plights = new Lighting.PointLight[4];

				using (program.Scope ())
				{
					scene.Root.Traverse ()
						.WhenOfType<SceneNode, PointLight> (pointLight =>
							plights[numPointLights++] = new Lighting.PointLight
							{
								position = pointLight.Position,
								intensity = pointLight.Intensity,
								linearAttenuation = pointLight.LinearAttenuation,
								quadraticAttenuation = pointLight.QuadraticAttenuation
							})
						.ToVoid ();
					pointLights &= plights;

					var samp = new Sampler2D[4];
					var sampParams = new SamplerParams ()
					{
						{ SamplerParameterName.TextureMagFilter, All.Linear },
						{ SamplerParameterName.TextureMinFilter, All.Linear },
						{ SamplerParameterName.TextureWrapR, All.ClampToEdge },
						{ SamplerParameterName.TextureWrapS, All.ClampToEdge },
						{ SamplerParameterName.TextureWrapT, All.ClampToEdge }
					};
					for (int i = 0; i < samp.Length; i++)
						samp[i] = new Sampler2D (i, sampParams);
					samplers &= samp;
					diffuseMap &= new SamplerCube (4, sampParams);
				}
			}
		}

		public readonly Program EntityShader;
		public readonly EntityUniforms Uniforms;
		public readonly LightingUniforms LightingUniforms;
		public readonly TransformUniforms Transforms;

		public Entities (SceneGraph sceneGraph)
		{
			EntityShader = new Program (VertexShader (), FragmentShader ());
			Uniforms = new EntityUniforms (EntityShader, sceneGraph);
			LightingUniforms = new LightingUniforms (EntityShader, sceneGraph);
			Transforms = new TransformUniforms (EntityShader);
		}

		public static TransformNode CreateScene (SceneGraph sceneGraph)
		{
			var fighter = new FighterGeometry<Vertex, PathNode> ();
			return new Mesh<Vertex> (sceneGraph, fighter.Fighter.RotateY (0f /* MathHelper.PiOver2 */).Compact ())
				.OffsetOrientAndScale (new Vec3 (0f, 15f, -10f), new Vec3 (0f, 0f, 0f), new Vec3 (1f));
		}

		public void Render (Camera camera)
		{
			GL.Enable (EnableCap.CullFace);
			GL.CullFace (CullFaceMode.Back);
			GL.FrontFace (FrontFaceDirection.Cw);
			GL.Enable (EnableCap.DepthTest);
			GL.DepthMask (true);
			GL.DepthFunc (DepthFunction.Less);

			var diffTexture = camera.Graph.GlobalLighting.DiffuseMap;

			using (EntityShader.Scope ())
				foreach (var mesh in camera.NodesInView <Mesh<Vertex>> ())
				{
					Sampler.Bind (!Uniforms.samplers, mesh.Textures);
					(!Uniforms.diffuseMap).Bind (diffTexture);
					Transforms.modelViewMatrix &= camera.WorldToCamera * mesh.Transform;
					Transforms.normalMatrix &= new Mat3 (mesh.Transform).Inverse.Transposed;
					EntityShader.DrawElements (PrimitiveType.Triangles, mesh.VertexBuffer, mesh.IndexBuffer);
					(!Uniforms.diffuseMap).Unbind (diffTexture);
					Sampler.Unbind (!Uniforms.samplers, mesh.Textures);
				}
		}

		public void UpdateViewMatrix (Mat4 matrix)
		{
			using (EntityShader.Scope ())
				Transforms.perspectiveMatrix &= matrix;
		}

		public static GLShader VertexShader ()
		{
			return GLShader.Create
			(
				ShaderType.VertexShader, () =>

				from v in Shader.Inputs<Vertex> ()
				from t in Shader.Uniforms<TransformUniforms> ()
				let viewPos = !t.modelViewMatrix * new Vec4 (v.position, 1f)
				select new EntityFragment ()
				{
					gl_Position = !t.perspectiveMatrix * viewPos,
					vertexPosition = viewPos[Coord.x, Coord.y, Coord.z],
					vertexNormal = (!t.normalMatrix * v.normal).Normalized,
					vertexDiffuse = v.diffuseColor,
					vertexSpecular = v.specularColor,
					vertexShininess = v.shininess,
					texturePosition = v.texturePos,
					vertexReflectivity = v.reflectivity
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

				from f in Shader.Inputs<EntityFragment> ()
				from u in Shader.Uniforms<EntityUniforms> ()
				from l in Shader.Uniforms<LightingUniforms> ()
				let samplerNo = (f.texturePosition.X / 10f).Truncate ()
				let fragDiffuse =
					samplerNo == 0 ? FragmentShaders.TextureColor ((!u.samplers)[0], f.texturePosition) :
					samplerNo == 1 ? FragmentShaders.TextureColor ((!u.samplers)[1], f.texturePosition - new Vec2 (10f)) :
					samplerNo == 2 ? FragmentShaders.TextureColor ((!u.samplers)[2], f.texturePosition - new Vec2 (20f)) :
					samplerNo == 3 ? FragmentShaders.TextureColor ((!u.samplers)[3], f.texturePosition - new Vec2 (30f)) :
					f.vertexDiffuse
					let dirLight = Lighting.DirLightIntensity (!l.directionalLight, f.vertexPosition, 
						f.vertexNormal, f.vertexShininess)
				let totalLight = 
					(from pl in !u.pointLights
					 select Lighting.PointLightIntensity (pl, f.vertexPosition, f.vertexNormal, f.vertexShininess))
					.Aggregate (dirLight, 
						(total, pointLight) => new Lighting.DiffuseAndSpecular (
							total.diffuse + pointLight.diffuse, total.specular + pointLight.specular))
				let envLight = (!u.diffuseMap).Texture (f.vertexNormal)[Coord.x, Coord.y, Coord.z]
					let ambient = envLight * (!l.globalLighting).ambientLightIntensity
				let reflectDiffuse = f.vertexReflectivity > 0f ? 
					fragDiffuse.Mix (Lighting.ReflectedColor (!u.diffuseMap, f.vertexPosition, f.vertexNormal), 
						f.vertexReflectivity) :
					fragDiffuse
				select new
				{
					outputColor = Lighting.GlobalLightIntensity (!l.globalLighting, ambient, 
						totalLight.diffuse, totalLight.specular, reflectDiffuse, f.vertexSpecular)
				}
			);
		}
	}
}
