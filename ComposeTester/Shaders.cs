namespace ComposeTester
{
	using Compose3D.Arithmetics;
	using Compose3D.Geometry;
	using Compose3D.GLTypes;
	using OpenTK.Graphics.OpenGL;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public struct Vertex : IVertex
	{
		internal Vec3 position;
        internal Vec3 normal;
        internal Vec3 diffuseColor;
        internal Vec3 specularColor;
		internal Vec2 textureCoord;
        internal float shininess;
        [OmitInGlsl]
        internal int tag;

        Vec3 IPositional.Position
		{
			get { return position; }
			set { position = value; }
		}

        Vec3 IVertexMaterial.DiffuseColor
		{
			get { return diffuseColor; }
			set { diffuseColor = value; }
		}

        Vec3 IVertexMaterial.SpecularColor
		{
			get { return specularColor; }
			set { specularColor = value; }
		}

        float IVertexMaterial.Shininess
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

		Vec2 IVertex.TextureCoord
		{
			get { return textureCoord; }
			set { textureCoord = value; }
		}

        int IVertex.Tag
		{
			get { return tag; }
			set { tag = value; }
		}

		public override string ToString ()
		{
			return string.Format ("[Vertex: Position={0}, DiffuseColor={1}, SpecularColor={2}, Normal={3}, Tag={4}]", 
				position, diffuseColor, specularColor, normal, tag);
		}
	}

	public class Fragment
	{
		[Builtin] internal Vec4 gl_Position;
		internal Vec3 vertexPosition;
        internal Vec3 vertexNormal;
		internal Vec3 vertexDiffuse;
        internal Vec3 vertexSpecular;
        internal float vertexShininess;
    }

	[GLStruct ("DirectionalLight")]
	public struct DirectionalLight
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

	[GLStruct ("GlobalLighting")]
	public struct GlobalLighting
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
		internal Uniform<GlobalLighting> globalLighting;
		internal Uniform<DirectionalLight> directionalLight;
		internal Uniform<PointLight> pointLight;
		//[GLArray (1)]
		//internal Uniform<SpotLight[]> spotLights;
	}

	public static class Shaders
	{
		/// <summary>
		/// The calculate intensity of the directional light.
		/// </summary>
		public static readonly Func<DirectionalLight, Vec3, Vec3> CalcDirLight =
			GLShader.Function (() => CalcDirLight,
				(dirLight, normal) => 
				(from cosAngle in normal.Dot (dirLight.direction).ToShader ()
				 select dirLight.intensity * cosAngle.Clamp (0f, 1f))
				.Evaluate ());

		/// <summary>
		/// Calculate attenuation of a point light.
		/// </summary>
        public static readonly Func<PointLight, float, float> CalcAttenuation =
            GLShader.Function (() => CalcAttenuation,
                (pointLight, distance) => 
                (1f / ((pointLight.linearAttenuation * distance) + 
					(pointLight.quadraticAttenuation * distance * distance))).Clamp (0f, 1f));

		/// <summary>
		/// Calculate intensity of the directional light.
		/// </summary>
		public static readonly Func<PointLight, Vec3, Vec3, Vec3, Vec3, float, Vec3> CalcPointLight =
			GLShader.Function (() => CalcPointLight,
				(pointLight, position, normal, diffuse, specular, shininess) => 
				(from vecToLight in (pointLight.position - position).ToShader ()
				 let lightVec = vecToLight.Normalized
				 let cosAngle = lightVec.Dot (normal).Clamp (0f, 1f)
				 let attenIntensity = CalcAttenuation (pointLight, vecToLight.Length) * pointLight.intensity
				 let viewDir = -position.Normalized
				 let halfAngle = (lightVec + viewDir).Normalized
				 let blinn = cosAngle == 0f ? 0f :
					normal.Dot (halfAngle).Clamp (0f, 1f).Pow (shininess)
				 select (diffuse * attenIntensity * cosAngle) + (specular * attenIntensity * blinn))
				.Evaluate ());

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

		public static readonly Func<GlobalLighting, Vec3, Vec3, Vec3> CalcGlobalLight =
			GLShader.Function (() => CalcGlobalLight,
				(globalLight, diffuse, other) =>
				(from gamma in new Vec3 (globalLight.inverseGamma).ToShader ()
				 let maxInten = globalLight.maxintensity
				 let ambient = diffuse * globalLight.ambientLightIntensity
					select ((ambient + other).Pow (gamma) / maxInten).Clamp (0f, 1f))
				.Evaluate ());

		public static GLShader VertexShader ()
		{
			return GLShader.Create (ShaderType.VertexShader, 
				() =>
				from v in Shader.Inputs<Vertex> ()
				from u in Shader.Uniforms<Uniforms> ()
				let worldPos = !u.worldMatrix * new Vec4 (v.position, 1f)
				select new Fragment () 
				{   
					gl_Position = !u.perspectiveMatrix * worldPos,
					vertexPosition = worldPos [Coord.x, Coord.y, Coord.z],
					vertexNormal = (!u.normalMatrix * v.normal).Normalized,
					vertexDiffuse = v.diffuseColor,
					vertexSpecular = v.specularColor,
                    vertexShininess = v.shininess
				});
		}

		public static GLShader FragmentShader ()
		{
			return GLShader.Create (ShaderType.FragmentShader, 
				() =>
				from f in Shader.Inputs<Fragment> ()
				from u in Shader.Uniforms<Uniforms> ()
				let diffuse = CalcDirLight (!u.directionalLight, f.vertexNormal)
				let specular = CalcPointLight (!u.pointLight, f.vertexPosition, f.vertexNormal, 
					    f.vertexDiffuse, f.vertexSpecular, f.vertexShininess)
				select new 
				{
					outputColor = CalcGlobalLight (!u.globalLighting, f.vertexDiffuse, diffuse + specular)
				});
		}
	}
}

