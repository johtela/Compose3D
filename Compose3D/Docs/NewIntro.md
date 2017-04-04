# ComposeFx - Toolbox for GPU Programming in C#

ComposeFx is a C# library that helps you harness the power of GPU computing. 
It builds on top of OpenGL and OpenCL and provides an abstraction layer that makes their
usage simpler and easier. The name "Compose Framework" stems from the guiding principle of
the library design. By using functional programming style and composable abstractions 
ComposeFx is able to generalize the concepts required to build graphical OpenGL applications
and general purpose GPU programming in OpenCL.

Core part of the library is the Linq parser that enables writing both GLSL shaders and OpenCL
kernels inside C# code. Linq expressions are translated to GLSL and OpenCL code on the fly
eliminating the need to include exernal shader or kernel source files. Practically all of the
features of GLSL and OpenCL C are supported, including the built-in math libraries and support
for vectors, matrices, textures, etc.

The goal of ComposeFx is to provide all the facilities to build programs utilizing OpenGL and
OpenCL with minimal external dependencies. It does not try to hide the low-level concepts but
rather wrap them in nicer packages making the resulting code more succinct and clear.

This does not mean that ComposeFx is just a wrapper API - quite the contrary. It delivers a
ton of functionality missing from the core OpenGL and OpenCL. Below is a sample list of the 
features it offers:

**Math Support**

* Vector and matrix types supported by GLSL and OpenCL C
* Mathematical functions defined for floats and generalized for vectors
* Basic operations of linear algebra: vector addition, dot product, cross product,
  matrix multiplication, determinant, and inverse
* Quaternions, planes, and B-splines

**Building 3D Geometry**

* Basic primitives: triangles, quadrilaterals, polygons, ellipses, and polygons with 
  arbitrary number of lines and curves
* Extrusion and lather operations to construct 3D volumes from 2D primitives
* Constructing complex geometry from simple primitives by composition operation
* Transformations and simplifications
* Smoothing of surface normals
* Tesselation support

**Spatial Data Structures**

* Axis-aligned bounding boxes (_AABB_)
* Interval trees, Kd-trees, Octrees for fast occlusion testing, and for finding 
  points and vertices quickly based on their coordinates. Interval tree is balanced
  for best possible performance

**Scene Graph**

* Scene graph maintaing a 2D/3D scene of object of various types; paths, meshes, 
  panels, and GUI windows
* Interval tree based occlusion testing
* Viewing frustum based occlusion testing

**Textures**

* Functionality to load textures from bitmaps and create them from arbitrary data
* Support for different sampler types
* Mapping texture coordinates to surfaces

**OpenGL Shaders**

* Basic diffuse and phong shaders
* Different light models
* Shadow map shaders including percentage closer filterin, summed area variance, and
  cascaded shadow maps
* Geometry shader support

**User Interface**

* Building simple user interfaces for OpenGL views using controls like: buttons, edit
  boxes, sliders, list views, and color pickers
* Automatic sizing and layout of controls
* Support for moving and resizing UI panels

**Imaging and Texture Generation**

* Building procedural textures from noise functions and compositing and transformation
  functions
* Editors for changing the parameters of procedural textures
* Image filtering in OpenGL
* OpenCL accelerated texture generation

**Simple Reactive Programming Framework**

* Small set of types and operations allowing push-based reactive programming
* Used mainly to manage the state of OpenGL renderers, and composing call-backs
  for various events



