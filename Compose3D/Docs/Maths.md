#Math Support

It is virtually impossible to write an OpenGL application without some kind of a 
mathematical library that provides at least the basic concepts of matrix algebra. 
OpenGL is just a graphics API, so it does not provide a library for mathematical 
calculations. GLSL, on the other hand, has such a library built into it, but the 
same functionality is not available in the client code, because it can be written 
in any programming language.

So, the first need that arises is to have a mathematical library that provides at 
least the same functionality as GLSL. Preferably the semantics of the library should 
be as close to GLSL as possible. The mathematical entities like vectors and matrices 
should be defined in the same way in both. The only difference should be where the 
mathematics code is run; on CPU or on GPU.

This is the motivation for adding the mathematics support to Compose3D. There is a 
similar library in OpenTK, but it differs in many places from GLSL so Compose3D does 
not use it. The requirement of the math library is to have the same set of types and 
functions for C# and for GLSL, so that the code would work the same way regardless of 
where it is executed. For this reason we cannot use some generic mathematics library 
like the one provided with OpenTK.

Compose3D implements big part of the functionality available in GLSL. The purpose is 
not only to allow writing code that can be translated to GLSL. The implementation 
works also in C#. In some cases the existing functions of `System.Math` class can 
be used. Function provided by .NET framework are mapped to GLSL if they have same 
interface and semantics in both languages.

The math types and functions live in the `Compose3D.Maths` namespace.

##Vectors

Vectors lift the scalar values into _n_th dimension. They are used to represent 
position, direction, color, and many other things that are considered when generating
3D graphics. The main operations of vectors are the same as of scalar values: 
addition, subraction, multiplication, and division. These are calculated in a 
_componentwise_ manner. That is, each component of a vector is added, subracted, 
multiplied, or divided separately; either with a same value, or wih a corresponding
component of another vector.

Most of the scalar operations can be generalized analogously to vectors. Also, a big
part of the code in the `Compose3D.Maths` namespace is lifting the mathematic 
operations from scalar values to vectors. Almost all of the operations are also 
supported by GLSL, so they can be used interchangeably in the shader code.

The only decimal type currently supported by Compose3D is `float`, i.e. a 32-bit 
floating point value. This is the most commonly used decimal type in GLSL as well --
mostly because double precision types come with a performance penalty, and they are 
only supported by more recent graphics hardware. In most applications single precision 
is enough to produce good quality graphics. 

There is an easy way to include support for double precision types to Compose3D later on, 
though. All the vector and matrix types are parameterized by their component type. So, 
adding support to `double` is just a matter of adding the implementations to all of the 
functions, which is trivial to do. Currently this is left out, just to keep the library 
footprint a bit smaller. The support will come some time later depending on the demand.

###The Basic Vector Types

The basic vector types are the same as in GLSL: `Vec2` for 2-dimensional vectors, 
`Vec3` for 3-dimensional, and so on. Their naming convention differs from GLSL in 
two ways. Firstly, the type names are in Pascal case, i.e. they begin with a capital 
letter. The second difference is that the component type indicator is at the end of the 
type name, not at the beginning. I.e. instead of `ivec2` the type is named `Vec2i` in 
Compose3D. This is to prevent confusing them to interface types which also begin with 
capital `I`.

Also the operations defined for vectors are as similar to GLSL as possible. The main 
arithmetic operators are overloaded, so that the syntax is also similar. The overloaded
operators are.

- `-v` negation,
- `v1 + v2` addition,
- `v1 - v2` subtraction,
- `v1 * v2` multiplication, and
- `v1 / v2` division.

