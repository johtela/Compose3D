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
		private ExampleShaders.Uniforms _uniforms;

		// Scene graph
		private SceneNode _sceneGraph;
		private OffsetOrientationScale[] _positions;

		public TestWindow ()
			: base (800, 600, GraphicsMode.Default, "Compose3D")
		{
			_sceneGraph = CreateSceneGraph ();
			_positions = _sceneGraph.SubNodes.OfType<OffsetOrientationScale> ().ToArray ();

			_program = new Program (ExampleShaders.VertexShader (), ExampleShaders.FragmentShader ());
			_program.InitializeUniforms (_uniforms = new ExampleShaders.Uniforms ());

			_passthrough = new Program (GLShader.Create (ShaderType.VertexShader, () =>
				from v in Shader.Inputs<PathNode> ()
				select new Fragment () 
				{
					gl_Position = new Vec4 (v.position, 1f)
				}), 
				FragmentShaders.WhiteOutput ());
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
			var pointLight1 = new PointLight (new Vec3 (2f), new Vec3 (30f, 10f, -30f), 0.001f, 0.001f);
			var pointLight2 = new PointLight (new Vec3 (2f), new Vec3 (-30f, 10f, -30f), 0.001f, 0.001f);

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

			var nose = Lathe<Vertex>.Turn (Geometries.NoseProfile (), Axis.X, new Vec3 (0f), 
				MathHelper.Pi / 20f, 0f, 0f)
				.ManipulateVertices (
					v => v.position.Y < 0f, Manipulators.Scale<Vertex> (1f, 0.6f, 1f));
			
			var fuselage = Polygon<Vertex>.FromVertices (nose.Vertices.Rightmost ().Reverse ()
				.Select (v => v.With (v.position, new Vec3 (1f, 0f, 0f))))
				.Extrude (1f, false);
			var fighter = Composite.Create (Stacking.StackRight (nose, fuselage))
				.RotateY (90f.Radians ())
				.Smoothen (0.8f)
				.Color (VertexColor<Vec3>.Chrome);

			var path = Path<PathNode, Vec3>.FromVecs (
				fighter.Vertices.Backmost ().Facing (FaceDir.Back).Select (
					v => new Vec3 (v.position.X, v.position.Y, 0f)))
				.Close ();
			var lineSeg = new LineSegment<PathNode, Vec3> (path);

			var fuselageXSection = new LineSegment<PathNode, Vec3> (
				Geometries.FuselageCrossSection (path.Nodes.Length)); 
			
			var mesh1 = new Mesh<Vertex> (fighter)
				.OffsetOrientAndScale (new Vec3 (0f, 0f, -20f), new Vec3 (0f), new Vec3 (5f));

			//			var plasticTexture = Texture.FromFile ("Textures/Tulips.jpg", new TextureParams () 
			//			{
			//				{ TextureParameterName.TextureBaseLevel, 0 },
			//				{ TextureParameterName.TextureMaxLevel, 0 }
			//			});
			//			var geometry2 = Geometries.Tube ().Color (VertexColor<Vec3>.Chrome);
			//			geometry2.ApplyTextureFront<Vertex> (0.5f, new Vec2 (0f), new Vec2 (1f));
			//			geometry2.ApplyTextureBack<Vertex> (0.5f, new Vec2 (10f), new Vec2 (11f));
			//			var mesh2 = new Mesh<Vertex> (geometry2, plasticTexture)
			//				.OffsetOrientAndScale (new Vec3 (-15f, 0f, -40f), new Vec3 (0f), new Vec3 (1f));

			return new GlobalLighting (new Vec3 (0.2f), 2f, 1.2f).Add (dirLight, pointLight1, pointLight2, 
				mesh1, lineSeg, fuselageXSection);
		}

		private void InitializeUniforms ()
		{
			var numPointLights = 0;
			var pointLights = new Lighting.PointLight[4];

			using (_program.Scope ())
			{
				_sceneGraph.Traverse<GlobalLighting, DirectionalLight, PointLight> 
				(
					(globalLight, mat, nmat) =>
					_uniforms.globalLighting &= new Lighting.GlobalLight () 
					{
						ambientLightIntensity = globalLight.AmbientLightIntensity,
						maxintensity = globalLight.MaxIntensity,
						inverseGamma = 1f / globalLight.GammaCorrection
					},
					(dirLight, mat, nmat) =>
					_uniforms.directionalLight &= new Lighting.DirectionalLight () 
					{
						direction = dirLight.Direction,
						intensity = dirLight.Intensity
					},
					(pointLight, mat, nmat) =>
					pointLights [numPointLights++] = new Lighting.PointLight 
					{
						position = pointLight.Position,
						intensity = pointLight.Intensity,
						linearAttenuation = pointLight.LinearAttenuation,
						quadraticAttenuation = pointLight.QuadraticAttenuation
					}
				);
				for (int i = numPointLights; i < 4; i++)
				{
					pointLights [i].position = new Vec3 (0f);
					pointLights [i].intensity = new Vec3 (0f);
				}
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
			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			GL.Enable (EnableCap.CullFace);
			GL.CullFace (CullFaceMode.Back);
			GL.FrontFace (FrontFaceDirection.Cw);
			GL.Enable (EnableCap.DepthTest);
			GL.DepthMask (true);
			GL.DepthFunc (DepthFunction.Less);
			_sceneGraph.Traverse<Mesh<Vertex>, LineSegment<PathNode, Vec3>> 
			(
				(mesh, mat, nmat) =>
				{
					using ( _program.Scope ())
					{
						Sampler.Bind (!_uniforms.samplers, mesh.Textures);
						_uniforms.worldMatrix &= mat;
						_uniforms.normalMatrix &= nmat ;
						_program.DrawTriangles (mesh.VertexBuffer, mesh.IndexBuffer);
//						_program.DrawNormals (mesh.NormalBuffer);
						Sampler.Unbind (!_uniforms.samplers, mesh.Textures);
					}
				},
				(lines, mat, nmat) =>
				{
					using (_passthrough.Scope ())
					{
						_passthrough.DrawLinePath (lines.VertexBuffer);
					}
				}
			);
			SwapBuffers ();
		}

		private void ResizeViewport (Vec2 size)
		{
			using (_program.Scope ())
			{
				_uniforms.perspectiveMatrix &= Mat.Scaling<Mat4> (size.Y / size.X, 1f, 1f) *
				Mat.PerspectiveProjection (-1f, 1f, -1f, 1f, 1f, 100f);
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