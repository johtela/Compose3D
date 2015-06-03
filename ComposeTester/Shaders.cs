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
			set { normal = value; }
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
		[GLArray (4)] 
		internal Uniform<SpotLight[]> spotLights;
	}

	public static class Shaders
	{
		public static readonly Func<SpotLight, float, float> Attenuation = 
			GLShader.Function (
				() => Attenuation,
				(sp, d) => 1f / ((sp.linearAttenuation * d) + (sp.quadraticAttenuation * d * d)));

		public static readonly Func<SpotLight, Vec3, Vec3> CalcSpotLight = 
			GLShader.Function (
				() => CalcSpotLight,
				(sp, v) => (from vecToLight in (sp.position - v).ToShader ()
				            let dist = vecToLight.Length
				            let lightDir = vecToLight.Normalized
				            let attenuation = Attenuation (sp, dist)
				            let cosAngle = -lightDir.Dot (sp.direction)
				            select sp.intensity *
				                (cosAngle < sp.cosSpotCutoff ? 0f : attenuation * cosAngle.Pow (sp.spotExponent)))
				.Evaluate ());

		public static GLShader VertexShader ()
		{
			return GLShader.Create (ShaderType.VertexShader, 
				() => 
				from v in Shader.Inputs<Vertex> ()
				from u in Shader.Uniforms<Uniforms> ()
				let normalizedNormal = (!u.normalMatrix * v.normal).Normalized
				let angle = normalizedNormal.Dot ((!u.directionalLight).direction)
				let ambient = new Vec4 (!u.ambientLightIntensity, 0f)
				let diffuse = new Vec4 ((!u.directionalLight).intensity, 0f) * angle
				let spot = (from sp in !u.spotLights
					select CalcSpotLight (sp, v.position)).
							Aggregate (new Vec3 (0f), (r, i) => r + i)
				select new Fragment () 
				{   
					gl_Position = !u.perspectiveMatrix * !u.worldMatrix * new Vec4 (v.position, 1f),
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

