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
    
    let program = GLUtils.loadProgram [ 
                    (ShaderType.FragmentShader, "Fragment.glsl")
                    (ShaderType.VertexShader, "Vertex.glsl")
                ]
    let time = GLUtils.getUniform program "time"
    let loopDuration = GLUtils.getUniform program "lopDuration"
    let mutable elapsedTime = 0.0f

    override this.OnRenderFrame args =
        base.OnRenderFrame args
        elapsedTime <- elapsedTime + float32 (args.Time)
        GL.Clear (ClearBufferMask.ColorBufferBit)
        GL.Viewport (0, 0, 600, 600)
        GL.Uniform1 (loopDuration, 5.0f)
        GL.Uniform1 (time, elapsedTime)
        let (vbo, count) = GLUtils.initVertexBuffer (List.toSeq vertices) 
        GLUtils.drawVertexBuffer<Vertex> program vbo count
        this.SwapBuffers ()

[<EntryPoint>]
[<STAThread>]
let main args =
    let form = new SimpleExample ()
    form.Run ()
    0