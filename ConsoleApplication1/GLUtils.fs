module GLUtils

#if INTERACTIVE
#r "../packages/OpenTK.1.1.1589.5942/lib/NET40/OpenTK.dll"
#endif

open System
open System.IO
open System.Reflection
open System.Runtime.InteropServices
open OpenTK
open OpenTK.Graphics.OpenGL

/// Get the vertex attributes of a type as sequence of tuples: 
/// (size of field, number of components, type of component)
let getVertexAttrs<'a when 'a : struct> () =
    let t = typeof<'a>
    let getFieldSizes (f : FieldInfo) =
        let ft = f.FieldType
        let s = Marshal.SizeOf (f.FieldType)
        if ft = typeof<Vector3> then (s, 3, VertexAttribPointerType.Float)
        else if ft = typeof<Vector4> then (s, 4, VertexAttribPointerType.Float)
        else raise  <| new ArgumentException ("Incompatible vertex attribute type " + ft.Name)
    let fields = t.GetFields (BindingFlags.Instance ||| BindingFlags.NonPublic ||| BindingFlags.Public)
    fields |> Seq.map getFieldSizes

/// Initalize a vertex buffer with sequence of vertices. Get the number of buffer as parameter.
/// Returns the handle to the vbo and the length of the sequence as a tuple.
let initVertexBuffer (vertices : seq<'a>) =
    let vbo = GL.GenBuffer ()
    let varr = Seq.toArray vertices
    let size = sizeof<'a> * varr.Length |> nativeint
    GL.BindBuffer (BufferTarget.ArrayBuffer, vbo)
    GL.BufferData (BufferTarget.ArrayBuffer, size, varr, BufferUsageHint.StaticDraw)
    (vbo, varr.Length)

/// Draw the vertex buffer referred by the vbo. The number of vertices is also given as
/// parameter.
let drawVertexBuffer<'a when 'a : struct> (vbo : int, vertexCount : int) = 
    let recSize = sizeof<'a>
    let setupAttr (index : int, offset : int) (size : int, count : int, ftype : VertexAttribPointerType) =
        GL.EnableVertexAttribArray (index)
        GL.VertexAttribPointer (index, count, ftype, false, recSize, offset)
        (index + 1, offset + size)        
    GL.BindBuffer (BufferTarget.ArrayBuffer, vbo)
    let (attrCount, _) = Seq.fold setupAttr (0, 0) <| getVertexAttrs<'a> ()
    GL.DrawArrays (PrimitiveType.Triangles, 0, vertexCount)
    for i = 0 to attrCount - 1 do
        GL.DisableVertexAttribArray (i)

let createShader program (stype, path) =
    let shader = GL.CreateShader stype
    GL.ShaderSource (shader, File.ReadAllText path)
    GL.CompileShader shader
    GL.AttachShader (program, shader) 

let loadProgram shaders = 
    let program = GL.CreateProgram ()
    let createShader = createShader program
    shaders |> List.iter createShader
    GL.LinkProgram program
    GL.UseProgram program
