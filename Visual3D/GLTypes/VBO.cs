namespace Visual3D.GLTypes
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using System.Linq;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;

	public class VBO
	{
		internal int _glvbo;
		internal int _count;

		public VBO (int glvbo, int count)
		{
			_glvbo = glvbo;
			_count = count;
		}

		public static VBO Init<V> (IEnumerable<V> vertices, BufferTarget buffertType) where V : struct
		{
			var glvbo = GL.GenBuffer ();
			var varr = vertices.ToArray ();
			var size = new IntPtr (Marshal.SizeOf (typeof (V)) * varr.Length);

			GL.BindBuffer (buffertType, glvbo);
			GL.BufferData (buffertType, size, varr, BufferUsageHint.StaticDraw);
			return new VBO (glvbo, varr.Length);
		}
	}
}
