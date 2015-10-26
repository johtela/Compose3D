namespace ComposeTester
{
	using Compose3D.Arithmetics;
	using Compose3D.Geometry;
	using Compose3D.GLTypes;
    using Compose3D.Textures;
	using OpenTK.Graphics.OpenGL;
	using System;
	using System.Linq;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public struct Vertex : IVertex, IVertexInitializer<Vertex>, IVertexColor, ITextured
	{
		internal Vec3 position;
        internal Vec3 normal;
        internal Vec3 diffuseColor;
        internal Vec3 specularColor;
		internal Vec2 texturePos;
        internal float shininess;
        [OmitInGlsl]
        internal int tag;

        Vec3 IPositional.Position
		{
			get { return position; }
			set { position = value; }
		}

        Vec3 IVertexColor.Diffuse
		{
			get { return diffuseColor; }
			set { diffuseColor = value; }
		}

        Vec3 IVertexColor.Specular
		{
			get { return specularColor; }
			set { specularColor = value; }
		}

        float IVertexColor.Shininess
        {
            get { return shininess; }
            set { shininess = value; }
        }

        Vec3 IVertex.Normal
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

	public static class Shaders
	{
		public class Fragment
		{
			[Builtin]
			internal Vec4 gl_Position;
			internal Vec3 vertexPosition;
			internal Vec3 vertexNormal;
			internal Vec3 vertexDiffuse;
			internal Vec3 vertexSpecular;
			internal float vertexShininess;
			internal Vec2 texturePosition;
		}

		[GLStruct ("DirLight")]
		public struct DirLight
		{
			internal Vec3 intensity;
			internal Vec3 direction;
		}

		[GLStruct ("PointLight")]
		public struct PointLight
		{
			internal Vec3 position;
			internal Vec3 intensity;
			internal float linearAttenuation, quadraticAttenuation;
		}

		[GLStruct ("SpotLight")]
		public struct SpotLight
		{
			internal PointLight pointLight;
			internal Vec3 direction;
			internal float cosSpotCutoff, spotExponent;
		}

		[GLStruct ("GlobalLight")]
		public struct GlobalLight
		{
			internal Vec3 ambientLightIntensity;
			internal float maxintensity;
			internal float inverseGamma;
		}

		public class Uniforms
		{
			internal Uniform<Mat4> worldMatrix;
			internal Uniform<Mat4> perspectiveMatrix;
			internal Uniform<Mat3> normalMatrix;
			internal Uniform<GlobalLight> globalLighting;
			internal Uniform<DirLight> directionalLight;
			[GLArray (4)] internal Uniform<PointLight[]> pointLights;
			[GLArray (4)] internal Uniform<Sampler2D[]> samplers;
		}

		/// <summary>
		/// The calculate intensity of the directional light.
		/// </summary>
		public static readonly Func<DirLight, Vec3, Vec3> CalcDirLight =
			GLShader.Function 
			(
				() => CalcDirLight,

				(DirLight dirLight, Vec3 normal) => 
				Shader.Evaluate 
				(
					from cosAngle in normal.Dot (dirLight.direction).ToShader ()
					select dirLight.intensity * cosAngle.Clamp (0f, 1f)
				)
			);

		/// <summary>
		/// Calculate attenuation of a point light.
		/// </summary>
        public static readonly Func<PointLight, float, float> CalcAttenuation = 
			GLShader.Function 
			(
				() => CalcAttenuation, 

				(PointLight pointLight, float distance) => 
					(1f / ((pointLight.linearAttenuation * distance) + 
						(pointLight.quadraticAttenuation * distance * distance))).Clamp (0f, 1f)
			);

		/// <summary>
		/// Calculate intensity of a point light.
		/// </summary>
		public static readonly Func<PointLight, Vec3, Vec3, Vec3, Vec3, float, Vec3> CalcPointLight =
			GLShader.Function
			(
				() => CalcPointLight,

				(PointLight pointLight, Vec3 position, Vec3 normal, Vec3 diffuse, Vec3 specular, float shininess) =>
				Shader.Evaluate
				(
					from vecToLight in (pointLight.position - position).ToShader ()
					let lightVec = vecToLight.Normalized
					let cosAngle = lightVec.Dot (normal).Clamp (0f, 1f)
					let attenIntensity = CalcAttenuation (pointLight, vecToLight.Length) * pointLight.intensity
					let viewDir = -position.Normalized
					let halfAngle = (lightVec + viewDir).Normalized
					let blinn = cosAngle == 0f ? 0f :
					   normal.Dot (halfAngle).Clamp (0f, 1f).Pow (shininess)
					select (diffuse * attenIntensity * cosAngle) + (specular * attenIntensity * blinn)
				)
			);

//		public static readonly Func<SpotLight, Vec3, Vec3> CalcSpotLight = 
//			GLShader.Function (() => CalcSpotLight,
//				(spotLight, position) => 
//				(from vecToLight in (spotLight.pointLight.position - position).ToShader ()
//				 let dist = vecToLight.Length
//				 let lightDir = vecToLight.Normalized
//				 let attenuation = CalcAttenuation (spotLight.pointLight, dist)
//				 let cosAngle = (-lightDir).Dot (spotLight.direction)
//				 select spotLight.pointLight.intensity *
//				     (cosAngle < spotLight.cosSpotCutoff ? 0f : attenuation * cosAngle.Pow (spotLight.spotExponent)))
//				.Evaluate ());

		public static readonly Func<GlobalLight, Vec3, Vec3, Vec3> CalcGlobalLight =
			GLShader.Function
			(
				() => CalcGlobalLight,

				(GlobalLight globalLight, Vec3 diffuse, Vec3 other) =>
				Shader.Evaluate
				(
					from gamma in new Vec3 (globalLight.inverseGamma).ToShader ()
					let maxInten = globalLight.maxintensity
					let ambient = diffuse * globalLight.ambientLightIntensity
					select ((ambient + other).Pow (gamma) / maxInten).Clamp (0f, 1f)
				)
			);

		public static readonly Func<Sampler2D, Vec2, Vec3, Vec3> GetFragmentDiffuse =
			GLShader.Function
			(
				() => GetFragmentDiffuse,

				(Sampler2D sampler, Vec2 texturePos, Vec3 other) =>
				Shader.Evaluate
				(
					from textColor in sampler.Texture (texturePos).ToShader ()
					select textColor == new Vec4 (0f, 0f, 0f, 1f) ? other : textColor[Coord.x, Coord.y, Coord.z]
				)
			);

		public static GLShader VertexShader ()
		{
			return GLShader.Create 
			(
				ShaderType.VertexShader, () =>

				from v in Shader.Inputs<Vertex> ()
				from u in Shader.Uniforms<Uniforms> ()
				let worldPos = !u.worldMatrix * new Vec4 (v.position, 1f)
				select new Fragment ()
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
			return GLShader.Create 
			(
				ShaderType.FragmentShader, () =>
				from f in Shader.Inputs<Fragment> ()
				from u in Shader.Uniforms<Uniforms> ()
				let samplerNo = (f.texturePosition.X / 10f).Truncate ()
				let fragDiffuse =
					samplerNo == 0 ? GetFragmentDiffuse ((!u.samplers)[0], f.texturePosition, f.vertexDiffuse) :
					samplerNo == 1 ? GetFragmentDiffuse ((!u.samplers)[1], f.texturePosition - new Vec2 (10f), f.vertexDiffuse) :
					samplerNo == 2 ? GetFragmentDiffuse ((!u.samplers)[2], f.texturePosition - new Vec2 (20f), f.vertexDiffuse) :
					samplerNo == 3 ? GetFragmentDiffuse ((!u.samplers)[3], f.texturePosition - new Vec2 (30f), f.vertexDiffuse) :
					f.vertexDiffuse
				let diffuse = CalcDirLight (!u.directionalLight, f.vertexNormal) * fragDiffuse
				let specular = (!u.pointLights).Aggregate
				(
					new Vec3 (0f),
					(spec, pl) =>
						pl.intensity == new Vec3 (0f) ?
							spec :
							spec + CalcPointLight (pl, f.vertexPosition, f.vertexNormal, fragDiffuse,
								f.vertexSpecular, f.vertexShininess)
				)
				select new 
				{
					outputColor = CalcGlobalLight (!u.globalLighting, fragDiffuse, diffuse + specular)
				}
			);
		}
	}
}

