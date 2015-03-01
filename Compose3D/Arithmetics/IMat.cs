namespace Compose3D.Arithmetics
{
    using System;

    interface IMat<M, T>
        where M : struct, IMat<M, T>, IEquatable<M>
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
