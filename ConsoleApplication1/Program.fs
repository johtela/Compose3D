﻿// Disable warning about non-verifiable code
#nowarn "9"

open System
open System.Drawing
open System.Windows.Forms
open System.Reflection
open System.Runtime.InteropServices
open OpenTK
open OpenTK.Graphics.OpenGL

[<AttributeUsage (AttributeTargets.Field)>]
type VertexAttrAttribute (index) = 
    inherit Attribute ()
    member this.Index = index

[<StructLayout(LayoutKind.Sequential)>]
type Vertex = {
    [<VertexAttr (0)>]
    position : Vector3;
    [<VertexAttr (1)>]
    color : Vector3
}

let getVertexAttrs (vertex : 'a) =
    let t = typeof<'a>
    let vat = typeof<VertexAttrAttribute>
    let getResPair (f : FieldInfo) =
        let vattr = Seq.exactlyOne (f.GetCustomAttributes (vat, false)) :?> VertexAttrAttribute
        (vattr.Index, Marshal.SizeOf (f.FieldType))
    t.GetFields (BindingFlags.Public) |>
        Seq.filter (fun f -> f.IsDefined (vat, false)) |>
        Seq.map getResPair
        

let initVertexBuffer (vertices : seq<'a>) (buffer : int) =
    let vbo = GL.GenBuffer ()
    let varr = Seq.toArray vertices
    let size = sizeof<'a> * varr.Length |> nativeint
    GL.BufferData (BufferTarget.ArrayBuffer, size, varr, BufferUsageHint.StaticDraw)
    GL.BindBuffer (BufferTarget.ArrayBuffer, buffer)
    vbo

type SimpleExample () = 
    inherit GameWindow (600, 600)
    
    let mutable frames = 0

    member this.init () =
        this.createShaders ()

    member this.createShaders () =
        let program = GL.CreateProgram ()
        let createShader stype src =
            let shader = GL.CreateShader stype
            GL.ShaderSource (shader, src)
            GL.CompileShader shader
            GL.AttachShader (program, shader)

        createShader ShaderType.FragmentShader "vec3 hsv(float h) { if(h==0.0) return vec3(0.0,0.0,0.0); float h6 = h*6.0; float x = 1.0 - abs(mod(h6,2.0)-1.0); if(h6<1.0) return vec3(1.0,x,0.0); if(h6<2.0) return vec3(x,1.0,0.0); if(h6<3.0) return vec3(0.0,1.0,x); if(h6<4.0) return vec3(0.0,x,1.0); if(h6<5.0) return vec3(x,0.0,1.0); return vec3(1.0,0.0,x);  }"
        createShader ShaderType.FragmentShader "varying vec2 texCoord; varying float scale;  vec3 hsv(float h); void main() { vec2 z = vec2(0.0); vec2 c = texCoord; float iter = 0.0; float step = pow(scale-0.00004,-0.3)/800.0 ; for(iter=0.0; iter<1.0; iter+=step) { float t = z.x; z.x = z.x*z.x - z.y*z.y + c.x; z.y = 2.0*t*z.y + c.y; if(z.x*z.x+z.y*z.y>4.0) break; } float mixfactor = pow(iter,pow(scale+0.01,0.1)/0.9); gl_FragColor = vec4(hsv(mixfactor),1); }"
        createShader ShaderType.VertexShader "varying vec2 texCoord; varying float scale; void main() { gl_Position = ftransform(); texCoord = gl_Vertex.xy; scale = gl_ModelViewMatrix[2][2]/4000; }"
        GL.LinkProgram program
        GL.UseProgram program

    override this.OnRenderFrame args =
        base.OnRenderFrame args
        frames <- frames + 1
        let renderTime = (float frames)
        GL.Clear ClearBufferMask.ColorBufferBit
        GL.Viewport (0, 0, this.ClientSize.Width, this.ClientSize.Height)

        GL.LoadIdentity ()
        GL.Rotate (-renderTime / 100.0, 0.0, 0.0, 1.0)
        let scale = Math.Pow (45.0 * (1.01 - Math.Cos (renderTime / 5000.0)), 2.0)
        GL.Scale (scale, scale, scale)
        GL.Translate (0.74003, -0.18828, 0.0)

        GL.Begin PrimitiveType.TriangleStrip
        GL.Vertex2 [| -2.0; -2.0 |]
        GL.Vertex2 [| -2.0;  2.0 |]
        GL.Vertex2 [|  2.0; -2.0 |]
        GL.Vertex2 [|  2.0;  2.0 |]
        GL.End ()

        this.SwapBuffers ()

[<EntryPoint>]
[<STAThread>]
let main args =
    let form = new SimpleExample ()
    form.init ()
    form.Run ()
    0