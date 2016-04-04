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
	using OpenTK;

	[StructLayout (LayoutKind.Sequential, Pack = 4)]
	public struct EntityVertex : IVertex, IVertexInitializer<EntityVertex>, IVertexColor<Vec3>, 
		ITextured, ITagged<EntityVertex>, IReflective
	{
		public Vec3 position;
		public Vec3 normal;
		public Vec3 diffuse;
		public Vec3 specular;
		public Vec2 texturePos;
		public float shininess;
		public float reflectivity;
		[OmitInGlsl]
		internal int tag;

		Vec3 IPositional<Vec3>.position
		{
			get { return position; }
			set { position = value; }
		}

		Vec3 IDiffuseColor<Vec3>.diffuse
		{
			get { return diffuse; }
			set { diffuse = value; }
		}

		Vec3 ISpecularColor<Vec3>.specular
		{
			get { return specular; }
			set { specular = value; }
		}

		float ISpecularColor<Vec3>.shininess
		{
			get { return shininess; }
			set { shininess = value; }
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

		int ITagged<EntityVertex>.tag
		{
			get { return tag; }
			set { tag = value; }
		}
		
		float IReflective.reflectivity
		{
			get { return reflectivity; }
			set { reflectivity = value; }
		}

		void IVertexInitializer<EntityVertex>.Initialize (ref EntityVertex frag)
		{
			frag.texturePos = new Vec2 (float.PositiveInfinity);
		}

		public override string ToString ()
		{
			return string.Format ("[Vertex: position={0}, diffuse={1}, specular={2}, normal={3}, tag={4}]",
				position, diffuse, specular, normal, tag);
		}
	}
	
	public class EntityFragment : Fragment, IFragmentPosition, IFragmentDiffuse, IFragmentSpecular, 
		IFragmentTexture<Vec2>, IFragmentReflectivity
	{
		public Vec3 fragPosition { get; set; }
		public Vec3 fragNormal { get; set; }
		public Vec3 fragDiffuse { get; set; }
		public Vec3 fragSpecular { get; set; }
		public float fragShininess { get; set; }
		public float fragReflectivity { get; set; }
		public Vec2 fragTexturePos { get; set; }
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
					foreach (var pointLight in scene.Root.Traverse ().OfType<PointLight> ())
					{
						plights[numPointLights++] = new Lighting.PointLight
						{
							position = pointLight.Position,
							intensity = pointLight.Intensity,
							linearAttenuation = pointLight.LinearAttenuation,
							quadraticAttenuation = pointLight.QuadraticAttenuation
						};
					}
					pointLights &= plights;

					var samp = new Sampler2D[4];
					for (int i = 0; i < samp.Length; i++)
						samp[i] = new Sampler2D (i, Sampler.BasicParams);
					samplers &= samp;
					diffuseMap &= new SamplerCube (4, Sampler.BasicParams);
				}
			}
		}

		public readonly Program EntityShader;
		public readonly EntityUniforms Uniforms;
		public readonly LightingUniforms LightingUniforms;
		public readonly TransformUniforms Transforms;

		public Entities (SceneGraph sceneGraph)
		{
			EntityShader = new Program (
				VertexShaders.BasicShader<EntityVertex, EntityFragment, TransformUniforms> (), 
				FragmentShader ());
			Uniforms = new EntityUniforms (EntityShader, sceneGraph);
			LightingUniforms = new LightingUniforms (EntityShader, sceneGraph);
			Transforms = new TransformUniforms (EntityShader);
		}

		public static TransformNode CreateScene (SceneGraph sceneGraph)
		{
			var fighter = new FighterGeometry<EntityVertex, PathNode> ();
			return new Mesh<EntityVertex> (sceneGraph, fighter.Fighter.RotateY (MathHelper.PiOver2).Compact ())
				.OffsetOrientAndScale (new Vec3 (0f, 15f, -10f), new Vec3 (0f, 0f, 0f), new Vec3 (1f));
		}

		public void Render (Camera camera)
		{
			using (EntityShader.Scope ())
			{
				GL.Enable (EnableCap.CullFace);
				GL.CullFace (CullFaceMode.Back);
				GL.FrontFace (FrontFaceDirection.Cw);
				GL.Enable (EnableCap.DepthTest);
				GL.DepthMask (true);
				GL.DepthFunc (DepthFunction.Less);
				GL.Disable (EnableCap.Blend);
				GL.DrawBuffer (DrawBufferMode.Back);

				var diffTexture = camera.Graph.GlobalLighting.DiffuseMap;
				var dirLight = camera.Graph.Root.Traverse ().OfType<DirectionalLight> ().First ();

				foreach (var mesh in camera.NodesInView<Mesh<EntityVertex>> ())
				{
					Sampler.Bind (!Uniforms.samplers, mesh.Textures);
					(!Uniforms.diffuseMap).Bind (diffTexture);
					LightingUniforms.UpdateDirectionalLight (camera);
					Transforms.UpdateModelViewAndNormalMatrices (camera.WorldToCamera * mesh.Transform);
					EntityShader.DrawElements (PrimitiveType.Triangles, mesh.VertexBuffer, mesh.IndexBuffer);
					(!Uniforms.diffuseMap).Unbind (diffTexture);
					Sampler.Unbind (!Uniforms.samplers, mesh.Textures);
				}
			}
		}

		public void UpdateViewMatrix (Mat4 matrix)
		{
			using (EntityShader.Scope ())
				Transforms.perspectiveMatrix &= matrix;
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
				let samplerNo = (f.fragTexturePos.X / 10f).Truncate ()
				let fragDiffuse =
					samplerNo == 0 ? FragmentShaders.TextureColor ((!u.samplers)[0], f.fragTexturePos) :
					samplerNo == 1 ? FragmentShaders.TextureColor ((!u.samplers)[1], f.fragTexturePos - new Vec2 (10f)) :
					samplerNo == 2 ? FragmentShaders.TextureColor ((!u.samplers)[2], f.fragTexturePos - new Vec2 (20f)) :
					samplerNo == 3 ? FragmentShaders.TextureColor ((!u.samplers)[3], f.fragTexturePos - new Vec2 (30f)) :
					f.fragDiffuse
					let dirLight = Lighting.DirLightIntensity (!l.directionalLight, f.fragPosition, 
						f.fragNormal, f.fragShininess)
				let totalLight = 
					(from pl in !u.pointLights
					 select Lighting.PointLightIntensity (pl, f.fragPosition, f.fragNormal, f.fragShininess))
					.Aggregate (dirLight, 
						(total, pointLight) => new Lighting.DiffuseAndSpecular (
							total.diffuse + pointLight.diffuse, total.specular + pointLight.specular))
				let envLight = (!u.diffuseMap).Texture (f.fragNormal)[Coord.x, Coord.y, Coord.z]
					let ambient = envLight * (!l.globalLighting).ambientLightIntensity
				let reflectDiffuse = f.fragReflectivity > 0f ? 
					fragDiffuse.Mix (Lighting.ReflectedColor (!u.diffuseMap, f.fragPosition, f.fragNormal), 
						f.fragReflectivity) :
					fragDiffuse
				select new
				{
					outputColor = Lighting.GlobalLightIntensity (!l.globalLighting, ambient, 
						totalLight.diffuse, totalLight.specular, reflectDiffuse, f.fragSpecular)
				}
			);
		}
	}
}