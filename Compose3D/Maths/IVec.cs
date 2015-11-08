namespace Compose3D.Maths
{
    using System;

    public interface IVec<V, T> : IEquatable<V>
        where V : struct, IVec<V, T>
        where T : struct, IEquatable<T>
    {
        V Invert ();
        V Add (V other);
        V Subtract (V other);
        V Multiply (T scalar);
        V Multiply (V scale);
        V Divide (T scalar);
        T Dot (V other);

        int Dimensions { get; }
        T this[int index] { get; set; }
        T Length { get; }
        T LengthSquared { get; }
        V Normalized { get; }
    }
}