namespace Compose3D.Shaders
{
	using GLTypes;
	using Maths;
	using OpenTK.Graphics.OpenGL4;

	[GLType ("gl_PerVertex")]
	public class PerVertexIn
	{
		[Builtin]
		public Vec4 gl_Position;
		[Builtin]
		public float gl_PointSize;
		[Builtin]
		public float[] gl_ClipDistance;
	}
	
	public class PerVertexOut : PerVertexIn
	{
		[Builtin]
		public int gl_Layer;
	}

	public class Primitive
	{
		[Builtin]
		public PerVertexIn[] gl_in;
		[Builtin]
		public int gl_PrimitiveIDIn;
		[Builtin]
		public int gl_InvocationID;
	}

	public static class GeometryShaders
	{
		public static GLShader Passthrough<P, V> ()
			where P : Primitive
			where V : PerVertexOut, new ()
		{
			return GLShader.CreateGeometryShader<V> (3, 0,
				PrimitiveType.Triangles, PrimitiveType.TriangleStrip, () =>
				from p in Shader.Inputs<P> ()
				select new V[3]
				{
					new V() { gl_Position = p.gl_in[0].gl_Position, gl_Layer = 0 },
					new V() { gl_Position = p.gl_in[1].gl_Position, gl_Layer = 0 },
					new V() { gl_Position = p.gl_in[2].gl_Position, gl_Layer = 0 }
				}
			);
		}
	}
}
