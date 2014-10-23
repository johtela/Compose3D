namespace Visual3D.GLTypes
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;

	public class Program
	{
		internal int _glProgram;

		public Program (int glProgram)
		{
			_glProgram = glProgram;
		}

		public Program (params Shader[] shaders)
		{
			_glProgram = GL.CreateProgram ();
			foreach (var shader in shaders)
				GL.AttachShader (_glProgram, shader._glShader);
			GL.LinkProgram (_glProgram);
			var log = GL.GetProgramInfoLog (_glProgram);
			if (log.Contains ("ERROR"))
				throw new GLError (string.Format ("Program linking error:\n{0}", log));
			GL.UseProgram (_glProgram);
		}

		public int GetVertexAttrIndex (string name)
		{
			var index = GL.GetAttribLocation (_glProgram, name);
			if (index < 0)
				throw new GLError (string.Format ("Attribute '{0}' was not found in program", name));
			return index;
		}

		public Uniform<T> GetUniform<T> (string name) where T : struct
		{
			var loc = GL.GetUniformLocation (_glProgram, name);
			if (loc < 0)
				throw new GLError (string.Format ("Uniform '{0}' was not found in program", name));
			return new Uniform<T> (loc);
		}

		public void DrawVertexBuffer<V> (VBO<V> vertices, VBO<int> indices) where V : struct, IVertex
		{
			var recSize = Marshal.SizeOf (typeof(V));
			var offset = 0;

			GL.BindBuffer (BufferTarget.ArrayBuffer, vertices._glvbo);
			foreach (var attr in VertexAttr.GetAttributes<V>())
			{
				var index = GetVertexAttrIndex (attr.Name);
				GL.EnableVertexAttribArray (index);
				GL.VertexAttribPointer (index, attr.Count, attr.PointerType, false, recSize, offset);
				offset += attr.Size;
			}
			GL.BindBuffer (BufferTarget.ElementArrayBuffer, indices._glvbo);
			GL.DrawElements (PrimitiveType.Triangles, indices._count, DrawElementsType.UnsignedInt, 0);
		}
	}
}
