module GLUtils

open System
open System.IO
open System.Reflection
open System.Runtime.InteropServices
open OpenTK
open OpenTK.Graphics.OpenGL

/// Exception type for OpenGL errors
type OpenGLError (msg) = 
    inherit Exception (msg)

/// Wrap OpenGL program (int) to an union type.
type Program = Program of int 

/// Wrap OpenGL shader (int) to an union type.
type Shader = Shader of int

/// Wrap OpenGL vertex buffer object (int) to an union type. 
/// Second value contains the number of items in the buffer.
type VBO = VBO of int * int

/// Wrap OpenGL uniform (int) to an union type.
type Uniform = Uniform of int

/// Get the last error occurred, or null if there was no error.
let getError =
    let error = GL.GetError ()
    match error with
    | ErrorCode.NoError -> None
    | ErrorCode.InvalidEnum ->
        Some (OpenGLError "GL_INVALID_ENUM: An unacceptable value has been specified for an enumerated argument")
    | ErrorCode.InvalidValue ->
        Some (OpenGLError "GL_INVALID_VALUE: A numeric argument is out of range")
    | ErrorCode.InvalidOperation ->
        Some (OpenGLError "GL_INVALID_OPERATION: The specified operation is not allowed in the current state")
    | ErrorCode.StackOverflow ->
        Some (OpenGLError "GL_STACK_OVERFLOW: This command would cause a stack overflow")
    | ErrorCode.StackUnderflow ->
        Some (OpenGLError "GL_STACK_UNDERFLOW: This command would cause a stack underflow")
    | ErrorCode.OutOfMemory ->
        Some (OpenGLError "GL_OUT_OF_MEMORY: There is not enough memory left to execute the command")
    | ErrorCode.InvalidFramebufferOperation
    | ErrorCode.InvalidFramebufferOperationExt
    | ErrorCode.InvalidFramebufferOperationOes ->
        Some (OpenGLError ("GL_INVALID_FRAMEBUFFER_OPERATION(_EXT|_OES): " +
                "The object bound to FRAMEBUFFER_BINDING is not \"framebuffer complete\""))
    | _ -> Some (OpenGLError ("Unexptected error: " + error.ToString ()))  

/// Call the function and throw an exception, if error occurred. Otherwise return
/// the result normally.
let checkError fn =
    let res = fn
    match getError with
    | None -> res
    | Some (error) -> raise error

/// Type representing a single vertex attribute defined in a struct. The name
/// and type of the struct field must match the name and type of the attribute
/// used in a shader.
type VertexAttr = {
    name  : string                  // name of the attribute
    ptype : VertexAttribPointerType // type of the data items in the struct
    size  : int                     // size of the struct
    count : int                     // number of items in the struct
}

/// Get the vertex attributes of a type as sequence of VertexAttr records. 
let getVertexAttrs<'a when 'a : struct> () : VertexAttr seq =
    let t = typeof<'a>

    // map a FieldInfo to VertexAttr
    let mapToVertexAttr (f : FieldInfo) : VertexAttr =
        let ft = f.FieldType
        let s = Marshal.SizeOf (f.FieldType)
        let name = f.Name.TrimEnd('@')
        
        if ft = typeof<Vector3> then 
            { name = name; ptype = VertexAttribPointerType.Float; size = s; count = 3 }
        else if ft = typeof<Vector4> then
            { name = name; ptype = VertexAttribPointerType.Float; size = s; count = 4 }
        else 
            raise <| new ArgumentException ("Incompatible vertex attribute type " + name)

    t.GetFields (BindingFlags.Instance ||| BindingFlags.NonPublic ||| BindingFlags.Public) |>
        Seq.map mapToVertexAttr

let getVertexAttrIndex (Program glprog) (name : string) = 
    let index = GL.GetAttribLocation (glprog, name)
    if index < 0 then 
        raise <| OpenGLError (sprintf "Attribute '%s' was not found in program" name)
    index

let getUniform (Program glprog) (name : string) : Uniform =
    let glunif = GL.GetUniformLocation (glprog, name)
    if glunif < 0 then 
        raise <| OpenGLError (sprintf "Uniform '%s' was not found in program" name)
    Uniform glunif

let (&=) (Uniform glunif) (value : obj) =
    match value with
    | :? int as i -> GL.Uniform1 (glunif, i)
    | :? uint32 as i -> GL.Uniform1 (glunif, i)
    | :? float32 as f -> GL.Uniform1 (glunif, f)
    | :? float as f -> GL.Uniform1 (glunif, f)
    | :? (int []) as a -> GL.Uniform1 (glunif, a.Length, a)
    | :? (uint32 []) as a -> GL.Uniform1 (glunif, a.Length, a)
    | :? (float32 []) as a -> GL.Uniform1 (glunif, a.Length, a)
    | :? (float []) as a -> GL.Uniform1 (glunif, a.Length, a)
    | _ -> raise <| OpenGLError "Unsupported uniform type"

/// Initalize a vertex buffer with sequence of vertices. Get the number of buffer as parameter.
/// Returns the handle to the vbo and the length of the sequence as a tuple.
let initVertexBuffer (vertices : seq<'a>) : VBO =
    let glvbo = GL.GenBuffer ()
    let varr = Seq.toArray vertices
    let size = sizeof<'a> * varr.Length |> nativeint

    checkError <| GL.BindBuffer (BufferTarget.ArrayBuffer, glvbo)
    checkError <| GL.BufferData (BufferTarget.ArrayBuffer, size, varr, BufferUsageHint.StaticDraw)
    VBO (glvbo, varr.Length)

/// Draw the vertex buffer referred by the vbo. 
/// The program, vbo and number of is given as parameter.
let drawVertexBuffer<'a when 'a : struct> prog (VBO (glvbo, cnt)) = 
    let recSize = sizeof<'a>

    // Setup an attribute
    let setupAttr (offset : int) (attr : VertexAttr) =
        let index = getVertexAttrIndex prog attr.name
        checkError <| GL.EnableVertexAttribArray (index)
        checkError <| GL.VertexAttribPointer (index, attr.count, attr.ptype, false, recSize, offset)
        offset + attr.size        
    
    checkError <| GL.BindBuffer (BufferTarget.ArrayBuffer, glvbo)
    Seq.fold setupAttr 0 (getVertexAttrs<'a> ()) |> ignore
    checkError <| GL.DrawArrays (PrimitiveType.TriangleStrip, 0, cnt)

/// Create a shader of specific type given the file path to the source.
let createShader (Program glprog) (stype, path) =
    let shader = GL.CreateShader stype
    let src = File.ReadAllText path

    checkError <| GL.ShaderSource (shader, src)
    checkError <| GL.CompileShader shader
    let log = checkError <| GL.GetShaderInfoLog (shader)
    if log.Contains("ERROR") then
        raise <| OpenGLError (sprintf "Shader compilation error in '%s':\n%s" path log)
    checkError <| GL.AttachShader (glprog, shader)
    printf "%s" log

/// Load a program consisting of a give set of shaders
let loadProgram shaders : Program = 
    let glprog = GL.CreateProgram ()
    let prog = Program glprog
    let createShader = createShader prog

    shaders |> List.iter createShader
    checkError <| GL.LinkProgram glprog
    let log = checkError <| GL.GetProgramInfoLog glprog
    if log.Contains("ERROR") then
        raise <| OpenGLError (sprintf "Program linking error:\n%s" log)
    checkError <| GL.UseProgram glprog
    printf "%s" <| log
    prog
