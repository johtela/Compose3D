namespace Compose3D.GLTypes
{
    using System;
    using System.Runtime.InteropServices;
    using OpenTK.Graphics.OpenGL;
    using Compose3D.Geometry;
    using System.Reflection;

	public class Program
	{
		internal int _glProgram;

		public Program (int glProgram)
		{
			_glProgram = glProgram;
		}

		public Program (params GLShader[] shaders)
		{
			_glProgram = GL.CreateProgram ();
			foreach (var shader in shaders)
				GL.AttachShader (_glProgram, shader._glShader);
			GL.LinkProgram (_glProgram);
			var log = GL.GetProgramInfoLog (_glProgram);
			if (log.ToUpper ().Contains ("ERROR:"))
				throw new GLError (string.Format ("Program linking error:\n{0}", log));
			GL.UseProgram (_glProgram);
        }

        public Uniform<T> GetUniform<T> (string name)
        {
            return new Uniform<T> (this, name);
        }

        public void InitializeUniforms<U> (U uniforms) where U : class
        {
            foreach (var field in typeof (U).GetUniforms ())
                field.SetValue (uniforms, Activator.CreateInstance (field.FieldType, this, field));
        }

		void BindVertices<V> (VBO<V> vertices) where V : struct, IVertex
		{
			var recSize = Marshal.SizeOf (typeof(V));
			var offset = 0;
			GL.BindBuffer (BufferTarget.ArrayBuffer, vertices._glvbo);
			foreach (var attr in VertexAttr.GetAttributes<V> ())
			{
				var index = GL.GetAttribLocation (_glProgram, attr.Name);
				if (index >= 0)
				{
					GL.EnableVertexAttribArray (index);
					GL.VertexAttribPointer (index, attr.Count, attr.PointerType, false, recSize, offset);
				}
				offset += attr.Size;
			}
		}

		public void DrawTriangles<V> (VBO<V> vertices, VBO<int> indices) where V : struct, IVertex
		{
			BindVertices (vertices);
			GL.BindBuffer (BufferTarget.ElementArrayBuffer, indices._glvbo);
			GL.DrawElements (PrimitiveType.Triangles, indices._count, DrawElementsType.UnsignedInt, 0);
			GL.BindBuffer (BufferTarget.ElementArrayBuffer, 0);
			GL.BindBuffer (BufferTarget.ArrayBuffer, 0);
		}

		public void DrawNormals<V> (VBO<V> vertices) where V : struct, IVertex
		{
			BindVertices (vertices);
			GL.DrawArrays (PrimitiveType.Lines, 0, vertices._count);
			GL.BindBuffer (BufferTarget.ArrayBuffer, 0);
		}
	}
}
