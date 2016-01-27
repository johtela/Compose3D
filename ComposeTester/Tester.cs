namespace ComposeTester
{
	using Compose3D;
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.GLTypes;
	using Compose3D.Reactive;
	using Compose3D.SceneGraph;
	using Compose3D.Shaders;
	using Compose3D.Textures;
	using OpenTK;
	using OpenTK.Graphics;
	using OpenTK.Graphics.OpenGL;
	using OpenTK.Input;
	using System;
	using System.Linq;
	using LinqCheck;

	public class TestWindow : GameWindow
	{
		// OpenGL objects
		private Program _program;
		private	Program _passthrough;
		private Program _shadowShader;
		private ExampleShaders.Uniforms _uniforms;
		private ExampleShaders.ShadowUniforms _shadowUniforms;

		// Scene graph
		private SceneNode _sceneGraph;
		private TransformNode[] _positions;
		private Camera _camera;

		public TestWindow ()
			: base (800, 600, GraphicsMode.Default, "Compose3D")
		{
			_sceneGraph = CreateSceneGraph ();
			_positions = _sceneGraph.Descendants.OfType<TransformNode> ().ToArray ();

			_program = new Program (ExampleShaders.VertexShader (), ExampleShaders.FragmentShader ());
			_program.InitializeUniforms (_uniforms = new ExampleShaders.Uniforms ());

			_shadowShader = new Program (ExampleShaders.ShadowVertexShader (), ExampleShaders.ShadowFragmentShader ());
			_shadowShader.InitializeUniforms (_shadowUniforms = new ExampleShaders.ShadowUniforms ());

			_passthrough = new Program (
				GLShader.Create (ShaderType.VertexShader, 
					() =>
					from v in Shader.Inputs<PathNode> ()
					select new ColoredFragment ()
					{
						gl_Position = new Vec4 (v.position.X, v.position.Y, -1f, 1f),
						vertexDiffuse = v.color
					} 
				),
				GLShader.Create (ShaderType.FragmentShader, 
					() =>
					from f in Shader.Inputs<ColoredFragment> ()
					select new 
					{
						outputColor = f.vertexDiffuse
					}
				)
			);
		}

		public void Init ()
		{
			InitializeUniforms ();
			SetupReactions ();
		}

		#region Setup

		private SceneNode CreateSceneGraph ()
		{
			var dirLight = new DirectionalLight (new Vec3 (0.2f), new Vec3 (-1f, 1f, 1f));
			var pointLight1 = new PointLight (new Vec3 (1f), new Vec3 (10f, 10f, 10f), 0.001f, 0.001f);
			var pointLight2 = new PointLight (new Vec3 (2f), new Vec3 (-10f, 10f, -10f), 0.001f, 0.001f);

//			var textTexture = Texture.FromBitmap (
//				"This is a test".TextToBitmapCentered (1024, 1024, 100),
//				new TextureParams () 
//				{
//					{ TextureParameterName.TextureBaseLevel, 0 },
//					{ TextureParameterName.TextureMaxLevel, 0 }
//				});
//			var geometry = Solids.Cube<Vertex> (30f, 30f, 1f).Color (VertexColor<Vec3>.Chrome);
//			geometry.ApplyTextureFront<Vertex> (1f, new Vec2 (0f), new Vec2 (1f));
//			var mesh1 = new Mesh<Vertex> (geometry, tulipTexture)
//				.OffsetOrientAndScale (new Vec3 (15f, 0f, -20f), new Vec3 (0f), new Vec3 (1f));

//			var plasticTexture = Texture.FromFile ("Textures/Plastic.jpg", new TextureParams () 
//			{
//				{ TextureParameterName.TextureBaseLevel, 0 },
//				{ TextureParameterName.TextureMaxLevel, 0 }
//			});

			var fighter = new FighterGeometry<Vertex, PathNode> ();
//			fighter.Fighter.ApplyTextureTop<Vertex> (0.99f, new Vec2 (0f), new Vec2 (1f));
			var mesh1 = new Mesh<Vertex> (fighter.Fighter)
				.OffsetOrientAndScale (new Vec3 (0f, 0f, -10f), new Vec3 (0f), new Vec3 (1f));

			var house = Geometries.House ().Color (VertexColor<Vec3>.Brass);
			var mesh2 = new Mesh<Vertex> (house)
				.OffsetOrientAndScale (new Vec3 (-10f, 0f, -10f), new Vec3 (0f), new Vec3 (0.2f));
			
			_camera = new Camera (
				position: new Vec3 (20f, 20f, 20f), 
				target: new Vec3 (0f, 0f, 0f), 
				upDirection: new Vec3 (0f, 1f, 0f),
				frustrum: new ViewingFrustum (1f, 1f, 1f, 100f),
				aspectRatio: 1f);
			return new GlobalLighting (new Vec3 (0.2f), 2f, 1.2f).Add (
				dirLight, pointLight1, pointLight2, _camera, mesh1, mesh2);
		}

		private void InitializeUniforms ()
		{
			var numPointLights = 0;
			var pointLights = new Lighting.PointLight[4];

			using (_program.Scope ())
			{
				_sceneGraph.Traverse<GlobalLighting, DirectionalLight, PointLight> 
				(
					(globalLight, mat) =>
					_uniforms.globalLighting &= new Lighting.GlobalLight () 
					{
						ambientLightIntensity = globalLight.AmbientLightIntensity,
						maxintensity = globalLight.MaxIntensity,
						inverseGamma = 1f / globalLight.GammaCorrection
					},
					(dirLight, mat) =>
					_uniforms.directionalLight &= new Lighting.DirectionalLight () 
					{
						direction = dirLight.Direction,
						intensity = dirLight.Intensity
					},
					(pointLight, mat) =>
					pointLights [numPointLights++] = new Lighting.PointLight 
					{
						position = pointLight.Position,
						intensity = pointLight.Intensity,
						linearAttenuation = pointLight.LinearAttenuation,
						quadraticAttenuation = pointLight.QuadraticAttenuation
					}
				);
				_uniforms.pointLights &= pointLights;

				var samplers = new Sampler2D[4];
				for (int i = 0; i < samplers.Length; i++)
					samplers [i] = new Sampler2D (i, new SamplerParams () 
					{
						{ SamplerParameterName.TextureMagFilter, All.Linear },
						{ SamplerParameterName.TextureMinFilter, All.Linear },
						{ SamplerParameterName.TextureWrapR, All.ClampToEdge },
						{ SamplerParameterName.TextureWrapS, All.ClampToEdge }
					});
				_uniforms.samplers &= samplers;
			}
		}

		private void SetupReactions ()
		{
			React.By<double> (Render)
				.WhenRendered (this);

			React.By<Vec2> (ResizeViewport)
				.WhenResized (this);

			React.By<Vec3> (RotateView)
				.Map<MouseMoveEventArgs, Vec3> (e =>
					new Vec3 (e.YDelta.Radians () / 2f, e.XDelta.Radians () / 2f, 0f))
				.Filter (e => e.Mouse.IsButtonDown (MouseButton.Left))
				.WhenMouseMovesOn (this);

			React.By<float> (ZoomView)
				.Map (delta => delta * -0.2f)
				.WhenMouseWheelDeltaChangesOn (this);
		}

		#endregion

		#region Update operations

		private void Render (double time)
		{
			GL.ClearColor (new Color4 (0, 50, 150, 255));
			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			GL.Enable (EnableCap.CullFace);
			GL.CullFace (CullFaceMode.Back);
			GL.FrontFace (FrontFaceDirection.Cw);
			GL.Enable (EnableCap.DepthTest);
			GL.DepthMask (true);
			GL.DepthFunc (DepthFunction.Less);
			_sceneGraph.Traverse<Mesh<Vertex>, LineSegment<PathNode, Vec3>> 
			(
				(mesh, mat) =>
				{
					using ( _program.Scope ())
					{
						Sampler.Bind (!_uniforms.samplers, mesh.Textures);
						_uniforms.worldMatrix &= mat;
						_uniforms.normalMatrix &= new Mat3 (mat).Inverse.Transposed;
						_program.DrawTriangles (mesh.VertexBuffer, mesh.IndexBuffer);
						Sampler.Unbind (!_uniforms.samplers, mesh.Textures);
					}
				},
				(lines, mat) =>
				{
					using (_passthrough.Scope ())
					{
						_passthrough.DrawLinePath (lines.VertexBuffer);
					}
				},
				_camera.Transform
			);
			SwapBuffers ();
		}

		private void ResizeViewport (Vec2 size)
		{
			using (_program.Scope ())
			{
				_camera.Frustrum = new ViewingFrustum (size.X, size.Y, 1f, 100f);
				_uniforms.perspectiveMatrix &= _camera.PerspectiveTransform;
				GL.Viewport (ClientSize);
			}
		}

		private void RotateView (Vec3 rot)
		{
			foreach (var pos in _positions)
				pos.Orientation += rot;
		}

		private void ZoomView (float delta)
		{
			foreach (var pos in _positions)
				pos.Offset = pos.Offset.With (2, Math.Min (pos.Offset.Z + delta, 2f));
		}

		#endregion

		[STAThread]
		static void Main (string[] args)
		{
			var wnd = new TestWindow ();
			Console.WriteLine (GL.GetString (StringName.Version));
			wnd.Init ();
			wnd.Run ();
//			Tester.RunTestsTimed (
//				new VecTests (),
//				new MatTests (),
//				new QuatTests ());
//			//new PerformanceTests ());
//			Console.ReadLine ();
		}
	}
}