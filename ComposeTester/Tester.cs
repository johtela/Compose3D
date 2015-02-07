namespace ComposeTester
{
	using System;
	using System.Runtime.InteropServices;
	using OpenTK;
	using OpenTK.Graphics;
	using OpenTK.Graphics.OpenGL;
	using OpenTK.Input;
	using Compose3D;
    using Compose3D.GLSL;
    using Compose3D.GLTypes;
	using Compose3D.Geometry;

	[StructLayout(LayoutKind.Sequential)]
	public struct Vertex : IVertex
	{
		private Vector3 position;
		private Vector4 color;
        private Vector3 normal;

		public Vector3 Position
		{
			get { return position; }
			set { position = value; }
		}

		public Vector4 Color
		{
			get { return color; }
			set { color = value; }
		}

        public Vector3 Normal
        {
            get { return normal; }
            set { normal = value; }
        }
	}

	public class TestWindow : GameWindow
	{
		private Program _program;
		private VBO<Vertex> _vbo;
		private VBO<int> _ibo;
		private Vector3 _orientation;
		private Uniform<Matrix4> _worldMatrix;
		private Uniform<Matrix4> _perspectiveMatrix;
        private Uniform<Matrix3> _normalMatrix;
        private Uniform<Vector3> _dirToLight;

		public TestWindow ()
			: base (800, 600, GraphicsMode.Default, "Compose3D")
		{
			var cube1 = Cube.Create<Vertex> (1.0f, 1.5f, 2.0f).Rotate (0.0f, MathHelper.PiOver2, 0.0f)
				.Material (Material.RepeatColors (Color.Random, Color.White, Color.Random));
			var cube2 = Cube.Create<Vertex> (1.0f, 1.0f, 1.0f).Scale (0.8f, 0.8f, 0.8f)
				.Material (Material.RepeatColors (Color.Random, Color.White, Color.Random));
			var cube3 = Cube.Create<Vertex> (1.0f, 1.0f, 2.0f)
				.Material (Material.RepeatColors (Color.Random, Color.White, Color.Random));
			var geometry = Composite.StackRight (Align.Center, Align.Center, cube1, cube2, cube3).Center ();
			_program = new Program (
				new Shader (ShaderType.FragmentShader, @"Shaders/Fragment.glsl"),
				new Shader (ShaderType.VertexShader, @"Shaders/Vertex.glsl"));

			_vbo = new VBO<Vertex> (geometry.Vertices, BufferTarget.ArrayBuffer);
			_ibo = new VBO<int> (geometry.Indices, BufferTarget.ElementArrayBuffer);

			_orientation = new Vector3 (0.0f, 0.0f, 3.0f);
			_worldMatrix = _program.GetUniform<Matrix4> ("worldMatrix");
			_perspectiveMatrix = _program.GetUniform<Matrix4> ("perspectiveMatrix");
            _normalMatrix = _program.GetUniform<Matrix3> ("normalMatrix");
            _dirToLight = _program.GetUniform<Vector3> ("dirToLight");
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
            var worm = Matrix4.CreateRotationY (_orientation.Y) * Matrix4.CreateRotationX (_orientation.X)
				* Matrix4.CreateTranslation (0f, 0f, -_orientation.Z);
			_worldMatrix &= worm;
            var norm = new Matrix3 (worm);
            norm.Invert ();
            norm.Transpose ();
            _normalMatrix &= norm;
            _dirToLight &= new Vector3 (0f, 0f, 1f);
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
			_perspectiveMatrix &= Matrix4.CreateScale ((float)ClientSize.Height / (float)ClientSize.Width, 1.0f, 1.0f) *
				Matrix4.CreatePerspectiveOffCenter (-1.0f, 1.0f, -1.0f, 1.0f, 1.0f, 100.0f);
			GL.Viewport (ClientSize);
		}

		protected override void OnMouseMove (MouseMoveEventArgs e)
		{
			base.OnMouseMove (e);
			if (e.Mouse.IsButtonDown (MouseButton.Left))
			{
				var rotX = ((float)e.YDelta / 500.0f) * MathHelper.Pi;
				var rotY = ((float)e.XDelta / 500.0f) * MathHelper.Pi;
				_orientation += new Vector3 (rotX, rotY, 0.0f);
				UpdateWorldMatrix ();
			}
		}

		protected override void OnMouseWheel (MouseWheelEventArgs e)
		{
			base.OnMouseWheel (e);
			_orientation.Z += e.DeltaPrecise * -0.1f;
			_orientation.Z = Math.Max (_orientation.Z, 2.0f);
			UpdateWorldMatrix ();
		}

		[STAThread]
		static void Main (string[] args)
		{
			var wnd = new TestWindow ();
			wnd.Init ();
			wnd.Run ();

            var v1 = new Vec2 (1f, 2f);
            var v2 = -v1;
            var r = v1 + v2;
		}
	}
}
