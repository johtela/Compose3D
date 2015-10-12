namespace ComposeTester
{
    using Compose3D.Arithmetics;
    using Compose3D.Geometry;
    using Compose3D.GLTypes;
	using Compose3D.Reactive;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Input;
    using System;

	public class TestWindow : GameWindow
	{
		// OpenGL objects
		private Program _program;
		private VBO<Vertex> _vbo;
		private VBO<int> _ibo;
		private VBO<Vertex> _normalVbo;
        private Uniforms _uniforms;

		// Scene state
		private Vec2 _orientation;
		private Vec3 _position;

		// Scene geometry
        private Geometry<Vertex> _geometry;

        public TestWindow ()
			: base (800, 600, GraphicsMode.Default, "Compose3D")
		{
			_geometry = Geometries.House ();
			_orientation = new Vec2 (0f, 0f);
			_position = new Vec3 (0f, 0f, -40f);

            _vbo = new VBO<Vertex> (_geometry.Vertices, BufferTarget.ArrayBuffer);
            _ibo = new VBO<int> (_geometry.Indices, BufferTarget.ElementArrayBuffer);
            var normals = _geometry.Normals;
            normals.Color (VertexColor.Uniform (VertexColor.White));
			_normalVbo = new VBO<Vertex> (normals, BufferTarget.ArrayBuffer);
			_program = new Program (Shaders.VertexShader (), Shaders.FragmentShader ());
            _program.InitializeUniforms (_uniforms = new Uniforms ());
        }

		public void Init ()
        {
            SetupOpenGL ();
            SetupLights ();
            SetupReactions ();
        }

        #region Setup

        private static void SetupOpenGL ()
        {
            GL.Enable (EnableCap.CullFace);
            GL.CullFace (CullFaceMode.Back);
            GL.FrontFace (FrontFaceDirection.Cw);
            GL.Enable (EnableCap.DepthTest);
            GL.DepthMask (true);
            GL.DepthFunc (DepthFunction.Less);
        }

        private void SetupLights ()
        {
			_uniforms.globalLighting &= new GlobalLighting ()
			{
				ambientLightIntensity = new Vec3 (0.1f),
				maxintensity = 2f,
				inverseGamma = 1f / 2.2f
			};
            _uniforms.directionalLight &= new DirectionalLight ()
            {
                direction = new Vec3 (-1f, 1f, 1f),
				intensity = new Vec3 (0.1f)
            };
            _uniforms.pointLight &= new PointLight
            {
                position = new Vec3 (10f, 10f, -10f),
				intensity = new Vec3 (2f),
				linearAttenuation = 0.005f,
				quadraticAttenuation = 0.005f
            };
        }

        private void SetupReactions ()
        {
            React.By<double> (Render)
                .WhenRendered (this);

            React.By<Vec2> (ResizeViewport)
                .WhenResized (this);

            React.By<Vec2> (RotateView)
                .Map<MouseMoveEventArgs, Vec2> (e =>
                    new Vec2 (e.YDelta.ToRadians () / 2f, e.XDelta.ToRadians () / 2f))
                .Filter (e => e.Mouse.IsButtonDown (MouseButton.Left))
                .WhenMouseMovesOn (this);

            React.By<float> (ZoomView)
				.Map (delta => delta * -0.2f)
                .WhenMouseWheelDeltaChangesOn (this);
        }

        #endregion

        #region Update operations

        private void UpdateWorldMatrix ()
        {
			var worm = Mat.Translation<Mat4> (_position.ToArray<Vec3, float> ()) *
                Mat.RotationY<Mat4> (_orientation.Y) * Mat.RotationX<Mat4> (_orientation.X);
            _uniforms.worldMatrix &= worm;
            _uniforms.normalMatrix &= new Mat3 (worm).Inverse.Transposed;
        }

        private void Render (double time)
        {
            GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _program.DrawTriangles<Vertex> (_vbo, _ibo);
            _program.DrawNormals<Vertex> (_normalVbo);
            SwapBuffers ();
        }

        private void ResizeViewport (Vec2 size)
        {
            UpdateWorldMatrix ();
            _uniforms.perspectiveMatrix &= Mat.Scaling<Mat4> (size.Y / size.X, 1f, 1f) *
                Mat.PerspectiveOffCenter (-1f, 1f, -1f, 1f, 1f, 100f);
            GL.Viewport (ClientSize);
        }

        private void RotateView (Vec2 rot)
        {
			_orientation += rot;
            UpdateWorldMatrix ();
        }

        private void ZoomView (float delta)
        {
			_position.Z = Math.Min (_position.Z + delta, 2f);
            UpdateWorldMatrix ();
        }

        #endregion

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
