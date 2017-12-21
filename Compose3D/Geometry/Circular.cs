namespace Compose3D.Geometry
{
    using Compose3D.Maths;
    using OpenTK;
    using System;
    using System.Collections.Generic;

    public class Circular<V> : Primitive<V> where V : struct, IVertex3D
    {
		bool _fullCircle;

		private Circular (V[] vertices, bool fullCircle)
			: base (vertices)
        {
			_fullCircle = fullCircle;
		}

        public static Circular<V> Pie (float width, float height, float stepAngle, 
            float startAngle, float endAngle)
        {
            if (startAngle > endAngle)
                throw new ArgumentException ("Start angle must be bigger than end angle");
			var fullCircle = startAngle == endAngle;
			if (fullCircle)
				endAngle += MathHelper.TwoPi;
			var vertCount = (int)Math.Ceiling ((endAngle - startAngle) / stepAngle) + (fullCircle ? 1 : 2);
            var normal = new Vec3 (0f, 0f, 1f);
			var vertices = new V[vertCount];
			vertices [0] = VertexHelpers.New<V> (new Vec3 (0f), normal);
			var angle = startAngle;
			for (var i = 1; i < vertCount; i++)
			{
				var pos = new Vec3 (width * (float)Math.Cos (angle), height * (float)Math.Sin (angle), 0f);
				vertices [i] = VertexHelpers.New<V> (pos, normal);
				angle = Math.Min (angle + stepAngle, endAngle);
			}
			return new Circular<V> (vertices, fullCircle);
        }

        public static Circular<V> Ellipse (float width, float height, float stepAngle)
        {
			return Pie (width, height, stepAngle, 0f, 0f);
        }

        public static Circular<V> Ellipse (float width, float height)
        {
			return Ellipse (width, height, MathHelper.Pi / 18);
        }

        public static Circular<V> Circle (float diameter, float stepAngle)
        {
			return Ellipse (diameter, diameter, stepAngle);
        }

        public static Circular<V> Circle (float diameter)
        {
			return Ellipse (diameter, diameter);
        }

        protected override IEnumerable<int> GenerateIndices ()
        {
            for (int i = 2; i < Vertices.Length; i++)
            {
                yield return 0;
                yield return i;
                yield return i - 1; 
			}
			if (_fullCircle)
			{
				yield return 0;
				yield return 1; 
				yield return Vertices.Length - 1;
			}
        }
    }
}
