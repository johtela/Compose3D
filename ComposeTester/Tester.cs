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
        [Builtin]
        internal Vec4 gl_Position = new Vec4 ();
        [GLQualifier ("smooth")]
        internal Vec4 theColor = new Vec4 ();
    }

    public struct Uniforms
    {
        internal Uniform<Mat4> worldMatrix;
        internal Uniform<Mat4> perspectiveMatrix;
        internal Uniform<Mat3> normalMatrix;
        internal Uniform<Vec3> dirToLight;
    }

	public class TestWindow : GameWindow
	{
		private Program _program;
		private VBO<Vertex> _vbo;
		private VBO<int> _ibo;
		private Vector3 _orientation;
        private Uniforms _uniforms;

        public TestWindow ()
			: base (800, 600, GraphicsMode.Default, "Compose3D")
		{
			var cube1 = Cube.Create<Vertex> (1f, 1.5f, 2f).Rotate (0f, MathHelper.PiOver2, 0f)
				.Material (Material.RepeatColors (Color.Random, Color.White, Color.Random));
			var cube2 = Cube.Create<Vertex> (1f, 1f, 1f).Scale (0.8f, 0.8f, 0.8f)
				.Material (Material.RepeatColors (Color.Random, Color.White, Color.Random));
			var cube3 = Cube.Create<Vertex> (1f, 1f, 2f)
				.Material (Material.RepeatColors (Color.Random, Color.White, Color.Random));
			var geometry = Composite.StackRight (Align.Center, Align.Center, cube1, cube2, cube3).Center ();

            var vertexShader = new VertexShader<Vertex, Uniforms, Fragment> ((v, u) => new Fragment()
            {
                gl_Position = !u.perspectiveMatrix * !u.worldMatrix * new Vec4(v.position, 1f),
                theColor = v.color * (!u.normalMatrix * v.normal).Normalized.Dot (!u.dirToLight)
            });
			
            _program = new Program (
				Shader.FromFile (ShaderType.FragmentShader, @"Shaders/Fragment.glsl"),
                vertexShader);

			_vbo = new VBO<Vertex> (geometry.Vertices, BufferTarget.ArrayBuffer);
			_ibo = new VBO<int> (geometry.Indices, BufferTarget.ElementArrayBuffer);

			_orientation = new Vector3 (0f, 0f, 3f);
			_uniforms.worldMatrix = _program.GetUniform<Mat4> ("worldMatrix");
            _uniforms.perspectiveMatrix = _program.GetUniform<Mat4> ("perspectiveMatrix");
            _uniforms.normalMatrix = _program.GetUniform<Mat3> ("normalMatrix");
            _uniforms.dirToLight = _program.GetUniform<Vec3> ("dirToLight");
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
            _uniforms.dirToLight &= new Vec3 (0f, 0f, 1f);
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
            Tester.RunTestsTimed (
                new VecTests (),
                new MatTests ());
                //new PerformanceTests ());
            Console.ReadLine ();
		}
	}
}
