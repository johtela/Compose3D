﻿namespace ComposeTester
{
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.GLTypes;
	using Compose3D.Shaders;
    using Compose3D.Textures;
	using OpenTK.Graphics.OpenGL;
	using System;
	using System.Linq;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
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

	public static class ExampleShaders
	{
		public class Uniforms
		{
			internal Uniform<Mat4> worldMatrix;
			internal Uniform<Mat4> perspectiveMatrix;
			internal Uniform<Mat3> normalMatrix;
			internal Uniform<GlobalLight> globalLighting;
			internal Uniform<DirectionalLight> directionalLight;
			[GLArray (4)] internal Uniform<PointLight[]> pointLights;
			[GLArray (4)] internal Uniform<Sampler2D[]> samplers;
		}

		/// <summary>
		/// The calculate intensity of the directional light.
		/// </summary>
		public static readonly Func<DirectionalLight, Vec3, Vec3> CalcDirLight =
			GLShader.Function 
			(
				() => CalcDirLight,

				(DirectionalLight dirLight, Vec3 normal) => 
					(dirLight.intensity * normal.Dot (dirLight.direction)).Clamp (0f, 1f)
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

		public static readonly Func<Sampler2D, Vec2, Vec3> TextureColor =
			GLShader.Function
			(
				() => TextureColor,

				(Sampler2D sampler, Vec2 texturePos) =>
					sampler.Texture (texturePos)[Coord.x, Coord.y, Coord.z]
			);

		public static GLShader VertexShader ()
		{
			return GLShader.Create 
			(
				ShaderType.VertexShader, () =>

				from v in Shader.Inputs<Vertex> ()
				from u in Shader.Uniforms<Uniforms> ()
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
			return GLShader.Create 
			(
				ShaderType.FragmentShader, () =>
				from f in Shader.Inputs<TexturedFragment> ()
				from u in Shader.Uniforms<Uniforms> ()
				let samplerNo = (f.texturePosition.X / 10f).Truncate ()
				let fragDiffuse =
					samplerNo == 0 ? TextureColor ((!u.samplers)[0], f.texturePosition) :
					samplerNo == 1 ? TextureColor ((!u.samplers)[1], f.texturePosition - new Vec2 (10f)) :
					samplerNo == 2 ? TextureColor ((!u.samplers)[2], f.texturePosition - new Vec2 (20f)) :
					samplerNo == 3 ? TextureColor ((!u.samplers)[3], f.texturePosition - new Vec2 (30f)) :
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

