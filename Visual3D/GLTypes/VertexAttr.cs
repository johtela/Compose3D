﻿namespace Visual3D.GLTypes
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;

	public class VertexAttr
	{
		// Name of the attribute
		public readonly string Name;
		// Type of the data items in the struct
		public VertexAttribPointerType PointerType;
		// Size of the struct
		public readonly int Size;
		// Number of items in the struct
		public readonly int Count;

		public VertexAttr (string name, VertexAttribPointerType ptype, int size, int count)
		{
			Name = name;
			PointerType = ptype;
			Size = size;
			Count = count;
		}

		private static VertexAttr FromFieldInfo (FieldInfo fi)
		{
			var ft = fi.FieldType;
			var s = Marshal.SizeOf (ft);
			var name = fi.Name;

			if (ft == typeof (Vector3))
				return new VertexAttr (name, VertexAttribPointerType.Float, s, 3);
			else if (ft == typeof (Vector4))
				return new VertexAttr (name, VertexAttribPointerType.Float, s, 4);
			else throw new ArgumentException ("Incompatible vertex attribute type " + name);
		}

		public static IEnumerable<VertexAttr> GetAttributes<T> () where T : struct
		{
			return from fi in typeof (T).GetFields (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
				   select FromFieldInfo (fi);
		}
	}
}
