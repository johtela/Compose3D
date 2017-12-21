namespace Compose3D.Renderers
{
	using Compose3D.Compiler;
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

	[StructLayout (LayoutKind.Sequential, Pack = 4)]
	public struct EntityVertex : IVertex3D, IVertexInitializer<EntityVertex, Vec3>, IVertexColor<Vec3>, 
		ITextured, IReflective
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

		Vec3 IVertex<Vec3>.position
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

		Vec3 IVertex3D.normal
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

		float IReflective.reflectivity
		{
			get { return reflectivity; }
			set { reflectivity = value; }
		}

		void IVertexInitializer<EntityVertex, Vec3>.Initialize (ref EntityVertex frag)
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

	public class Entities : Uniforms
	{
		[FixedArray (4)]
		public Uniform<LightingShaders.PointLight[]> pointLights;
		[FixedArray (4)]
		public Uniform<Sampler2D[]> samplers;
		public Uniform<SamplerCube> diffuseMap;

		public LightingUniforms lighting;
		public TransformUniforms transforms;
		public CascadedShadowUniforms shadows;

		public Entities (GLProgram program, SceneGraph scene)
			: base (program)
		{
			var numPointLights = 0;
			var plights = new LightingShaders.PointLight[4];

			using (program.Scope ())
			{ 
				foreach (var pointLight in scene.Root.Traverse ().OfType<PointLight> ())
				{
					plights[numPointLights++] = new LightingShaders.PointLight
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
					samp[i] = new Sampler2D (i + 1).LinearFiltering ().ClampToEdges (Axes.All);
				samplers &= samp;
				diffuseMap &= new SamplerCube (5).LinearFiltering ().ClampToEdges (Axes.All);

				lighting = new LightingUniforms (program, scene);
				transforms = new TransformUniforms (program);
				shadows = new CascadedShadowUniforms (program,
					new Sampler2DArray (0).LinearFiltering ().ClampToEdges (Axes.All));
			}
		}

		private static GLProgram _entityShader;
		private static Entities _entities;

		public static Reaction<Camera> Renderer (SceneGraph sceneGraph, CascadedShadowUniforms shadowSource)
		{
			_entityShader = new GLProgram (
				VertexShader (), 
				FragmentShader ());
			_entities = new Entities (_entityShader, sceneGraph);

			return React.By<Camera> (cam => _entities.Render (cam, shadowSource))
				.BindSamplers (new Dictionary<Sampler, Texture> ()
				{
					{ !_entities.shadows.csmShadowMap, sceneGraph.GlobalLighting.ShadowMap },
					{ !_entities.diffuseMap, sceneGraph.GlobalLighting.DiffuseMap }
				})
				.DepthTest ()
				.Culling ()
				.Program (_entityShader);
		}

		private void Render (Camera camera, CascadedShadowUniforms shadowSource)
		{
			lighting.UpdateDirectionalLight (camera);
			shadows.viewLightMatrices &= !shadowSource.viewLightMatrices;
			//shadows.lightSpaceMatrix &= !shadowSource.lightSpaceMatrix;

			foreach (var mesh in camera.NodesInView<Mesh<EntityVertex>> ())
			{
				Sampler.Bind (!samplers, mesh.Textures);
				transforms.UpdateModelViewAndNormalMatrices (camera.WorldToCamera * mesh.Transform);
				_entityShader.DrawElements (PrimitiveType.Triangles, mesh.VertexBuffer, mesh.IndexBuffer);
				Sampler.Unbind (!samplers, mesh.Textures);
			}
		}

		public static Reaction<Mat4> UpdatePerspectiveMatrix ()
		{
			return React.By<Mat4> (matrix => _entities.transforms.perspectiveMatrix &= matrix)
				.Program (_entityShader);
		}

		public static GLShader VertexShader ()
		{
			return GLShader.Create (ShaderType.VertexShader, () =>
				from v in Shader.Inputs<EntityVertex> ()
				from t in Shader.Uniforms<TransformUniforms> ()
				let viewPos = !t.modelViewMatrix * new Vec4 (v.position, 1f)
				select new EntityFragment ()
				{
					gl_Position = !t.perspectiveMatrix * viewPos,
					fragPosition = viewPos[Coord.x, Coord.y, Coord.z],
					fragNormal = (!t.normalMatrix * v.normal).Normalized,
					fragDiffuse = v.diffuse,
					fragSpecular = v.specular,
					fragShininess = v.shininess,
					fragTexturePos = v.texturePos,
					fragReflectivity = v.reflectivity
				});
		}

		private static GLShader FragmentShader ()
		{
			return GLShader.Create
			(
				ShaderType.FragmentShader, () =>

				from f in Shader.Inputs<EntityFragment> ()
				from u in Shader.Uniforms<Entities> ()
				from l in Shader.Uniforms<LightingUniforms> ()
				from c in Shader.Uniforms<CascadedShadowUniforms> ()
				let samplerNo = (f.fragTexturePos.X / 10f).Truncate ()
				let fragDiffuse =
					samplerNo == 0 ? FragmentShaders.TextureColor ((!u.samplers)[0], f.fragTexturePos) :
					samplerNo == 1 ? FragmentShaders.TextureColor ((!u.samplers)[1], f.fragTexturePos - new Vec2 (10f)) :
					samplerNo == 2 ? FragmentShaders.TextureColor ((!u.samplers)[2], f.fragTexturePos - new Vec2 (20f)) :
					samplerNo == 3 ? FragmentShaders.TextureColor ((!u.samplers)[3], f.fragTexturePos - new Vec2 (30f)) :
					f.fragDiffuse
				let dirLight = LightingShaders.DirLightIntensity (!l.directionalLight, f.fragPosition, 
					f.fragNormal, f.fragShininess)
				let totalLight = Control<LightingShaders.DiffuseAndSpecular>.For (0, (!u.pointLights).Length, dirLight,
					(i, total) =>
						(from pl in (!u.pointLights)[i].ToShader ()
						let plint = LightingShaders.PointLightIntensity (pl, f.fragPosition, f.fragNormal, f.fragShininess)
						select new LightingShaders.DiffuseAndSpecular (plint.diffuse + total.diffuse,
							plint.specular + total.specular))
						.Evaluate ())
				let envLight = (!u.diffuseMap).Texture (f.fragNormal)[Coord.x, Coord.y, Coord.z]
				let ambient = envLight * (!l.globalLighting).ambientLightIntensity
				let reflectDiffuse = f.fragReflectivity == 0f ? fragDiffuse :
					fragDiffuse.Mix (LightingShaders.ReflectedColor (!u.diffuseMap, f.fragPosition, f.fragNormal), 
						f.fragReflectivity)
				//let shadow = ShadowShaders.PcfShadowMapFactor (f.fragPositionLightSpace, 0.0015f)
				//let shadow = ShadowShaders.VarianceShadowMapFactor (new Vec4 (f.fragPosition, 1f))
				let shadow = ShadowShaders.CascadedShadowMapFactor (new Vec4 (f.fragPosition, 1f), 0.002f)
				select new
				{
					outputColor = LightingShaders.GlobalLightIntensity (!l.globalLighting, ambient, 
						totalLight.diffuse * shadow, totalLight.specular * shadow, reflectDiffuse, f.fragSpecular)
				}
			);
		}
	}
}