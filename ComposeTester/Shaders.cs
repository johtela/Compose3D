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
		internal Vec4 color;
		internal Vec3 normal;
		[OmitInGlsl] internal int tag;

		public Vec3 Position
		{
			get { return position; }
			set { position = value; }
		}

		public Vec4 Color
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
			return string.Format ("[Vertex: Position={0}, Color={1}, Normal={2}, Tag={3}]", 
				Position, Color, Normal, Tag);
		}
	}

	public class Fragment
	{
		[Builtin] internal Vec4 gl_Position = new Vec4 ();
		[GLQualifier ("smooth")]
		internal Vec4 theColor = new Vec4 ();
	}

	public class FrameBuffer
	{
		internal Vec4 outputColor = new Vec4 ();
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
	}

	[GLStruct ("SpotLight")]
	public struct SpotLight
	{
		internal Vec3 position;
		internal Vec3 direction;
		internal Vec3 intensity;
		internal float linearAttenuation, quadraticAttenuation;
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
		public static readonly Func<DirectionalLight, Vec3, Vec4> CalcDirLight =
			GLShader.Function (() => CalcDirLight,
				(dirLight, normal) => 
				(from cosAngle in normal.Dot (dirLight.direction).ToShader ()
				 select new Vec4 (dirLight.intensity, 0f) * cosAngle.Clamp (0f, 1f))
				.Evaluate ());

		public static readonly Func<PointLight, Vec3, Vec3, Vec4> CalcPointLight =
			GLShader.Function (() => CalcPointLight,
				(pointLight, position, normal) => 
				(from vecToLight in (pointLight.position - position).ToShader ()
				 let lightVec = vecToLight.Normalized
				 let cosAngle = lightVec.Dot (normal).Clamp (0f, 1f)
				 select new Vec4 (pointLight.intensity, 0f) * cosAngle)
				.Evaluate ());

		public static readonly Func<SpotLight, float, float> Attenuation = 
			GLShader.Function (() => Attenuation,
				(sp, d) => 1f / ((sp.linearAttenuation * d) + (sp.quadraticAttenuation * d * d)));

		public static readonly Func<SpotLight, Vec3, Vec3> CalcSpotLight = 
			GLShader.Function (() => CalcSpotLight,
				(sp, v) => 
				(from vecToLight in (sp.position - v).ToShader ()
				 let dist = vecToLight.Length
				 let lightDir = vecToLight.Normalized
				 let attenuation = Attenuation (sp, dist)
				 let cosAngle = (-lightDir).Dot (sp.direction)
				 select sp.intensity *
				     (cosAngle < sp.cosSpotCutoff ? 0f : attenuation * cosAngle.Pow (sp.spotExponent)))
				.Evaluate ());

		public static GLShader VertexShader ()
		{
			return GLShader.Create (ShaderType.VertexShader, () =>
				from v in Shader.Inputs<Vertex> ()
			 from u in Shader.Uniforms<Uniforms> ()
			 let transformedNormal = (!u.normalMatrix * v.normal).Normalized
			 let worldPos = !u.worldMatrix * new Vec4 (v.position, 1f)
			 let ambient = new Vec4 (!u.ambientLightIntensity, 0f)
			 let diffuse = CalcDirLight (!u.directionalLight, transformedNormal) +
			     CalcPointLight (!u.pointLight, worldPos [Coord.x, Coord.y, Coord.z], transformedNormal)
			 select new Fragment () {   
				gl_Position = !u.perspectiveMatrix * worldPos,
				theColor = (v.color * (ambient + diffuse)).Clamp (0f, 1f)
			});
		}

		public static GLShader FragmentShader ()
		{
			return GLShader.Create (ShaderType.FragmentShader, () =>
				from f in Shader.Inputs<Fragment> ()
				select new { outputColor = f.theColor });
		}
	}
}

