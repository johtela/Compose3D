namespace ComposeTester
{
    using Compose3D.Arithmetics;
    using Compose3D.Geometry;
    using Compose3D.GLTypes;
    using LinqCheck;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Input;
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public struct Vertex : IVertex
	{
		internal Vec3 position;
        internal Vec4 color;
        internal Vec3 normal;

		public Vec3 Position
		{
			get { return position; }
			set { position = value; }
		}

		public Vec4 Color
		{
			get { return color; }
			set { color = value; }
		}

        public Vec3 Normal
        {
            get { return normal; }
            set { normal = value; }
        }
	}

    public class Fragment
    {
        [Builtin] internal Vec4 gl_Position = new Vec4 ();
        [GLQualifier ("smooth")]
        internal Vec4 theColor = new Vec4 ();
    }

    public class FrameBuffer
    {
        internal Vec4 outputColor = new Vec4 ();
    }

    [GLStruct ("DirectionalLight")]
    public struct DirectionalLight
    {
        internal Vec3 intensity;
        internal Vec3 direction;
    }

    [GLStruct ("SpotLight")]
    public struct SpotLight
    {
        internal Vec3 position;
        internal Vec3 direction;
        internal Vec3 intensity;
        internal float linearAttenuation, quadraticAttenuation;
        internal float cosSpotCutoff, spotExponent;
    }

    public class Uniforms
    {
        internal Uniform<Mat4> worldMatrix;
        internal Uniform<Mat4> perspectiveMatrix;
        internal Uniform<Mat3> normalMatrix;
        internal Uniform<Vec3> ambientLightIntensity;
        internal Uniform<DirectionalLight> directionalLight;
        [GLArray (4)] 
        internal Uniform<SpotLight[]> spotLights;
    }

	public class TestWindow : GameWindow
	{
		private Program _program;
		private VBO<Vertex> _vbo;
		private VBO<int> _ibo;
		private Vector3 _orientation;
        private Uniforms _uniforms;
        private Geometry<Vertex> _geometry;

        public TestWindow ()
			: base (800, 600, GraphicsMode.Default, "Compose3D")
		{
            _geometry = CreateGeometry ();
            _orientation = new Vector3 (0f, 0f, 3f);
            _vbo = new VBO<Vertex> (_geometry.Vertices, BufferTarget.ArrayBuffer);
            _ibo = new VBO<int> (_geometry.Indices, BufferTarget.ElementArrayBuffer);
            _program = new Program (VertexShader (), FragmentShader ());
            _program.InitializeUniforms (_uniforms = new Uniforms ());
        }

        private Geometry<Vertex> CreateGeometry ()
        {
            var cube1 = Cube.Create<Vertex> (1f, 1.5f, 2f).Rotate (0f, MathHelper.PiOver2, 0f)
                .Material (Material.RepeatColors (Color.Random, Color.White, Color.Random));
            var cube2 = Cube.Create<Vertex> (1f, 1f, 1f).Scale (0.8f, 0.8f, 0.8f)
                .Material (Material.RepeatColors (Color.Random, Color.White, Color.Random));
            var cube3 = Cube.Create<Vertex> (1f, 1f, 2f)
                .Material (Material.RepeatColors (Color.Random, Color.White, Color.Random));
            return Composite.StackRight (Align.Center, Align.Center, cube1, cube2, cube3).Center ();
        }

        private Shader VertexShader ()
        {
            return Shader.Create (ShaderType.VertexShader,
                from v in new ShaderObject<Vertex> (ShaderObjectKind.Input)
                from u in new ShaderObject<Uniforms> (ShaderObjectKind.Uniform)
                let normalizedNormal = (!u.normalMatrix * v.normal).Normalized
                let angle = normalizedNormal.Dot ((!u.directionalLight).direction)
                let ambient = new Vec4 (!u.ambientLightIntensity, 0f)
                let diffuse = new Vec4 ((!u.directionalLight).intensity, 0f) * angle
                let spot = (from sp in new ShaderObject<SpotLight> (!u.spotLights)
                            let vecToLight = sp.position - v.position
                            let dist = vecToLight.Length
                            let lightDir = vecToLight.Normalized
                            let attenuation = 1f / 
                                ((sp.linearAttenuation * dist) + (sp.quadraticAttenuation * dist * dist))
                            let cosAngle = -lightDir.Dot (sp.direction)
                            select cosAngle < sp.cosSpotCutoff ? 
                                0f : attenuation *  cosAngle.Pow (sp.spotExponent))
                            .Aggregate (0f, (r, i) => r + i)
                select new Fragment ()
                {   
                    gl_Position = !u.perspectiveMatrix * !u.worldMatrix * new Vec4 (v.position, 1f),
                    theColor = v.color * (ambient + diffuse).Clamp (0f, 1f)
                });
        }

        private Shader FragmentShader ()
        {
            return Shader.Create (ShaderType.FragmentShader,
                from f in new ShaderObject<Fragment> (ShaderObjectKind.Input)
                select new { outputColor = f.theColor });
        }

		public void Init ()
		{
            GL.Enable (EnableCap.CullFace);
			GL.CullFace (CullFaceMode.Back);
			GL.FrontFace (FrontFaceDirection.Cw);
			GL.Enable (EnableCap.DepthTest);
			GL.DepthMask (true);
			GL.DepthFunc (DepthFunction.Less);
		}

		private void UpdateWorldMatrix ()
		{
            var worm = Mat.Translation<Mat4> (0f, 0f, -_orientation.Z) * 
                Mat.RotationY<Mat4> (_orientation.Y) * Mat.RotationX<Mat4> (_orientation.X);
			_uniforms.worldMatrix &= worm;
            _uniforms.normalMatrix &= new Mat3 (worm).Inverse.Transposed;
            _uniforms.ambientLightIntensity &= new Vec3 (0.5f);
            _uniforms.directionalLight &= new DirectionalLight ()
            {
                direction = new Vec3 (-1f, -1f, 1f),
                intensity = new Vec3 (0.5f)
            };
		}

		protected override void OnRenderFrame (FrameEventArgs e)
		{
			base.OnRenderFrame (e);
			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			_program.DrawVertexBuffer<Vertex> (_vbo, _ibo);
			SwapBuffers ();
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
			UpdateWorldMatrix ();
            _uniforms.perspectiveMatrix &= Mat.Scaling<Mat4> ((float)ClientSize.Height / (float)ClientSize.Width, 1f, 1f) *
				Mat.PerspectiveOffCenter (-1f, 1f, -1f, 1f, 1f, 100f);
            GL.Viewport (ClientSize);
        }

		protected override void OnMouseMove (MouseMoveEventArgs e)
		{
			base.OnMouseMove (e);
			if (e.Mouse.IsButtonDown (MouseButton.Left))
			{
				var rotX = ((float)e.YDelta / 500f) * MathHelper.Pi;
				var rotY = ((float)e.XDelta / 500f) * MathHelper.Pi;
				_orientation += new Vector3 (rotX, rotY, 0f);
				UpdateWorldMatrix ();
			}
		}

		protected override void OnMouseWheel (MouseWheelEventArgs e)
		{
			base.OnMouseWheel (e);
			_orientation.Z += e.DeltaPrecise * -0.1f;
			_orientation.Z = Math.Max (_orientation.Z, 2f);
			UpdateWorldMatrix ();
		}

		[STAThread]
		static void Main (string[] args)
        {
            var wnd = new TestWindow ();
            wnd.Init ();
            wnd.Run ();
            //Tester.RunTestsTimed (
            //    new VecTests (),
            //    new MatTests ());
            ////new PerformanceTests ());
            //Console.ReadLine ();
		}
	}
}
