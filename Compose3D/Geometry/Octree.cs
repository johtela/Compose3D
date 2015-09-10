namespace Compose3D.Geometry
{
    using System;
    using Arithmetics;
    using System.Collections.Generic;
    using System.Linq;

    public class Octree<T, U, V> 
        where V : struct, IVec<V, T>
        where T : struct, IEquatable<T>
    {
        private class Node
        {
            public Node[] Children;
            public U Data;

            public Node ()
            {
                Children = new Node[8];
            }
        }


    }
}
