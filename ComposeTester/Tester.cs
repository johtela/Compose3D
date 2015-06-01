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
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

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
			_program = new Program (Shaders.VertexShader (), Shaders.FragmentShader ());
            _program.InitializeUniforms (_uniforms = new Uniforms ());
        }

        private Geometry<Vertex> CreateGeometry ()
        {
            var cube1 = Cube.Create<Vertex> (1f, 1.5f, 2f).Rotate (0f, MathHelper.PiOver2, 0f)
                .Material (Material.RepeatColors (Color.Random));
            var cube2 = Cube.Create<Vertex> (1f, 1f, 1f).Scale (0.8f, 0.8f, 0.8f)
                .Material (Material.RepeatColors (Color.Random));
            var cube3 = Cube.Create<Vertex> (1f, 1f, 2f)
                .Material (Material.RepeatColors (Color.Random));
            return Composite.StackRight (Align.Center, Align.Center, cube1, cube2, cube3).Center ();
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
