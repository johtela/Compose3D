namespace ComposeTester
{
	using System.Linq;
	using System.Drawing;
	using Extensions;
	using Visuals;
	using Compose3D.Maths;
	using Compose3D.Imaging;
	using Compose3D.Geometry;
	using Compose3D.Reactive;
	using Compose3D.SceneGraph;
	using Compose3D.Shaders;
	using Compose3D.Renderers;
	using Compose3D.Textures;
	using Compose3D.UI;
	using OpenTK;
	using OpenTK.Graphics;
	using OpenTK.Input;
	using OpenTK.Graphics.OpenGL4;

	public class MaterialWindow : GameWindow
	{
		private class SignalTextureParams
		{
			public int PerlinSeed;
			public float PerlinScale = 10f;
			public float WarpScale = 0.001f;
			public ColorMap<Vec3> ColorMap = new ColorMap<Vec3>
			{
				{ -0.5f, new Vec3 (1f, 0f, 0f) },
				{ 0f, new Vec3 (0f, 1f, 0f) },
				{ 0.5f, new Vec3 (0f, 0f, 1f) }
			};
		}

		// Scene graph
		private Camera _camera;
		private Mesh<MaterialVertex> _mesh;
		private Vec2 _rotation;
		private float _zoom;
		private SceneGraph _sceneGraph;
		private ControlPanel<TexturedVertex> _infoWindow;
		private Texture _signalTexture;
		private DelayedReactionUpdater _updater;

		public MaterialWindow ()
			: base (512, 512, GraphicsMode.Default, "Compose3D", GameWindowFlags.Default, 
				DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
		{
			_rotation = new Vec2 ();
			_zoom = 1000f;
			_updater = new DelayedReactionUpdater (this);
			CreateSceneGraph ();
			SetupRendering ();
			SetupCameraMovement ();
		}

		public static Geometry<V> Brick<V> (float width, float height, float depth, 
			Vec3 color, float edgeSharpness)
			where V : struct, IVertex, IDiffuseColor<Vec3>
		{
			return Polygon<V>.FromPath (
				Path<PathNode, Vec3>.FromRectangle (width, height).Subdivide (4))
				.Scale (edgeSharpness, edgeSharpness)
				.ExtrudeToScale (
					depth: depth,
					targetScale: 1f / edgeSharpness,
					steepness: 2f,
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
					Manipulators.JitterPosition<V> (maxDimensionError).Compose (
						Manipulators.JitterColor<V> (maxColorError))
					.Where (v => v.position.Z >= 0f), true);
			var bbox = bricks.BoundingBox;
			var mortar = Quadrilateral<V>.Rectangle (bbox.Size.X, bbox.Size.Y)
				.Translate (0f, 0f, bbox.Back)
				.ColorInPlace (mortarColor)
				.ManipulateVertices<V> (Manipulators.JitterColor<V> (maxColorError), false);
			return Composite.Create (bricks, mortar)
				.Smoothen (0.5f);
		}

		private void UpdateSignalTexture (SignalTextureParams pars)
		{
			var size = new Vec2i (256);

			var perlin = new Signal<Vec3, float> (new PerlinNoise (pars.PerlinSeed	).Noise)
				.MapInput ((Vec2 v) => new Vec3 (v, 0f) * pars.PerlinScale);
			var sine = new Signal<Vec2, float> (v => v.X.Sin () * v.Y.Sin ())
				.MapInput ((Vec2 v) => v * MathHelper.Pi * 4f);
			var signal = sine.Warp (perlin.Scale (pars.WarpScale), 1f / size.X)
				//.NormalRangeToZeroOne ().FloatToUintGrayscale ();
				.Colorize (pars.ColorMap).Vec3ToUintColor ();
			var buffer = signal.MapInput (Signal.BitmapCoordToUnitRange (size, 1f)).SampleToBuffer (size);
			_signalTexture.LoadArray (buffer, _signalTexture.Target, 0, 256, 256, PixelFormat.Rgba, 
				PixelInternalFormat.Rgb, PixelType.UnsignedInt8888);
		}

		private Control CreateUI ()
		{
			var color = Color.White;
			return new Container (VisualDirection.Vertical, HAlign.Left, VAlign.Center, true,
				FoldableContainer.WithLabel ("Perlin Noise", true, HAlign.Left,
					Container.LabelAndControl ("Seed: ",
						new NumericEdit (_textureParams.Value.PerlinSeed, 1f, React.By ((float s) =>
							_textureParams.Change.PerlinSeed = (int)s)), true),
					Container.LabelAndControl ("Scale: ",
						new NumericEdit (_textureParams.Value.PerlinScale, 1f, React.By ((float s) =>
							_textureParams.Change.PerlinScale = s)), true),
					Container.LabelAndControl ("Warp Scale: ",
						new NumericEdit (_textureParams.Value.WarpScale, 0.001f, React.By ((float s) =>
							_textureParams.Change.WarpScale = s)), true)
				),
				FoldableContainer.WithLabel ("Color Map", true, HAlign.Left,
					new ColorMapEdit (-1f, 1f, 20f, 200f, _textureParams.Value.ColorMap,
							React.By ((ColorMap<Vec3> _) => _textureParams.Changed ()))
				), 
				new Button ("Test", React.Ignore<bool> ()));
		}

		private void CreateSceneGraph ()
		{
			_sceneGraph = new SceneGraph ();

			_camera = new Camera (_sceneGraph,
				position: new Vec3 (0f, 0f, 1f),
				target: new Vec3 (0f, 0f, 0f),
				upDirection: new Vec3 (0f, 1f, 0f),
				frustum: new ViewingFrustum (FrustumKind.Perspective, 1f, 1f, -1f, -10000f),
				aspectRatio: 1f);

			var brick = Brick<MaterialVertex> (
	            width: 28.5f,
	            height: 8.5f,
				depth: 1f,
	            color: new Vec3 (0.54f, 0.41f, 0.34f),
	            edgeSharpness: 0.95f);
			var brickWall = BrickWall<MaterialVertex> (
                brick: brick, 
                seamWidth: 1.5f, 
                rows: 10, 
                cols: 5, 
                offset: 10f,
                mortarColor: new Vec3 (0.52f, 0.5f, 0.45f),
                maxDimensionError: 0.1f,
                maxColorError: 0.05f);

			_infoWindow = new ControlPanel<TexturedVertex> (_sceneGraph, CreateUI (), new Vec2i (300, 400));
			
			_mesh = new Mesh<MaterialVertex> (_sceneGraph, brickWall);
			_sceneGraph.Root.Add (_camera, _mesh.Scale (new Vec3 (10f)), 
				_infoWindow.Offset (new Vec3 (-0.9f, 0.9f, 0f)));

			_signalTexture = new Texture (TextureTarget.Texture2D);
			UpdateSignalTexture (_textureParams.Value);
			var textureWindow = new Panel<TexturedVertex> (_sceneGraph, false, _signalTexture);
			_sceneGraph.Root.Add (textureWindow.Offset (new Vec3 (0.25f, 0.75f, 0f)));
		}

		private void SetupRendering ()
		{
			var renderMaterial = Materials.Renderer ().Select ((double _) => _camera);
			var renderPanel = Panels.Renderer (_sceneGraph)
				.And (React.By ((Vec2i vp) => ControlPanel<TexturedVertex>.UpdateAll (_sceneGraph, this, vp)))
				.Select ((double _) => new Vec2i (ClientSize.Width, ClientSize.Height));

			Render.Clear<double> (new Vec4 (0f, 0f, 0f, 1f), 
					ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit)
				.And (React.By<double> (UpdateCamera))
				.And (renderMaterial)
				.And (renderPanel)
				.Viewport (this)
				.SwapBuffers (this)
				.WhenRendered (this).Evoke ();

			Materials.UpdatePerspectiveMatrix ()
				.Select ((Vec2 size) =>
					(_camera.Frustum = new ViewingFrustum (FrustumKind.Perspective, size.X, size.Y, -1f, -10000f))
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