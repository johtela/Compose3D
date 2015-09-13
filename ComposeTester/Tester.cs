namespace ComposeTester
{
    using Compose3D.Arithmetics;
    using Compose3D.Geometry;
    using Compose3D.GLTypes;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Input;
    using System;

	public class TestWindow : GameWindow
	{
		private Program _program;
		private VBO<Vertex> _vbo;
		private VBO<int> _ibo;
		private VBO<Vertex> _normalVbo;
		private Vector3 _orientation;
        private Uniforms _uniforms;
        private Geometry<Vertex> _geometry;

        public TestWindow ()
			: base (800, 600, GraphicsMode.Default, "Compose3D")
		{
			_geometry = Geometries.Pipe ();
			_orientation = new Vector3 (0f, 0f, 40f);
            _vbo = new VBO<Vertex> (_geometry.Vertices, BufferTarget.ArrayBuffer);
            _ibo = new VBO<int> (_geometry.Indices, BufferTarget.ElementArrayBuffer);
			_normalVbo = new VBO<Vertex> (_geometry.Normals, BufferTarget.ArrayBuffer);
			_program = new Program (Shaders.VertexShader (), Shaders.FragmentShader ());
            _program.InitializeUniforms (_uniforms = new Uniforms ());
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
			_uniforms.ambientLightIntensity &= new Vec3 (0.0f);
            _uniforms.directionalLight &= new DirectionalLight ()
            {
				direction = new Vec3 (1f, 1f, 1f),
				intensity = new Vec3 (0.0f)
            };
			_uniforms.spotLights &= new SpotLight[1] {
				new SpotLight () {
					position = new Vec3 (-10f, 10f, -10f),
					direction = new Vec3 (1f, -1f, -1f),
					intensity = new Vec3 (10f, 10f, 10f),
					linearAttenuation = 0.1f,
					quadraticAttenuation = 0.1f,
					cosSpotCutoff = 0.1f,
					spotExponent = 1f
				}
			};
		}

		protected override void OnRenderFrame (FrameEventArgs e)
		{
			base.OnRenderFrame (e);
			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			_program.DrawTriangles<Vertex> (_vbo, _ibo);
			_program.DrawNormals<Vertex> (_normalVbo);
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
			Console.WriteLine (GL.GetString (StringName.Version));
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
