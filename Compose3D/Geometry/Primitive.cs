﻿namespace Compose3D.Geometry
{
    using Compose3D.Maths;
    using OpenTK;
    using System;
    using System.Collections.Generic;

    public abstract class Primitive<V> : Geometry<V> where V : struct, IVertex3D
    {
		protected V[] _vertices;

		protected Primitive (V[] vertices)
        {
			_vertices = vertices;
        }

        protected override IEnumerable<V> GenerateVertices ()
        {
			return _vertices;
        }
    }
}
