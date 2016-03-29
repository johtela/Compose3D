namespace Compose3D.Shaders
{
	using System;
	using System.Runtime.InteropServices;
	using Maths;
	using GLTypes;
	using Geometry;
	using OpenTK.Graphics.OpenGL;

	[StructLayout (LayoutKind.Sequential, Pack = 4)]
	public struct BasicVertex : IVertex
	{
		public Vec3 position;
		[OmitInGlsl]
		public Vec3 normal;

		Vec3 IPositional<Vec3>.Position
		{
			get { return position; }
			set { position = value; }
		}

		Vec3 IPlanar<Vec3>.Normal
		{
			get { return normal; }
			set
			{
				if (value.IsNan ())
					throw new ArgumentException ("Normal component NaN.");
				normal = value;
			}
		}

		public override string ToString ()
		{
			return string.Format ("[Vertex: Position={0}, Normal={1}]", position, normal);
		}
	}

	public static class VertexShaders
	{
		public static GLShader Passthrough<F> () where F : Fragment, new ()
		{
			return GLShader.Create (ShaderType.VertexShader, () =>
				from v in Shader.Inputs<BasicVertex> ()
				select new F ()
				{
					gl_Position = new Vec4( v.position, 1f)
				});
		}

	}
}