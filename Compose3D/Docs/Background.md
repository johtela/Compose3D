#Motivation for Compose3D

Getting started with modern OpenGL programming can be a daunting task. To get a 
simple triangle on the screen you need to write couple of screenfuls of code. You 
need to explicitly tell OpenGL how it should set itself up and feed it with all kinds 
of strange objects such as vertex buffer objects, vertex array objects, shaders, programs,
uniforms, etc. Furthermore, you need to switch to a completely different programming
language in order to define how the vertices are transformed, and how the fragments
are converted to pixels in the screen.

Even if you know the basics of 3D programming and understand matrix algebra, affine 
transformations, lighting calculations, and so on, the learning curve of OpenGL is
still pretty steep. What's worse, when you get finally something working, the resulted
code looks ugly and imperative. Usually it consist of a couple of long functions which 
call OpenGL API interleaved with the code that actually creates the 3D content. The 
programming model behind OpenGL is a massive state machine which is changed by hundreds 
of special purpose functions. Typically these functions change a small part the state 
machine in a specific way.

There are of course alternatives to OpenGL, if you just like to produce 3D graphics
without diving deep into how GPUs and graphics processing work in a modern hardware.
These might be a better alternative, especially, if you don't want to spend the time 
learning the theory behind realistic 3D graphics. The most popular alternative for .NET 
is WPF, which is mainly targeted for desktop applications that don't need to produce 
realistic or high-performance graphics.

If you are out to write a game, there are also many game engines that can produce
really nice looking graphics. Unity is a popular example of this kind of product. 
Game engines' primary drawback is that they are not programming libraries but more
like toolkits for building 3D content, and games of a certain genre. Their flexibility 
is limited, and most of them are not designed to be used by programmers, but rather 
game and content designers. Also, their licensing model might prevent using them in 
certain scenarios, or at least make it prohibitively expensive. 

Despite its shortcomings, OpenGL has a lot of benefits as well. It is cross-platform;
supported in desktop computers, mobile devices, and on the web. It is OS neutral; you
can write applications that work on Windows, MacOS, and Linux. It is also supported
by almost all of the hardware vendors, so you can be sure that all the 3D capable
devices will support OpenGL in some form. 

OpenGL is truly prevalent, even though it is considered to be outdated, and there 
are already challengers in the market to replace it. Unfortunately the alternatives are 
either vendor/platform specific like Microsoft's DirectX or Apple's Metal, or just 
not mature enough yet. The most promising initiative to replace OpenGL is Vulkan, 
which is backed by many of the leading hardware and software vendors. However, its 
development is still in very early stage, and it will take some time until it is as 
widely adopted as OpenGL, assuming that this will happen eventually.

So, we are stuck with OpenGL for now, at least if you want to write cross-platrorm, 
fast-performance 3D applications using all the features provided by modern graphics 
hardware. If you want to write them in C#/.NET, the options are even more limited. 
There are a couple of bindings to OpenGL that expose the API to .NET, OpenTK being 
the most popular of them. These bindings provide a thin wrapper over the API, so code 
ends up looking more or less the same as if you would use OpenGL API directly from C++.

Compose3D was created to remedy the problems listed above and to make OpenGL programming 
more enjoyable in C# and .NET. Its main design goals are:

-   to provide a functional interface over API hiding the OpenGL state machine,
-   make writing shaders easier by converting LINQ expressions to GLSL. LINQ is
    effectively the internal _DSL_ (domain specific language) that defines the
    shaders computations,
-   unify the data types and functions between C# code and GLSL code, and
-   implement the most important operations and functions provided by GLSL.

In addition to simplifying OpenGL programming, Compose3D also provides a lot of 
functionality that is not directly dependent on OpenGL. This includes things like:

-   creating complex geometries and meshes from simple primitives, composing and 
    enriching them with various tools usually found in 3D modelling software,
-   spatial data structures such as octrees and interval trees that enable scene
    graph culling and fast manipulation of geometrical objects,
-   reactive framework which simplifies event handling, and OpenGL state management, and
-   predifined functions for calculating lighting, shadows, and other things typically 
    done in shaders.

In essence, Compose3D provides higher-level abstractions and concepts than raw OpenGL 
API while retaining the control to the lower-level functionality. The concepts of OpenGL 
are all there, but in a more succinct form. 

In addition, there are a lot of higher-level concepts that are not present in OpenGL,
like geometry definitions, scene graphs, renderers, and so on. Typically an OpenGL 
application needs to reinvent these concepts and features time and again. Compose3D 
provides most of them in a generic form, so that they can be reused and modified based 
on the application's needs.