namespace ComposeTester
{
	using System;
	using System.Linq;
	using Compose3D.Maths;
	using Compose3D.GLTypes;
	using Compose3D.Reactive;
	using Compose3D.SceneGraph;
	using OpenTK;
	using OpenTK.Graphics;
	using OpenTK.Graphics.OpenGL;
	using OpenTK.Input;
	using Extensions;

	public class TestWindow : GameWindow
	{
		//// OpenGL objects
		//private Program _shadowShader;
		//private ExampleShaders.ShadowUniforms _shadowUniforms;

		// Renderers
		private Skybox _skybox;
		private Terrain _terrain;
		private Entities _entities;

		// Scene graph
		private SceneGraph _sceneGraph;
		private Camera _camera;
		private DirectionalLight _dirLight;
		private Vec3 _rotation;
		private TransformNode _fighter;

		private readonly Vec3 _skyColor = new Vec3 (0.84f, 0.79f, 0.69f);

		public TestWindow ()
			: base (800, 600, GraphicsMode.Default, "Compose3D")
		{
			_skybox = new Skybox ();
			_terrain = new Terrain ();
			_entities = new Entities ();
			_sceneGraph = CreateSceneGraph ();
			_skybox.Uniforms.Initialize (_skybox.SkyboxShader);
			_terrain.Uniforms.Initialize (_terrain.TerrainShader, _sceneGraph, _skyColor);
			_entities.Uniforms.Initialize (_entities.EntityShader, _sceneGraph);
			SetupReactions ();

			//_shadowShader = new Program (ExampleShaders.ShadowVertexShader (), ExampleShaders.ShadowFragmentShader ());
			//_shadowShader.InitializeUniforms (_shadowUniforms = new ExampleShaders.ShadowUniforms ());
		}

		private SceneGraph CreateSceneGraph ()
		{
			var sceneGraph = new SceneGraph ();
			_dirLight = new DirectionalLight (sceneGraph,
				intensity: new Vec3 (10f), 
				direction: new Vec3 (0.75f, 0.75f, -1f),
				distance: 100f);
			//var pointLight1 = new PointLight (sceneGraph,
			//	intensity: new Vec3 (2f),
			//	position: new Vec3 (100f, 100f, -100f),
			//	linearAttenuation: 0.00001f,
			//	quadraticAttenuation: 0.00001f);

			_camera = new Camera (sceneGraph,
				position: new Vec3 (0f, 10f, 10f), 
				target: new Vec3 (0f, 10f, -1f), 
				upDirection: new Vec3 (0f, 1f, 0f),
				frustum: new ViewingFrustum (FrustumKind.Perspective, 1f, 1f, 1f, 400f),
				aspectRatio: 1f);

			sceneGraph.GlobalLighting = new GlobalLighting ()
			{
				AmbientLightIntensity = new Vec3 (1f),
				MaxIntensity = 5f,
				GammaCorrection = 1.4f,
				DiffuseMap = _skybox.DiffuseMap,
				EnvironmentMap = _skybox.EnvironmentMap
			};
			_fighter = _entities.CreateScene (sceneGraph);
			sceneGraph.Root.Add (_dirLight, _camera, _terrain.CreateScene (sceneGraph), _fighter);
			return sceneGraph;
		}

		private void SetupReactions ()
		{
			React.Propagate (
				React.By<double> (Render),
				React.By<float> (MoveFighter)
					.Reduce<double, float> ((s, t) => s + (float)t * 25f, 0f))
				.WhenRendered (this);

			React.By<Vec2> (ResizeViewport)
				.WhenResized (this);

			React.By<Vec3> (RotateCamera)
				.Map<MouseMoveEventArgs, Vec3> (e =>
					new Vec3 (-e.YDelta.Radians () / 2f, -e.XDelta.Radians () / 2f, 0f))
				.Filter (e => e.Mouse.IsButtonDown (MouseButton.Left))
				.WhenMouseMovesOn (this);

			React.By<float> (MoveCamera)
				.Map (delta => delta * -0.2f)
				.WhenMouseWheelDeltaChangesOn (this);
			
			React.By<float> (MoveCamera)
				.Map<Key, float> (key => key == Key.W ? 1f : -1f)
				.WhenKeyDown (this, Key.W, Key.S);

			React.By<Vec3> (RotateCamera)
				.Map<Key, Vec3> (key => key == Key.A ? 
					new Vec3 (0f, 0.01f, 0f) : 
					new Vec3 (0f, -0.01f, 0f))
				.WhenKeyDown (this, Key.A, Key.D);
		}

		private void Render (double time)
		{
			GL.ClearColor (_skyColor.X, _skyColor.Y, _skyColor.Z, 1f);
			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			_skybox.Render (_camera);
			_terrain.Render (_camera);
			_entities.Render (_camera);

			using (ExampleShaders.PassThrough.Scope ())
				foreach (var lines in _sceneGraph.Root.Traverse ().OfType <LineSegment<PathNode, Vec3>> ())
					ExampleShaders.PassThrough.DrawLinePath (lines.VertexBuffer);

			SwapBuffers ();
		}

		private void ResizeViewport (Vec2 size)
		{
			_camera.Frustum = new ViewingFrustum (FrustumKind.Perspective, size.X, size.Y, 1f, 400f);
			var viewMatrix = _camera.Frustum.CameraToScreen;
			_skybox.UpdateViewMatrix (viewMatrix);
			_terrain.UpdateViewMatrix (viewMatrix);
			_entities.UpdateViewMatrix (viewMatrix);
			GL.Viewport (ClientSize);
		}

		private Vec3 LookVec ()
		{
			return (Mat.RotationX<Mat4> (_rotation.X) * Mat.RotationY<Mat4> (_rotation.Y))
				.Transform (new Vec3 (0f, 0f, -1f));
		}

		private void RotateCamera (Vec3 rot)
		{
			_rotation += rot;
			_camera.Target = _camera.Position + LookVec ();
		}

		private void MoveCamera (float delta)
		{
			var lookVec = LookVec ();
			var cameraPos = _camera.Position + (lookVec * delta);
			var terrainHeight = _terrain.Height (cameraPos);
			_camera.Position = cameraPos.Y >= terrainHeight ? cameraPos :
				new Vec3 (cameraPos.X, terrainHeight, cameraPos.Z);
			_camera.Target = _camera.Position + lookVec;
		}

		private void MoveFighter (float x)
		{
			_fighter.Offset = new Vec3 (x, 
				Math.Max (_terrain.Height (_fighter.Offset) + 20f, _fighter.Offset.Y), 0f);
			var angle = x * 0.02f;
			_fighter.Orientation = new Vec3 (GLMath.Cos (angle), 0f, 0f);
			var rotation = Mat.RotationY<Mat4> (angle * 0.133f);
			_camera.Position = _fighter.Offset + rotation.Transform (new Vec3 (50f * GLMath.Cos (angle * 0.277f), 5f, 0f));
			_camera.Target = _fighter.Offset;
		}
	}
} 