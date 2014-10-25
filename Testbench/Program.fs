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
open Geometry

type SimpleExample () = 
    inherit GameWindow (600, 600)
    
    let program = loadProgram [ 
                    (ShaderType.FragmentShader, "Fragment.glsl")
                    (ShaderType.VertexShader, "Vertex.glsl")
                ]
    let matrix = Matrix4.CreateTranslation (0.0f, 0.0f, -2.0f)
    let vertices = Geometry.rectVertices 1.0f 1.0f (Vector3 (1.0f, 0.0f, 0.0f)) matrix
    let vbo = initVertexBuffer vertices BufferTarget.ArrayBuffer
    let ibo = initVertexBuffer Geometry.rectIndices BufferTarget.ElementArrayBuffer
    let time = getUniform program "time"
    let loopDuration = getUniform program "loopDuration"
    let perspectiveMatrix = getUniform program "perspectiveMatrix"
    let mutable elapsedTime = 0.0f
    do perspectiveMatrix &= Matrix4.CreatePerspectiveOffCenter (-1.0f, 1.0f, -1.0f, 1.0f, 1.0f, 100.0f)

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