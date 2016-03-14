Compose3D - Create 3D OpenGL Applications Completely in C#
==========================================================

Compose3D is a library that provides a rich toolset for writing OpenGL applications in C#. Instead of using the low-level 
OpenGL API and writing shader code in an external language, you can do everything in C# code. This includes writing GLSL 
shaders and generating complex geometrical shapes. There are a lot of functionalities provided by the library. Below is a 
list of features and some reasons why you might want to use it.

-	Compose3D minimizes the boilerplate code required to set up OpenGL for rendering. It wraps the concepts of OpenGL
	such as shaders, programs, and uniforms inside classes hiding the complexities of the OpenGL API.
-	You can write the GLSL shaders in C# using LINQ syntax. Shaders are modeled as processors that transform input data 
	structures such as vertices and normals into output data structures like color and depth buffer.
-	The GLSL datatypes such as vectors, matrices and samplers are included in the library as C# structs and classes. GLSL 
	functions are also provided and they have the same implementation as in GLSL. So, all the calculations and matrix math 
	work the same way in C# and GLSL allowing you to move code between C# and GLSL in a seamless way.
-	Compose3D makes creating 3D geometries a breeze by offering a composable way to construct them. You can build complex
	models from simple primitives. Instead of loading static 3D model files, you can procedurally create your models and 
	tweak them in limitless ways. Everything is dynamic and customizable.
-	You have a complete control over the OpenGL pipeline and you can use all the features provided by it. In fact, 
	Compose3D uses the OpenTK library as the interface to OpenGL libraries and all of its features are available to you.
-	Last but not least, the library is multiplatform compatible running both in .NET and Mono. It is tested with .NET in 
	Windows as well as Mono and Linux. MacOS platform is not yet tested, but there should be no reason why the code would 
	not work on it.

The library is called Compose3D, because generality and composability are its main design drivers. This also means that C# 
generics, interfaces, lambdas, and especially LINQ are employed heavily. Nevertheless, code readability and providing a 
clean API are also important concerns. Utilizing the tooling provided by Visual Studio and MonoDevelop makes discovering and 
using the API easier. The C# compiler and type checker helps in finding the trivial errors that are common when using the 
OpenGL API directly and writing shaders in GLSL.

In a nutshell, Compose3D attempts to make writing 3D applications as intuitive as possible, enabling you to create 
impressive 3D graphics with minimal amount of work. Anybody who has written OpenGL applications can testify that a lot of 
time usually goes to figuring out what is wrong with a shader you wrote, or why your formula is not calculating the correct 
result. Compose3D reduces this kind of grunt work and helps you get the results faster.

The last thing to mention is the performance which is always important in real-time 3D. There are no trade-offs between 
performance and ease of use in Compose3D. All the calculations required are done in advance, usually at application start-up 
time. Once the objects and scenes are computed, GPU can run in full speed without being constrained by CPU. All the CPU 
cycles are available to your application.
