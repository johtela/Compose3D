namespace ComposeTester
{
	using Compose3D.Maths;
	using Compose3D.GLTypes;
	using Compose3D.Reactive;
	using Compose3D.SceneGraph;
	using OpenTK;
	using OpenTK.Graphics;
	using OpenTK.Graphics.OpenGL;
	using OpenTK.Input;
	using System.Linq;
	using Extensions;

	public class TestWindow : GameWindow
	{
		// OpenGL objects
		private Program _shadowShader;
		private ExampleShaders.ShadowUniforms _shadowUniforms;

		// Renderers
		private Terrain _terrain;
		private Entities _entities;

		// Scene graph
		private SceneGraph _sceneGraph;
		private Camera _camera;
		private TransformNode _cameraTransform;
		private DirectionalLight _dirLight;

		public TestWindow ()
			: base (800, 600, GraphicsMode.Default, "Compose3D")
		{
			_terrain = new Terrain ();
			_entities = new Entities ();
			_sceneGraph = CreateSceneGraph ();

			_shadowShader = new Program (ExampleShaders.ShadowVertexShader (), ExampleShaders.ShadowFragmentShader ());
			_shadowShader.InitializeUniforms (_shadowUniforms = new ExampleShaders.ShadowUniforms ());
		}

		public void Init ()
		{
			_terrain.Uniforms.Initialize (_terrain.TerrainShader, _sceneGraph);
			_entities.Uniforms.Initialize (_entities.EntityShader, _sceneGraph);
			SetupReactions ();
		}

		private SceneGraph CreateSceneGraph ()
		{
			var sceneGraph = new SceneGraph ();
			_dirLight = new DirectionalLight (sceneGraph,
				intensity: new Vec3 (1f), 
				direction: new Vec3 (1f, 1f, -1f),
				distance: 100f);
			var pointLight1 = new PointLight (sceneGraph,
				intensity: new Vec3 (2f), 
				position: new Vec3 (10f, 10f, 10f), 
				linearAttenuation: 0.001f, 
				quadraticAttenuation: 0.001f);
			var pointLight2 = new PointLight (sceneGraph,
				intensity: new Vec3 (1f), 
				position: new Vec3 (-10f, 10f, -10f), 
				linearAttenuation: 0.001f, 
				quadraticAttenuation: 0.001f);

			_camera = new Camera (sceneGraph,
				position: new Vec3 (0f, 10f, 10f), 
				target: new Vec3 (0f, 0f, -75f), 
				upDirection: new Vec3 (0f, 1f, 0f),
				frustum: new ViewingFrustum (FrustumKind.Perspective, 1f, 1f, 1f, 75f),
				aspectRatio: 1f);
			_cameraTransform = _camera.Orient (new Vec3(0f));
			
			sceneGraph.Root.Add (new GlobalLighting (sceneGraph,
				ambientLightIntensity: new Vec3 (0.1f), 
				maxIntensity: 2f, 
				gammaCorrection: 1.2f),
				_dirLight, pointLight1, pointLight2, _cameraTransform, _terrain.CreateScene (sceneGraph),
					_entities.CreateScene (sceneGraph));
			return sceneGraph;
		}

		private void SetupReactions ()
		{
			React.By<double> (Render)
				.WhenRendered (this);

			React.By<Vec2> (ResizeViewport)
				.WhenResized (this);

			React.By<Vec3> (RotateCamera)
				.Map<MouseMoveEventArgs, Vec3> (e =>
					new Vec3 (e.YDelta.Radians () / 2f, e.XDelta.Radians () / 2f, 0f))
				.Filter (e => e.Mouse.IsButtonDown (MouseButton.Left))
				.WhenMouseMovesOn (this);

			React.By<float> (ZoomView)
				.Map (delta => delta * -0.2f)
				.WhenMouseWheelDeltaChangesOn (this);
			
			React.By<float> (ZoomView)
				.Map<Key, float> (key => 
					key == Key.W ? 0.5f :
					key == Key.S ? -0.5f :
					0f)
				.WhenKeyDown (this, Key.W, Key.S);

			React.By<Vec3> (RotateCamera)
				.Map<Key, Vec3> (key => 
					key == Key.A ? new Vec3 (0f, -0.01f, 0f) :
					key == Key.D ? new Vec3 (0f, 0.01f, 0f)  :
					new Vec3 (0f))
				.WhenKeyDown (this, Key.A, Key.D);
			}

		private void Render (double time)
		{
			GL.ClearColor (new Color4 (0.2f, 0.4f, 0.6f, 1f));
			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			_terrain.Render (_sceneGraph, _camera);
			_entities.Render (_sceneGraph, _camera);

			using (ExampleShaders.PassThrough.Scope ())
				foreach (var lines in _sceneGraph.Root.Traverse ().OfType <LineSegment<PathNode, Vec3>> ())
					ExampleShaders.PassThrough.DrawLinePath (lines.VertexBuffer);

			SwapBuffers ();
		}

		private void ResizeViewport (Vec2 size)
		{
			_camera.Frustum = new ViewingFrustum (FrustumKind.Perspective, size.X, size.Y, 1f, 200f);
			_terrain.UpdateViewMatrix (_camera.Frustum.CameraToScreen);
			_entities.UpdateViewMatrix (_camera.Frustum.CameraToScreen);
			GL.Viewport (ClientSize);
		}

		private void RotateCamera (Vec3 rot)
		{
			_cameraTransform.Orientation += rot;
		}

		private void ZoomView (float delta)
		{
			_cameraTransform.Offset += new Vec3 (0f, 0f, delta);
		}
	}
}