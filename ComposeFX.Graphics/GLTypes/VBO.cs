namespace ComposeFX.Graphics.GLTypes
{
    using OpenTK.Graphics.OpenGL4;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

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
			: this (elements.ToArray (), bufferType) { }

		public VBO (T[] elements, BufferTarget bufferType)
		{
			_glvbo = GL.GenBuffer ();
			var recSize = Marshal.SizeOf (typeof (T));
			var size = new IntPtr (recSize  * elements.Length);

			GL.BindBuffer (bufferType, _glvbo);
			GL.BufferData (bufferType, size, elements, BufferUsageHint.StaticDraw);
			_count = elements.Length;
		}

		public override bool Equals (object obj)
		{
			var other = obj as VBO<T>;
			return other != null && other._glvbo == _glvbo;
		}

		public override int GetHashCode ()
		{
			return _glvbo.GetHashCode ();
		}
	}
}