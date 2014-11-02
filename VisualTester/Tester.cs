namespace VisualTester
{
	using System;
	using System.Runtime.InteropServices;
	using OpenTK;
	using OpenTK.Graphics;
	using OpenTK.Graphics.OpenGL;
	using OpenTK.Input;
	using Visual3D;
	using Visual3D.GLTypes;

	[StructLayout(LayoutKind.Sequential)]
	public struct Vertex : IVertex
	{
		private Vector4 position;
		private Vector4 color;

		public Vector4 Position
		{
			get { return position; }
			set { position = value; }
		}

		public Vector4 Color
		{
			get { return color; }
			set { color = value; }
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
		float _elapsedTime;

		public TestWindow ()
			: base (800, 600, GraphicsMode.Default, "Visual3D")
		{
			var geometry = Geometry.Cube<Vertex> (1.0f, 1.5f, 2.0f)
				.Material (Material.RepeatColors (Color.White, Color.Blue, Color.White));
			_program = new Program (
				new Shader (ShaderType.FragmentShader, @"Shaders/Fragment.glsl"),
				new Shader (ShaderType.VertexShader, @"Shaders/Vertex.glsl"));

			_vbo = new VBO<Vertex> (geometry.Vertices, BufferTarget.ArrayBuffer);
			_ibo = new VBO<int> (geometry.Indices, BufferTarget.ElementArrayBuffer);

			_orientation = new Vector3 (0.0f, 0.0f, 3.0f);
			_worldMatrix = _program.GetUniform<Matrix4> ("worldMatrix");
			_perspectiveMatrix = _program.GetUniform<Matrix4> ("perspectiveMatrix");
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
			_worldMatrix &= Matrix4.CreateRotationY (_orientation.Y) * Matrix4.CreateRotationX (_orientation.X)
				* Matrix4.CreateTranslation (0.0f, 0.0f, -_orientation.Z);
		}

		protected override void OnRenderFrame (FrameEventArgs e)
		{
			base.OnRenderFrame (e);
			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			_elapsedTime = _elapsedTime + (float)e.Time;
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
				var rotX = ((float)e.YDelta / 200.0f) * MathHelper.Pi;
				var rotY = ((float)e.XDelta / 200.0f) * MathHelper.Pi;
				_orientation += new Vector3 (rotX, rotY, 0.0f);
				UpdateWorldMatrix ();
			}
		}

		protected override void OnMouseWheel (MouseWheelEventArgs e)
		{
			base.OnMouseWheel (e);
			var newCamera = _orientation + (_orientation * (e.DeltaPrecise * -0.1f));
			if (newCamera.Length >= 2.0f)
			{
				_orientation = newCamera;
				UpdateWorldMatrix ();
			}
		}

		[STAThread]
		static void Main (string[] args)
		{
			var wnd = new TestWindow ();
			wnd.Init ();
			wnd.Run ();
		}
	}
}
