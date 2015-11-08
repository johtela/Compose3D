namespace Compose3D.Maths
{
    using System;

    public interface IMat<M, T> : IEquatable<M>
        where M : struct, IMat<M, T>
        where T : struct, IEquatable<T>
    {
        M Add (M mat);
        M Subtract (M mat);
        M Multiply (T scalar);
        V Multiply<V> (V vec) where V : struct, IVec<V, T>, IEquatable<V>;
        M Divide (T scalar);

        int Columns { get; }
        int Rows { get; }
        T this[int column, int row] { get; set; }
    }
}
