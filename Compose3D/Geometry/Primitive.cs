namespace Compose3D.Geometry
{
    using Arithmetics;
    using OpenTK;
    using System;
    using System.Collections.Generic;

    public abstract class Primitive<V> : Geometry<V> where V : struct, IVertex
    {
        private Func<Geometry<V>, V[]> _generateVertices;
        private IMaterial _material;
        protected Primitive (Func<Geometry<V>, V[]> generateVertices, IMaterial material)
        {
            _generateVertices = generateVertices;
            _material = material;
        }

        protected override IEnumerable<V> GenerateVertices ()
        {
            return _generateVertices (this);
        }

        public override IMaterial Material
        {
            get { return _material; }
        }
    }
}
