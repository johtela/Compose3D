namespace Compose3D.Geometry
{
    using Arithmetics;
    using OpenTK;
    using System;
    using System.Collections.Generic;

    public abstract class Primitive<V> : Geometry<V> where V : struct, IVertex
    {
        private Func<Geometry<V>, V[]> _generateVertices;

        protected Primitive (Func<Geometry<V>, V[]> generateVertices)
        {
            _generateVertices = generateVertices;
        }

        protected override IEnumerable<V> GenerateVertices ()
        {
            return _generateVertices (this);
        }
    }
}
