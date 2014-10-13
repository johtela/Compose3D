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
    Vertex (Vector3 (0.0f, 0.5f, 0.0f), Vector3 (1.0f, 0.0f, 0.0f))
    Vertex (Vector3 (0.5f, -0.366f, 0.0f), Vector3 (0.0f, 1.0f, 0.0f))
    Vertex (Vector3 (-0.5f, -0.366f, 0.0f), Vector3 (0.0f, 0.0f, 1.0f))
    Vertex (Vector3 (0.0f, -0.5f, 0.0f), Vector3 (1.0f, 0.0f, 0.0f))
    Vertex (Vector3 (0.5f, 0.366f, 0.0f), Vector3 (0.0f, 1.0f, 0.0f))
    Vertex (Vector3 (-0.5f, 0.366f, 0.0f), Vector3 (0.0f, 0.0f, 1.0f))
]

type SimpleExample () = 
    inherit GameWindow (600, 600)
    
    let program = loadProgram [ 
                    (ShaderType.FragmentShader, "Fragment.glsl")
                    (ShaderType.VertexShader, "Vertex.glsl")
                ]
    let time = getUniform program "time"
    let loopDuration = getUniform program "loopDuration"
    let mutable elapsedTime = 0.0f

    override this.OnRenderFrame args =
        base.OnRenderFrame args
        elapsedTime <- elapsedTime + float32 (args.Time)
        GL.Clear (ClearBufferMask.ColorBufferBit)
        GL.Viewport (0, 0, 600, 600)
        loopDuration &= 5.0f
        time &= elapsedTime
        let vbo = initVertexBuffer (List.toSeq vertices) 
        drawVertexBuffer<Vertex> program vbo
        this.SwapBuffers ()

[<EntryPoint>]
[<STAThread>]
let main args =
    let form = new SimpleExample ()
    form.Run ()
    0