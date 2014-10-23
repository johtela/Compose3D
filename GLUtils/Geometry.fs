module Geometry

// Disable warning about non-verifiable code
#nowarn "9"

open System
open System.Runtime.InteropServices
open OpenTK
open OpenTK.Graphics.OpenGL

[<StructLayout(LayoutKind.Sequential)>]
type Vertex =
    struct
        val position : Vector4
        val color : Vector3
        new (pos, col) = { position = pos; color = col }
    end

let rectVertices width height color (matrix : Matrix4) =
    let right = width / 2.0f
    let top = height / 2.0f
    let vec x y =
        let v = Vector4 (x, y, 0.0f, 1.0f)
        Vector4.Transform (ref v, ref matrix)
    [| Vertex (vec right top , color)
       Vertex (vec right -top, color)
       Vertex (vec -right -top, color)
       Vertex (vec -right top, color)
    |]

let rectIndices = 
    [| 0; 1; 2; 2; 3; 0|] 
