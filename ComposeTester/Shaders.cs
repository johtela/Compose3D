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
		internal Vec3 color;
		internal Vec3 normal;
		[OmitInGlsl] internal int tag;

		public Vec3 Position
		{
			get { return position; }
			set { position = value; }
		}

		public Vec3 DiffuseColor
		{
			get { return color; }
			set { color = value; }
		}

		public Vec3 SpecularColor
		{
			get { return color; }
			set { color = value; }
		}

		public Vec3 Normal
		{
			get { return normal; }
			set 
			{
				if (float.IsNaN (value.X) || float.IsNaN (value.Y) || float.IsNaN (value.Z))
					throw new ArgumentException ("Normal component NaN");
				normal = value; 
			}
		}

		public int Tag
		{
			get { return tag; }
			set { tag = value; }
		}

		public override string ToString ()
		{
			return string.Format ("[Vertex: Position={0}, DiffuseColor={1}, SpecularColor={2}, Normal={3}, Tag={4}]", 
				Position, DiffuseColor, SpecularColor, Normal, Tag);
		}
	}

	public class Fragment
	{
		[Builtin] internal Vec4 gl_Position = new Vec4 ();
		internal Vec3 vertexPosition = new Vec3 ();
		internal Vec3 vertexNormal = new Vec3 ();
		internal Vec3 vertexColor = new Vec3 ();
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

	public class Uniforms
	{
		internal Uniform<Mat4> worldMatrix;
		internal Uniform<Mat4> perspectiveMatrix;
		internal Uniform<Mat3> normalMatrix;
		internal Uniform<Vec3> ambientLightIntensity;
		internal Uniform<DirectionalLight> directionalLight;
		internal Uniform<PointLight> pointLight;
//		[GLArray (1)]
//		internal Uniform<SpotLight[]> spotLights;
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
		public static readonly Func<PointLight, Vec3, Vec3, Vec3> CalcPointLight =
			GLShader.Function (() => CalcPointLight,
				(pointLight, position, normal) => 
				(from vecToLight in (pointLight.position - position).ToShader ()
				 let lightVec = vecToLight.Normalized
				 let cosAngle = lightVec.Dot (normal).Clamp (0f, 1f)
                 let attenuation = CalcAttenuation (pointLight, vecToLight.Length)
				 select pointLight.intensity * cosAngle * attenuation)
				.Evaluate ());

		public static readonly Func<SpotLight, Vec3, Vec3> CalcSpotLight = 
			GLShader.Function (() => CalcSpotLight,
				(spotLight, position) => 
				(from vecToLight in (spotLight.pointLight.position - position).ToShader ()
				 let dist = vecToLight.Length
				 let lightDir = vecToLight.Normalized
				 let attenuation = CalcAttenuation (spotLight.pointLight, dist)
				 let cosAngle = (-lightDir).Dot (spotLight.direction)
				 select spotLight.pointLight.intensity *
				     (cosAngle < spotLight.cosSpotCutoff ? 0f : attenuation * cosAngle.Pow (spotLight.spotExponent)))
				.Evaluate ());

		public static GLShader VertexShader ()
		{
			return GLShader.Create (ShaderType.VertexShader, 
				() =>
				from v in Shader.Inputs<Vertex> ()
				from u in Shader.Uniforms<Uniforms> ()
				let worldPos = !u.worldMatrix * new Vec4 (v.position, 1f)
				select new Fragment () {   
					gl_Position = !u.perspectiveMatrix * worldPos,
					vertexPosition = worldPos [Coord.x, Coord.y, Coord.z],
					vertexNormal = (!u.normalMatrix * v.normal).Normalized,
					vertexColor = v.color
				});
		}

		public static GLShader FragmentShader ()
		{
			return GLShader.Create (ShaderType.FragmentShader, 
				() =>
				from f in Shader.Inputs<Fragment> ()
				from u in Shader.Uniforms<Uniforms> ()
				let diffuse = CalcDirLight (!u.directionalLight, f.vertexNormal) +
				    CalcPointLight (!u.pointLight, f.vertexPosition, f.vertexNormal)
				select new 
				{
					outputColor = (f.vertexColor * (!u.ambientLightIntensity + diffuse)).Clamp (0f, 1f) 
				}
			);
		}
	}
}

