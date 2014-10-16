module Main

// Disable warning about non-verifiable code
#nowarn "9"

open System
open System.Drawing
open System.Windows.Forms
open System.Reflection
open System.Runtime.InteropServices
open OpenTK
open OpenTK.Graphics.OpenGL
open GLUtils

[<StructLayout(LayoutKind.Sequential)>]
type Vertex =
    struct
        val position : Vector3
        val color : Vector3
        new (pos, col) = { position = pos; color = col }
    end

let vertices = [ 
    Vertex (Vector3 (0.5f, 0.5f, 0.0f), Vector3 (1.0f, 0.0f, 0.0f))
    Vertex (Vector3 (0.5f, -0.5f, 0.0f), Vector3 (1.0f, 0.0f, 0.0f))
    Vertex (Vector3 (-0.5f, -0.5f, 0.0f), Vector3 (1.0f, 0.0f, 0.0f))
    Vertex (Vector3 (-0.5f, 0.5f, 0.0f), Vector3 (1.0f, 0.0f, 0.0f))
    Vertex (Vector3 (1.0f, 1.0f, 1.0f), Vector3 (0.0f, 1.0f, 0.0f))
    Vertex (Vector3 (1.0f, 0.0f, 1.0f), Vector3 (0.0f, 1.0f, 0.0f))
    Vertex (Vector3 (0.0f, 0.0f, 1.0f), Vector3 (0.0f, 1.0f, 0.0f))
    Vertex (Vector3 (0.0f, 1.0f, 1.0f), Vector3 (0.0f, 1.0f, 0.0f))
]

let indices = [ 
    0; 1; 2; 2; 3; 0;
    6; 5; 4; 4; 7; 6;
]

type SimpleExample () = 
    inherit GameWindow (600, 600)
    
    let program = loadProgram [ 
                    (ShaderType.FragmentShader, "Fragment.glsl")
                    (ShaderType.VertexShader, "Vertex.glsl")
                ]
    let vbo = initVertexBuffer (List.toSeq vertices) BufferTarget.ArrayBuffer
    let ibo = initVertexBuffer (List.toSeq indices) BufferTarget.ElementArrayBuffer
    let time = getUniform program "time"
    let loopDuration = getUniform program "loopDuration"
    let mutable elapsedTime = 0.0f

    member this.Init () =
        initVertexArrayObject ()
        GL.Enable (EnableCap.CullFace)
        GL.CullFace (CullFaceMode.Back)
        GL.FrontFace (FrontFaceDirection.Cw)
        GL.Viewport (this.ClientSize)

    override this.OnRenderFrame args =
        base.OnRenderFrame args
        GL.Clear (ClearBufferMask.ColorBufferBit)
        elapsedTime <- elapsedTime + float32 (args.Time)
        loopDuration &= 5.0f
        time &= elapsedTime
        drawVertexBuffer<Vertex> program vbo ibo
        this.SwapBuffers ()

[<EntryPoint>]
[<STAThread>]
let main args =
    let form = new SimpleExample ()
    form.Init ()
    form.Run ()
    0