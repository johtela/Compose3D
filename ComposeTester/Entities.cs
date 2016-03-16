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

	[StructLayout (LayoutKind.Sequential)]
	public struct Vertex : IVertex, IVertexInitializer<Vertex>, IVertexColor<Vec3>, ITextured
	{
		internal Vec3 position;
		internal Vec3 normal;
		internal Vec3 diffuseColor;
		internal Vec3 specularColor;
		internal Vec2 texturePos;
		internal float shininess;
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
				if (float.IsNaN (value.X) || float.IsNaN (value.Y) || float.IsNaN (value.Z))
					throw new ArgumentException ("Normal component NaN");
				normal = value;
			}
		}

		Vec2 ITextured.TexturePos
		{
			get { return texturePos; }
			set { texturePos = value; }
		}

		int IVertex.Tag
		{
			get { return tag; }
			set { tag = value; }
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

	public class Entities
	{
		public class EntityUniforms : BasicUniforms
		{
			[GLArray (4)]
			public Uniform<Lighting.PointLight[]> pointLights;
			[GLArray (4)]
			public Uniform<Sampler2D[]> samplers;

			public new void Initialize (Program program, SceneGraph scene)
			{
				base.Initialize (program, scene);
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
					for (int i = 0; i < samp.Length; i++)
						samp[i] = new Sampler2D (i, new SamplerParams ()
						{
							{ SamplerParameterName.TextureMagFilter, All.Linear },
							{ SamplerParameterName.TextureMinFilter, All.Linear },
							{ SamplerParameterName.TextureWrapR, All.ClampToEdge },
							{ SamplerParameterName.TextureWrapS, All.ClampToEdge }
						});
					samplers &= samp;
				}
			}
		}

		public readonly Program EntityShader;
		public readonly EntityUniforms Uniforms;

		public Entities ()
		{
			EntityShader = new Program (VertexShader (), FragmentShader ());
			EntityShader.InitializeUniforms (Uniforms = new EntityUniforms ());
		}

		public SceneNode CreateScene (SceneGraph sceneGraph)
		{
			var fighter = new FighterGeometry<Vertex, PathNode> ();
			return new Mesh<Vertex> (sceneGraph, fighter.Fighter)
				.OffsetOrientAndScale (new Vec3 (0f, 10f, -10f), new Vec3 (0f, MathHelper.Pi, 0f), new Vec3 (1f));
		}

		public void Render (Camera camera)
		{
			GL.Enable (EnableCap.CullFace);
			GL.CullFace (CullFaceMode.Back);
			GL.FrontFace (FrontFaceDirection.Cw);
			GL.Enable (EnableCap.DepthTest);
			GL.DepthMask (true);
			GL.DepthFunc (DepthFunction.Less);

			using (EntityShader.Scope ())
				foreach (var mesh in camera.NodesInView <Mesh<Vertex>> ())
				{
					Sampler.Bind (!Uniforms.samplers, mesh.Textures);
					Uniforms.worldMatrix &= camera.WorldToCamera * mesh.Transform;
					Uniforms.normalMatrix &= new Mat3 (mesh.Transform).Inverse.Transposed;
					EntityShader.DrawElements (PrimitiveType.Triangles, mesh.VertexBuffer, mesh.IndexBuffer);
					Sampler.Unbind (!Uniforms.samplers, mesh.Textures);
				}
		}

		public void UpdateViewMatrix (Mat4 matrix)
		{
			using (EntityShader.Scope ())
				Uniforms.perspectiveMatrix &= matrix;
		}

		public static GLShader VertexShader ()
		{
			return GLShader.Create
			(
				ShaderType.VertexShader, () =>

				from v in Shader.Inputs<Vertex> ()
				from u in Shader.Uniforms<EntityUniforms> ()
				let worldPos = !u.worldMatrix * new Vec4 (v.position, 1f)
				select new TexturedFragment ()
				{
					gl_Position = !u.perspectiveMatrix * worldPos,
					vertexPosition = worldPos[Coord.x, Coord.y, Coord.z],
					vertexNormal = (!u.normalMatrix * v.normal).Normalized,
					vertexDiffuse = v.diffuseColor,
					vertexSpecular = v.specularColor,
					vertexShininess = v.shininess,
					texturePosition = v.texturePos
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

				from f in Shader.Inputs<TexturedFragment> ()
				from u in Shader.Uniforms<EntityUniforms> ()
				let samplerNo = (f.texturePosition.X / 10f).Truncate ()
				let fragDiffuse =
					samplerNo == 0 ? FragmentShaders.TextureColor ((!u.samplers)[0], f.texturePosition) :
					//samplerNo == 1 ? FragmentShaders.TextureColor ((!u.samplers)[1], f.texturePosition - new Vec2 (10f)) :
					//samplerNo == 2 ? FragmentShaders.TextureColor ((!u.samplers)[2], f.texturePosition - new Vec2 (20f)) :
					samplerNo == 3 ? FragmentShaders.TextureColor ((!u.samplers)[3], f.texturePosition - new Vec2 (30f)) :
					f.vertexDiffuse
				let diffuse = Lighting.DirLightDiffuseIntensity (!u.directionalLight, f.vertexNormal) * fragDiffuse
				let specular = (!u.pointLights).Aggregate
				(
					new Vec3 (0f),
					(spec, pl) =>
						pl.intensity == new Vec3 (0f) ?
							spec :
							spec + Lighting.PointLightIntensity (pl, f.vertexPosition, f.vertexNormal, fragDiffuse,
								f.vertexSpecular, f.vertexShininess)
				)
				select new
				{
					outputColor = Lighting.GlobalLightIntensity (!u.globalLighting, fragDiffuse, diffuse + specular)
				}
			);
		}
	}
}
