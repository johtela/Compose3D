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
			_zoom = 20f;
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

			var frontFace = Polygon<MaterialVertex>.FromPath (
				Path<PathNode, Vec3>.FromRectangle (28f, 8f).Subdivide (4));
			var brick = frontFace.ExtrudeToScale (
				depth: 1f,
				targetScale: 1.1f,
				steepness: 2f,
				numSteps: 5,
				includeFrontFace: true,
				includeBackFace: false);

			brick.Vertices.Color (new Vec3 (0.7f, 0.1f, 0f));
			brick = brick.ManipulateVertices (
				Manipulators.JigglePosition<MaterialVertex> (0.3f).Compose (
					Manipulators.JiggleColor<MaterialVertex> (0.1f))
				.Where (v => v.position.Z >= 0f), true)
				.Smoothen (0.8f);

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