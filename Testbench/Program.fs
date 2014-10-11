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
        val position : Vector4
        val color : Vector4
        new (pos, col) = { position = pos; color = col }
    end

let vertices = [ 
    Vertex (Vector4 (0.0f, 0.5f, 0.0f, 1.0f), Vector4 (1.0f, 0.0f, 0.0f, 1.0f))
    Vertex (Vector4 (0.5f, -0.366f, 0.0f, 1.0f), Vector4 (0.0f, 1.0f, 0.0f, 1.0f))
    Vertex (Vector4 (-0.5f, -0.366f, 0.0f, 1.0f), Vector4 (0.0f, 0.0f, 1.0f, 1.0f))
]

type SimpleExample () = 
    inherit GameWindow (600, 600)
    
    let mutable program = 0;

    member this.init () =
        program <- GLUtils.loadProgram [ 
            (ShaderType.FragmentShader, "Fragment.glsl")
            (ShaderType.VertexShader, "Vertex.glsl")
        ]

    override this.OnRenderFrame args =
        base.OnRenderFrame args
        GL.Viewport (0, 0, 600, 600)

        let (vbo, count) = GLUtils.initVertexBuffer (List.toSeq vertices) 
        GLUtils.drawVertexBuffer<Vertex> program vbo count
        this.SwapBuffers ()

[<EntryPoint>]
[<STAThread>]
let main args =
    let form = new SimpleExample ()
    form.init ()
    form.Run ()
    0