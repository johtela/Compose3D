namespace Visual3D.GLTypes
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using System.Linq;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;

	public class VBO<T> where T : struct
	{
		internal int _glvbo;
		internal int _count;

		public VBO (int glvbo, int count)
		{
			_glvbo = glvbo;
			_count = count;
		}

		public VBO (IEnumerable<T> elements, BufferTarget bufferType)
		{
			_glvbo = GL.GenBuffer ();
			var varr = elements.ToArray ();
			var size = new IntPtr (Marshal.SizeOf (typeof (T)) * varr.Length);

			GL.BindBuffer (bufferType, _glvbo);
			GL.BufferData (bufferType, size, varr, BufferUsageHint.StaticDraw);
			_count = varr.Length;
		}
	}
}
