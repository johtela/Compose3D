namespace ComposeFX.Graphics.GLTypes
{
    using System;
	using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using OpenTK.Graphics.OpenGL4;

	public class GLProgram : GLObject
	{
		private int _prevProgram;

		internal int _glProgram;
		private Dictionary<object, int> _vertexArrays = new Dictionary<object, int> ();

		public GLProgram (int glProgram)
		{
			_glProgram = glProgram;
		}

		public GLProgram (params GLShader[] shaders)
		{
			_glProgram = GL.CreateProgram ();
			foreach (var shader in shaders)
				GL.AttachShader (_glProgram, shader._glShader);
			GL.LinkProgram (_glProgram);
			var log = GL.GetProgramInfoLog (_glProgram);
			if (log.ToUpper ().Contains ("ERROR:"))
				throw new GLError (string.Format ("Program linking error:\n{0}", log));
        }

		private int CreateVertexArray<V> (VBO<V> vertices) where V : struct
		{
			int vao = GL.GenVertexArray ();
			GL.BindVertexArray (vao);
			var recSize = Marshal.SizeOf (typeof (V));
			GL.BindBuffer (BufferTarget.ArrayBuffer, vertices._glvbo);
			foreach (var attr in VertexAttr.GetAttributes<V> ())
			{
				var index = GL.GetAttribLocation (_glProgram, attr.Name);
				if (index >= 0)
				{
					GL.EnableVertexAttribArray (index);
					var offset = Marshal.OffsetOf (typeof (V), attr.Name);
					GL.VertexAttribPointer (index, attr.Count, attr.PointerType, false, recSize, offset);
				}
			}
			return vao;
		}

		private void BindVertices<V> (VBO<V> vertices) where V : struct
		{
			int vao;
			if (_vertexArrays.TryGetValue (vertices, out vao))
			{
				GL.BindVertexArray (vao);
				GL.BindBuffer (BufferTarget.ArrayBuffer, vertices._glvbo);
			}
			else
				_vertexArrays.Add (vertices, CreateVertexArray<V> (vertices));
		}

		public override void Use ()
		{
			GL.GetInteger (GetPName.CurrentProgram, out _prevProgram);
			GL.UseProgram (_glProgram);
		}

		public override void Release ()
		{
			GL.UseProgram (_prevProgram);
		}

		public void DrawElements<V> (PrimitiveType primitive, VBO<V> vertices, VBO<int> indices) 
			where V : struct
		{
			BindVertices (vertices);
			GL.BindBuffer (BufferTarget.ElementArrayBuffer, indices._glvbo);
			GL.DrawElements (primitive, indices._count, DrawElementsType.UnsignedInt, 0);
			GL.BindBuffer (BufferTarget.ElementArrayBuffer, 0);
			GL.BindBuffer (BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray (0);
		}

		public void DrawElementsInstanced<V> (PrimitiveType primitive, VBO<V> vertices, 
			VBO<int> indices, int instanceCount) where V : struct
		{
			BindVertices (vertices);
			GL.BindBuffer (BufferTarget.ElementArrayBuffer, indices._glvbo);
			GL.DrawElementsInstanced (primitive, indices._count, DrawElementsType.UnsignedInt,
				IntPtr.Zero, instanceCount);
			GL.BindBuffer (BufferTarget.ElementArrayBuffer, 0);
			GL.BindBuffer (BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray (0);
		}

		public void DrawNormals<V> (VBO<V> vertices) where V : struct
		{
			BindVertices (vertices);
			GL.DrawArrays (PrimitiveType.Lines, 0, vertices._count);
			GL.BindBuffer (BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray (0);
		}

		public void DrawLinePath<V> (VBO<V> vertices) where V : struct
		{
			BindVertices (vertices);
			GL.DrawArrays (PrimitiveType.LineStrip, 0, vertices._count);
			GL.BindBuffer (BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray (0);
		}
	}
}