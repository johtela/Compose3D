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
			_zoom = 100f;
			CreateSceneGraph ();
			SetupRendering ();
			SetupCameraMovement ();
		}

		public static Geometry<V> Brick<V> (float width, float height, Vec3 color, 
			float edgeSharpness)
			where V : struct, IVertex, IDiffuseColor<Vec3>
		{
			return Polygon<V>.FromPath (
				Path<PathNode, Vec3>.FromRectangle (width, height).Subdivide (4))
				.ExtrudeToScale (
				depth: 1f,
				targetScale: 1.1f,
				steepness: edgeSharpness,
				numSteps: 5,
				includeFrontFace: true,
				includeBackFace: false)
				.ColorInPlace (color);
		}

		public static Geometry<V> BrickWall<V> (Geometry<V> brick, float seamWidth, int rows, int cols,
			float offset, Vec3 mortarColor, float maxDimensionError, float maxColorError)
			where V : struct, IVertex, IDiffuseColor<Vec3>
		{
			var size = brick.BoundingBox.Size + new Vec3 (seamWidth, seamWidth, 0f);
			var bricks = Composite.Create (
				             from r in Enumerable.Range (0, rows)
				from c in Enumerable.Range (0, cols)
				let offs = (r & 1) == 1 ? offset : 0f
				select brick.Translate (c * size.X - offs, r * size.Y))
				.Center ()
				.ManipulateVertices (
				             Manipulators.JigglePosition<V> (maxDimensionError).Compose (
					             Manipulators.JiggleColor<V> (maxColorError))
					.Where (v => v.position.Z >= 0f), true);
			var bbox = bricks.BoundingBox;
			var mortar = Quadrilateral<V>.Rectangle (bbox.Size.X, bbox.Size.Y)
				.Translate (0f, 0f, bbox.Back)
				.ColorInPlace (mortarColor)
				.ManipulateVertices<V> (Manipulators.JiggleColor<V> (maxColorError), false);
			return Composite.Create (bricks, mortar)
				.Smoothen (0.8f);
		}

		private void CreateSceneGraph ()
		{
			var sceneGraph = new SceneGraph ();

			_camera = new Camera (sceneGraph,
				position: new Vec3 (0f, 0f, 1f),
				target: new Vec3 (0f, 0f, 0f),
				upDirection: new Vec3 (0f, 1f, 0f),
				frustum: new ViewingFrustum (FrustumKind.Perspective, 1f, 1f, -1f, -1000f),
				aspectRatio: 1f);

			var brick = Brick<MaterialVertex> (
	            width: 28f,
	            height: 8f,
	            color: new Vec3 (0.54f, 0.41f, 0.34f),
	            edgeSharpness: 1.5f);
			var brickWall = BrickWall<MaterialVertex> (
				brick: brick, 
				seamWidth: 2f, 
				rows: 10, 
				cols: 5, 
				offset: 10f,
				mortarColor: new Vec3 (0.52f, 0.5f, 0.45f),
				maxDimensionError: 0.3f,
				maxColorError: 0.05f);
			
			_mesh = new Mesh<MaterialVertex> (sceneGraph, brickWall);
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
					(_camera.Frustum = new ViewingFrustum (FrustumKind.Perspective, size.X, size.Y, -1f, -1000f))
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