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
	using OpenTK.Graphics.OpenGL4;

	public class MaterialWindow : GameWindow
	{
		// Scene graph
		private Camera _camera;
		private Mesh<MaterialVertex> _mesh;
		private Vec2 _rotation;
		private float _zoom;
		
		public MaterialWindow ()
			: base (800, 600, GraphicsMode.Default, "Compose3D", GameWindowFlags.Default, 
				DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
		{
			_rotation = new Vec2 ();
			_zoom = 2f;
			CreateSceneGraph ();
			SetupRendering ();
			SetupCameraMovement ();
		}

		private void CreateSceneGraph ()
		{
			var sceneGraph = new SceneGraph ();

			_camera = new Camera (sceneGraph,
				position: new Vec3 (0f, 0f, 1f),
				target: new Vec3 (0f, 0f, 0f),
				upDirection: new Vec3 (0f, 1f, 0f),
				frustum: new ViewingFrustum (FrustumKind.Perspective, 1f, 1f, -1f, -100f),
				aspectRatio: 1f);

			var rectF = Path<PathNode, Vec3>.FromRectangle (0.5f, 0.5f).Subdivide (1);
			var rectB = rectF.Translate (0f, 0f, -0.5f);
			var brick = Extrusion.Extrude<MaterialVertex, PathNode> (true, false, rectF, rectB)
				.ManipulateVertices (Manipulators.Jiggle<MaterialVertex> (0.05f), true);

			brick.Vertices.Color (EnumerableExt.Generate (() => VertexColor<Vec3>.Random.diffuse));
			_mesh = new Mesh<MaterialVertex> (sceneGraph, brick);

			sceneGraph.Root.Add (_camera, _mesh);
		}

		private void SetupRendering ()
		{
			Render.Clear<Camera> (new Vec4 (0f, 0f, 0f, 1f), 
					ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit)
				.And (React.By<Camera> (UpdateCamera))
				.And (Materials.Renderer ())
				.Select ((double _) => _camera)
				.Viewport (this)
				.SwapBuffers (this)
				.WhenRendered (this).Evoke ();

			Materials.UpdatePerspectiveMatrix ()
				.Select ((Vec2 size) =>
					(_camera.Frustum = new ViewingFrustum (FrustumKind.Perspective, size.X, size.Y, -1f, -100f))
					.CameraToScreen)
				.WhenResized (this).Evoke ();
		}

		private void SetupCameraMovement ()
		{
			React.By<Vec2> (rot => _rotation += rot)
				.Select ((MouseMoveEventArgs e) =>
					new Vec2 (-e.XDelta.Radians (), -e.YDelta.Radians ()) * 0.2f)
				.Where (_ => Mouse[MouseButton.Left])
				.WhenMouseMovesOn (this)
				.Evoke ();

			React.By<float> (delta => _zoom += delta)
				.Select (delta => delta * -0.5f)
				.WhenMouseWheelDeltaChangesOn (this)
				.Evoke ();
		}

		private void UpdateCamera ()
		{
			var lookDir = (Quat.FromAxisAngle (Dir3D.Up, _rotation.X) * Quat.FromAxisAngle (Dir3D.Right, _rotation.Y))
				.RotateVec3 (Dir3D.Front) * _zoom;
			var meshPos = _mesh.BoundingBox.Center;
			_camera.Position = meshPos + lookDir;
			_camera.Target = meshPos;
		}
	}
}