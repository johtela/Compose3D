namespace Compose3D.Shaders
{
	using GLTypes;
	using Maths;
	using OpenTK.Graphics.OpenGL;

	public class PerVertex
	{
		[Builtin]
		public Vec4 gl_Position;
		[Builtin]
		float gl_PointSize;
		[Builtin]
		float[] gl_ClipDistance;
	}

	public class Primitive
	{
		[Builtin]
		public PerVertex[] gl_in;
		[Builtin]
		public int gl_PrimitiveIDIn;
		[Builtin]
		public int gl_InvocationID;
	}

	public static class GeometryShaders
	{
		public static GLShader Passthrough<P, V> ()
			where P : Primitive
			where V : PerVertex, new ()
		{
			return GLShader.CreateGeometryShader (3,
				PrimitiveType.Triangles, PrimitiveType.TriangleStrip, () =>
				from p in Shader.Inputs<P> ()
				select new V[3]
				{
					new V() { gl_Position = p.gl_in[0].gl_Position },
					new V() { gl_Position = p.gl_in[1].gl_Position },
					new V() { gl_Position = p.gl_in[2].gl_Position }
				}
			);
		}
	}
}
