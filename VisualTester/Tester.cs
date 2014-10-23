namespace VisualTester
{
	using System;
	using System.Runtime.InteropServices;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;
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
		private Uniform<float> _time;
		private Uniform<float> _loopDuration;
		private Uniform<Matrix4> _perspectiveMatrix;
		float _elapsedTime;

		public TestWindow ()
			: base (600, 600)
		{
			var color = new Vector4 (1.0f, 0.0f, 0.0f, 0.0f);
			var matrix = Matrix4.CreateTranslation (0.0f, 0.0f, -3.0f);
			var geometry = Geometry.Cube<Vertex> (1.0f, 1.0f, 1.0f, color).Transform (matrix);

			_program = new Program (
				new Shader (ShaderType.FragmentShader, @"Shaders\Fragment.glsl"),
				new Shader (ShaderType.VertexShader, @"Shaders\Vertex.glsl"));

			_vbo = new VBO<Vertex> (geometry.Vertices, BufferTarget.ArrayBuffer);
			_ibo = new VBO<int> (geometry.Indices, BufferTarget.ElementArrayBuffer);

			_time = _program.GetUniform<float> ("time");
			_loopDuration = _program.GetUniform<float> ("loopDuration");
			_perspectiveMatrix = _program.GetUniform<Matrix4> ("perspectiveMatrix") &
				Matrix4.CreatePerspectiveOffCenter (-1.0f, 1.0f, -1.0f, 1.0f, 1.0f, 100.0f);
		}

		public void Init ()
		{
			GL.Enable (EnableCap.CullFace);
			GL.CullFace (CullFaceMode.Back);
			GL.FrontFace (FrontFaceDirection.Cw);
			GL.Viewport (this.ClientSize);
		}

		protected override void OnRenderFrame (FrameEventArgs e)
		{
			base.OnRenderFrame (e);
			GL.Clear (ClearBufferMask.ColorBufferBit);
			_elapsedTime = _elapsedTime + (float)e.Time;
			_loopDuration &= 5.0f;
			_time &= _elapsedTime;
			_program.DrawVertexBuffer<Vertex> (_vbo, _ibo);
			SwapBuffers ();
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
