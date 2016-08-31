namespace ComposeTester
{
	using Extensions;
	using System.Linq;
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.Reactive;
	using Compose3D.SceneGraph;
	using Compose3D.Shaders;
	using Compose3D.Renderers;
	using OpenTK;
	using OpenTK.Graphics;
	using OpenTK.Input;

	public class MaterialWindow : GameWindow
	{
		// Scene graph
		private SceneGraph _sceneGraph;
		private Camera _camera;
		private DirectionalLight _dirLight;
		private Vec2 _rotation;
		private float _zoom;
		
		public MaterialWindow ()
			: base (800, 600, GraphicsMode.Default, "Compose3D", GameWindowFlags.Default, 
				DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
		{
			_rotation = new Vec2 ();
			_zoom = 20f;
			_sceneGraph = CreateSceneGraph ();
			SetupRendering ();
			SetupCameraMovement ();
		}

		private SceneGraph CreateSceneGraph ()
		{
			var sceneGraph = new SceneGraph ();
			_dirLight = new DirectionalLight (sceneGraph,
				intensity: new Vec3 (1f), 
				direction: new Vec3 (0.7f, 1f, -0.7f),
				maxShadowDepth: 200f);

			_camera = new Camera (sceneGraph,
				position: new Vec3 (0f, 10f, 10f), 
				target: new Vec3 (0f, 10f, -1f), 
				upDirection: new Vec3 (0f, 1f, 0f),
				frustum: new ViewingFrustum (FrustumKind.Perspective, 1f, 1f, -1f, -400f),
				aspectRatio: 1f);

			sceneGraph.GlobalLighting = new GlobalLighting ()
			{
				AmbientLightIntensity = new Vec3 (0.1f),
				MaxIntensity = 1f,
				GammaCorrection = 1.8f,
			};

			var rect = Path<PathNode, Vec3>.FromRectangle (0.5f, 0.5f).Subdivide (1);

			rect.Nodes.Color (EnumerableExt.Generate (() => VertexColor<Vec3>.Random.diffuse));
			var ls = new LineSegment<PathNode, Vec3> (sceneGraph, rect);

			sceneGraph.Root.Add (_dirLight, _camera, ls);
			return sceneGraph;
		}

		private void SetupRendering ()
		{
			LineSegments.Renderer (_sceneGraph)
				.Viewport (this)
				.SwapBuffers (this).Select ((double _) => _camera)
				.WhenRendered (this).Evoke ();
		}

		private void SetupCameraMovement ()
		{
			React.By<Vec2> (RotateCamera)
				.Select ((MouseMoveEventArgs e) =>
					new Vec2 (-e.XDelta.Radians (), -e.YDelta.Radians ()) * 0.2f)
				.Where (_ => Mouse[MouseButton.Left])
				.WhenMouseMovesOn (this)
				.Evoke ();

			React.By<float> (ZoomCamera)
				.Select (delta => delta * -0.5f)
				.WhenMouseWheelDeltaChangesOn (this)
				.Evoke ();
		}

		private void UpdateControls (Vec2i viewportSize)
		{
			var panels = _sceneGraph.Root.Traverse ().OfType<ControlPanel<TexturedVertex>> ();
			foreach (var panel in panels)
				panel.UpdateControl (viewportSize, Mouse, Keyboard);
		}

		private Vec3 LookVec ()
		{
			return (Quat.FromAxisAngle (Dir3D.Up, _rotation.X) * Quat.FromAxisAngle (Dir3D.Right, _rotation.Y))
				.RotateVec3 (Dir3D.Front) * _zoom;
		}

		private void RotateCamera (Vec2 rot)
		{
			_rotation += rot;
		}

		private void ZoomCamera (float delta)
		{
			_zoom += delta;
		}
	}
}