Also the scalar versions are available for multiplication and division (`v` is a
vector type, and `s` is a primitive type, like `float` or `int`.

- `v * s`, and
- `v / s`.

Other commonly used operations on vectors are dot product (`Dot ()`) and length of the
vector, which is defined as the property `Length`. 

Also equality and inequality operators are overloaded, of course:

- `v1 == v2`, equality (same as `v1.Equals (v2)`),
- `v1 != v2`, inequality.

####Accessing Components

There are three ways to access the vector components. The simplest way is to just to 
reference the component by name, for example `v.Y`. The more sophisticated way is to 
use swizzling, which is possible to implement in C#, but not with the same syntax as in 
GLSL. For example, to access the last two components of a 3-dimensional vector, you can 
use the indexer property as follows:

    v[Coord.y, Coord.z]

This translates to GLSL expression:

    v.yz

Swizling allows you to access the components in a random order even a same component
twice:

    v[Coord.y, Coord.x] + v[Coord.z, Coord.z]

These are translated to GLSL as follows:
    
    v.yx + v.zz

The third way is to refer to the vector component by its index; X-component has
index 0, Y-component 1, and so on. This makes it possible to iterate through the 
components generically without hard-coding the number of dimensions. Accessing by
index is done using the normal array syntax: `v[0] == v.X`.

###Generalizing Vectors

In GLSL there are distinct vector types for each component type and vector dimension.
Implementing these directly in C# would result in a lot of duplicate code that would
look almost identical across the types. Still, there is no easy way to generalize
the code using generics, because primitive types share no common interfaces of base 
classes, and number of components cannot reasonably be represented by a type.

For this reason Compose3D uses a couple of tricks to reduce the duplicate code. First,
the code for basic vector types is actually generated from T4 templates. This reduces
the number of source lines to be maintained. There are problems which entail this 
approach, such as a minor change propagating through the generated code. But all in 
all, it makes tasks like adding support for the `double` component type less laborious 
in the future.

Another way to reduce the duplicate code is to create an interface which all the basic
vector types implement, and define operations in terms of the interface rather than the 
concrete types. Vector types all implement the `IVec<V, T>` interface which is defined 
as follows:

	public interface IVec<V, T> : IEquatable<V>
		where V : struct, IVec<V, T>
		where T : struct, IEquatable<T>
	{
		/// <summary>
		/// Negate all of the components of the vector.
		/// </summary>
		V Invert ();

		/// <summary>
		/// Add another vector this one componentwise.
		/// </summary>
		V Add (V other);

		/// <summary>
		/// Subtract the given vector from this one componentwise.
		/// </summary>
		V Subtract (V other);

		/// <summary>
		/// Multiply the components of this vector with a same scalar value.
		/// </summary>
		V Multiply (T scalar);

		/// <summary>
		/// Multiply with another vector componentwise.
		/// </summary>
		V Multiply (V scale);

		/// <summary>
		/// Divide the components of this vector by a same scalar value.
		/// </summary>
		V Divide (T scalar);

		/// <summary>
		/// Divide by another vector componentwise.
		/// </summary>
		V Divide (V scale);

		/// <summary>
		/// Calculate the dot product with another vector.
		/// </summary>
		T Dot (V other);

		/// <summary>
		/// Number of dimensions/components in the vector.
		/// </summary>
		int Dimensions { get; }

		/// <summary>
		/// The value of the index'th component of the vector.
		/// </summary>
		T this[int index] { get; set; }

		/// <summary>
		/// The lengh of the vector.
		/// </summary>
		T Length { get; }

		/// <summary>
		/// The lengh of the vector squared. This is bit faster to calculate than the actual length
		/// because the square root operation is omitted.
		/// </summary>
		T LengthSquared { get; }

		/// <summary>
		/// The normalized vector. I.e. vector with same direction, but with lenght of 1.
		/// </summary>
		V Normalized { get; }
	}

Using this interface we can define the rest of the operations that operate on vectors
without resorting to code generation. This is especially pleasing when we can share
implementation with multiple component types and vector dimensions.

If you are wondering why the interface contains the implementing type as its first type
argument, there is a reson for this. This allows us to define the methods of the interface 
with concrete types instead of the interface type. So, instead of defining the addition as:

    IVec<T> Add (IVec<T> other)

we are defining it as:

    V Add (V other)

where V is the implementing struct. This way we don't loose the concrete type information
whenever calling the interface methods. It saves us from many type casts that would be
needed, if we would use the former way.

Another benefit of this approach is that all the extension methods that we define for
the interface can be defined in the same way, preserving the type information without 
upcasting the arguments to the `IVec` interface. The requirement that a type must 
implement an interface can be defined by the type constraint in the where clause. Below
is an example:

	public static V Reflect<V, T> (this V vec, V along)
		where V : struct, IVec<V, float>
		where T : struct, IEquatable<T>
	{
		return vec.Subtract (along.Multiply (2 * vec.Dot (along)));
	}

###Functions for Vectors

Most math functions in GLSL work both with scalar values and with vectors. The same
applies to Compose3D as well. The scalar versions of the functions can be found in
the `Compose3D.Maths.GLMath` class which is kind of an extension of the 
`System.Math` class. The main difference between these is that the default argument 
type of functions in `GLMath` is `float` whereas in `System.Math` it is `double`. 
Also `System.Math` lacks the very useful functions defined in GLSL like `Clamp ()`, 
`Mix ()`, `Step ()`, and `SmoothStep ()`. 

Almost all of the functions that work with scalars are "lifted" to work with vectors
as well. These functions take advantage of the `IVec<V, T>` interface, so typically
there is only one implementation which works accross all vector sizes, and sometimes
accross different component types too. The implementation works componentwise so that
each component of a vector is processed in the same way. The other arguments, if there
are any, can be vectors of the same dimension and component type, or scalars of the same
numeric type.

There are also some functions that work only for specific vector types. An example of
such is the cross product operation `Cross ()`, which is defined only for the `Vec3` 
type. 

##Matrices

One distinction between Compose3D and OpenTK is that in the former the matrices are 
defined in the column-major manner. OpenTK chooses the row-major way for some reason. 
This choice is significant for many reasons, one of which is that it determines also 
the order in which the matrices should be multiplied. Given two matrices _M_ and _N_, 
if you want to first apply _M_ and then _N_, you should multiply them together in this 
order: _N_ * _M_. The same applies if you want to transform a vector _V_ by matrix _M_. 
The correct order of multiplicands is: _M_ * _V_.

