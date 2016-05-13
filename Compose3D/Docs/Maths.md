#Mathematics Support

The need for mathematical library arises from the objective that code written using Compose3D should 
be equivalent to GLSL code as much as possible. The mathematical entities like vectors and matrices
should be defined in the same way in Compose3D as in GLSL. The functions operating on these entities
should have the same semantics regardless of whether they are executed in CPU or GPU. For this reason
we cannot use some generic mathematics library like the one provided with OpenTK.

Compose3D implements big part of the functions available in GLSL. The purpose of these functions is not
only to allow writing code that can be translated to GLSL. The implementation is also working in C#.
In some cases the existing functions of `System.Math` class can be used. Function provided by .NET 
framework are mapped to GLSL, if they have same interface and semantics in both languages.

One distinction between Compose3D and OpenTK is that in the former the matrices are defined in the 
column-major manner. OpenTK chooses the row-major way for some reason. This choice is significant
for many reasons, one of which is that it determines also the order in which the matrices should
be multiplied. Given two matrices _M_ and _N_, if you want to first apply _M_ and then _N_, you
should multiply them together in this order: _N_ * _M_. The same applies if you want to transform
a vector _V_ by matrix _M_. The correct order of multiplicands is: _M_ * _V_.

