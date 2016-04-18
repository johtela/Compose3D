namespace Compose3D.GLTypes
{
    using System;
    using System.Runtime.InteropServices;
    using OpenTK.Graphics.OpenGL;

	public class Program : GLObject
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
            GC.Collect ();
        }

		public override void Use ()
		{
			GL.UseProgram (_glProgram);
		}

		public override void Release ()
		{
			GL.UseProgram (0);
		}

		void BindVertices<V> (VBO<V> vertices) where V : struct
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

		public void DrawElements<V> (PrimitiveType primitive, VBO<V> vertices, VBO<int> indices) 
			where V : struct
		{
			BindVertices (vertices);
			GL.BindBuffer (BufferTarget.ElementArrayBuffer, indices._glvbo);
			GL.DrawElements (primitive, indices._count, DrawElementsType.UnsignedInt, 0);
			GL.BindBuffer (BufferTarget.ElementArrayBuffer, 0);
			GL.BindBuffer (BufferTarget.ArrayBuffer, 0);
		}

		public void DrawNormals<V> (VBO<V> vertices) where V : struct
		{
			BindVertices (vertices);
			GL.DrawArrays (PrimitiveType.Lines, 0, vertices._count);
			GL.BindBuffer (BufferTarget.ArrayBuffer, 0);
			GL.UseProgram (0);
		}

		public void DrawLinePath<V> (VBO<V> vertices) where V : struct
		{
			BindVertices (vertices);
			GL.DrawArrays (PrimitiveType.LineStrip, 0, vertices._count);
			GL.BindBuffer (BufferTarget.ArrayBuffer, 0);
		}
	}
}