#Compose3D - A Library for Writing 3D OpenGL Applications in C#

Compose3D is a C# library that helps writing OpenGL applications in C#. Modern OpenGL applications consist 
of code that runs on CPU and is written in some general purpose language like C#. The shaders that run on GPU are
written in GLSL, an external domain specific language. Compose3D makes it possible to write both OpenGL
client code and shaders in C#. It also includes a lot of other functionality that simplifies the OpenGL API
and provides higher-level concepts to work with. Below are some example features and reasons why you might want 
to use it.

-	Compose3D minimizes the boilerplate code required to set up OpenGL for rendering. It wraps the concepts 
	of OpenGL such as shaders, programs, and uniforms into generic types hiding the complexities of the OpenGL API.
-	You can write the GLSL shaders in C# using LINQ syntax. Shaders are modeled as processors that transform 
	input data 	structures such as vertices and normals into output data structures like color and depth buffer.
-	The GLSL datatypes such as vectors, matrices and samplers are included in the library as C# structs and classes. 
	GLSL functions are also provided and they have the same implementation as in GLSL. So, all the calculations and 
	matrix math work the same way in C# and GLSL allowing you to move code between C# and GLSL in a seamless way.
-	Compose3D makes creating 3D geometries a breeze by offering a composable way to construct them. You can build 
	complex models from simple primitives. Contrast to loading static 3D model files, you can procedurally create 
	your models and tweak them in limitless ways. Everything is dynamic and customizable.
-	You have a complete control over the OpenGL pipeline and you can use all the features provided by it. Compose3D 
	uses the OpenTK library as the interface to OpenGL libraries and all of its features are available to you.
-	Last but not least, the library is multiplatform compatible running both in .NET and Mono. It is tested with 
	.NET in Windows as well as Mono and Linux.

The library is called Compose3D, because generality and composability are its main design drivers. Generics, 
interfaces, lambdas, and especially LINQ are employed heavily to make the verbose OpenGL API more compact and 
manageable. Code readability and clean API are the main priorities of library design. Utilizing the tooling provided 
by Visual Studio and MonoDevelop makes discovering and using the API easier. The C# compiler and type checker helps 
in finding the trivial errors that are common when using the OpenGL API directly and writing shaders in GLSL.

In a nutshell, Compose3D attempts to make writing 3D applications as intuitive as possible, enabling you to create 
impressive 3D graphics with minimal amount of work. Anyone who has written OpenGL applications knows that a lot of 
time usually goes to figuring out what is wrong with your code, or why you don't see the correct result. 
Compose3D reduces this kind of grunt work and helps you get the results faster.

The last thing to mention is performance which is always a concern in real-time 3D. Compose3D does no trade-offs 
between performance and ease of use. Time-consuming computations are done in advance, usually at application start-up 
time. Once the objects and scenes are computed, GPU can run in full speed without being constrained by CPU. All the 
CPU cycles are available to your application.