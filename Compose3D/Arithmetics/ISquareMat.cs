namespace Compose3D.Arithmetics
{
    using System;

    interface ISquareMat<M, T> : IMat<M, T>
        where M : struct, ISquareMat<M, T>, IEquatable<M>
        where T : struct, IEquatable<T>
    {
        M Multiply (M mat);
        M Transposed { get; }
        T Determinant { get; }
        M Inverse { get; }
    }
}
