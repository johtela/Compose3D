﻿namespace Compose3D.Geometry
{
    using Arithmetics;
    using OpenTK;
    using System;
    using System.Collections.Generic;

    public class Circular<V> : Primitive<V> where V : struct, IVertex
    {
		bool _fullArc;

		private Circular (Func<Geometry<V>, V[]> generateVertices, IMaterial material, bool fullArc)
            : base (generateVertices, material)
        {
			_fullArc = fullArc;
		}

        public static Circular<V> Pie (float width, float height, float stepAngle, 
            float startAngle, float endAngle, IMaterial material)
        {
            if (startAngle > endAngle)
                throw new ArgumentException ("Start angle must be bigger than end angle");
			var fullArc = startAngle == endAngle;
			if (fullArc)
				endAngle += MathHelper.TwoPi;
			var vertCount = (int)Math.Ceiling ((endAngle - startAngle) / stepAngle) + (fullArc ? 1 : 2);
            var normal = new Vec3 (0f, 0f, 1f);
			return new Circular<V> (e =>
			{
				var materials = e.Material.VertexMaterials.GetEnumerator ();
				var vertices = new V[vertCount];
				vertices [0] = NewVertex (new Vec3 (0f), normal, new Vec2 (0.5f, 0.5f), materials.Next ());
				var angle = startAngle;
				for (var i = 1; i < vertCount; i++)
				{
					var unitPos = new Vec2 ((float)Math.Cos (angle), (float)Math.Sin (angle));
					var pos = new Vec3 (width * unitPos.X, height * unitPos.Y, 0f);
					vertices [i] = NewVertex (pos, normal, unitPos + new Vec2 (0.5f, 0.5f), materials.Next ());
					angle = Math.Min (angle + stepAngle, endAngle);
				}
				return vertices;
			}, material, fullArc);
        }

        public static Circular<V> Ellipse (float width, float height, float stepAngle, IMaterial material)
        {
			return Pie (width, height, stepAngle, 0f, 0f, material);
        }

        public static Circular<V> Ellipse (float width, float height, IMaterial material)
        {
			return Ellipse (width, height, MathHelper.Pi / 18, material);
        }

        public static Circular<V> Circle (float diameter, float stepAngle, IMaterial material)
        {
			return Ellipse (diameter, diameter, stepAngle, material);
        }

        public static Circular<V> Circle (float diameter, IMaterial material)
        {
			return Ellipse (diameter, diameter, material);
        }

        protected override IEnumerable<int> GenerateIndices ()
        {
            for (int i = 2; i < Vertices.Length; i++)
            {
                yield return 0;
                yield return i;
                yield return i - 1; 
			}
			if (_fullArc)
			{
				yield return 0;
				yield return 1; 
				yield return Vertices.Length - 1;
			}
        }
    }
}
