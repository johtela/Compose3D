namespace Compose3D.Geometry
{
    using Arithmetics;
    using OpenTK;
    using System;
    using System.Collections.Generic;

    public class Circular<V> : Primitive<V> where V : struct, IVertex
    {
        private Circular (Func<Geometry<V>, V[]> generateVertices, IMaterial material)
            : base (generateVertices, material)
        {}

        public static Circular<V> Pie (float width, float height, float stepAngle, 
            float startAngle, float endAngle, IMaterial material)
        {
            if (startAngle > endAngle)
                throw new ArgumentException ("Start angle must be bigger than end angle");
            var sweep = endAngle == startAngle ? MathHelper.TwoPi : endAngle - startAngle;
            var vertCount = (int)Math.Ceiling (sweep / stepAngle) + 2;
            var normal = new Vec3 (0f, 0f, -1f);
            return new Circular<V> (e =>
              {
                  var colors = e.Material.Colors.GetEnumerator ();
                  var vertices = new V[vertCount];
                  vertices[0] = NewVertex (new Vec3 (0f), colors.Next (), normal);
                  var angle = startAngle;
                  for (var i = 1; i < vertCount; i++)
                  {
                      var pos = new Vec3 (width * (float)Math.Cos (angle), height * (float)Math.Sin (angle), 0f);
                      vertices[i] = NewVertex (pos, colors.Next (), normal);
                      angle = Math.Min (angle + stepAngle, endAngle);
                  }
                  return vertices;
              }, material);
        }

        public static Circular<V> Ellipse (float width, float height, float stepAngle, IMaterial material)
        {
            return Pie (width, height, stepAngle, 0f, 0f, material);
        }

        public static Circular<V> Ellipse (float width, float height, IMaterial material)
        {
            return Pie (width, height, MathHelper.Pi / 18, 0f, 0f, material);
        }

        public static Circular<V> Circle (float diameter, float stepAngle, IMaterial material)
        {
            return Pie (diameter, diameter, stepAngle, 0f, 0f, material);
        }

        public static Circular<V> Circle (float diameter, IMaterial material)
        {
            return Pie (diameter, diameter, MathHelper.Pi / 18, 0f, 0f, material);
        }

        protected override IEnumerable<int> GenerateIndices ()
        {
            for (int i = 2; i < Vertices.Length; i++)
            {
                yield return 0;
                yield return i;
                yield return i - 1; 
            }
        }
    }
}
