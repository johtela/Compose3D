namespace Compose3D.Arithmetics
{
    using System;

    public interface IVec<V, T>
        where V : struct, IVec<V, T>, IEquatable<V>
        where T : struct
    {
        V Negate ();
        V Add (V other);
        V Subtract (V other);
        V Multiply (T scalar);
        V Multiply (V scale);
        V Divide (T scalar);
        T Dot (V other);

        T this[int index] { get; set; }
        T Length { get; }
        T LengthSquared { get; }
        V Normalized { get; }
    }
}