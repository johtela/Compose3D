namespace ComposeTester
{
	using System;
	using System.Linq;
	using System.Drawing;
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.Reactive;
	using Compose3D.SceneGraph;
	using Compose3D.Textures;
	using OpenTK;
	using OpenTK.Graphics;
	using OpenTK.Graphics.OpenGL;
	using OpenTK.Input;

	public class TestWindow : GameWindow
	{
		// Renderers
		private Skybox _skybox;
		private Shadows _shadows;
		private Terrain _terrain;
		private Entities _entities;
		private Windows _windows;

		// Scene graph
		private SceneGraph _sceneGraph;
		private Terrain.Scene _terrainScene;
		private Camera _camera;
		private DirectionalLight _dirLight;
		private Vec3 _rotation;
		private TransformNode _fighter;
		private Window<WindowVertex> _infoWindow;
		private Window<WindowVertex> _shadowWindow;
		private int _fpsCount;
		private double _fpsTime;
		
		private readonly Vec3 _skyColor = new Vec3 (0.84f, 0.79f, 0.69f);

		public TestWindow ()
			: base (800, 600, GraphicsMode.Default, "Compose3D")
		{
			_sceneGraph = CreateSceneGraph ();
			_shadows = new Shadows (_sceneGraph, 4096, ShadowMapType.Variance);
			_skybox = new Skybox (_sceneGraph);
			_terrain = new Terrain (_sceneGraph, _skyColor);
			_entities = new Entities (_sceneGraph);
			_windows = new Windows ();
			//AddShadowWindow ();
			SetupReactions ();
		}

		private SceneGraph CreateSceneGraph ()
		{
			var sceneGraph = new SceneGraph ();
			_dirLight = new DirectionalLight (sceneGraph,
				intensity: new Vec3 (10f), 
				direction: new Vec3 (0.7f, 1f, -0.7f),
				maxShadowDepth: 100f);

			_camera = new Camera (sceneGraph,
				position: new Vec3 (0f, 10f, 10f), 
				target: new Vec3 (0f, 10f, -1f), 
				upDirection: new Vec3 (0f, 1f, 0f),
				frustum: new ViewingFrustum (FrustumKind.Perspective, 1f, 1f, 1f, 400f),
				aspectRatio: 1f);

			sceneGraph.GlobalLighting = new GlobalLighting ()
			{
				AmbientLightIntensity = new Vec3 (1f),
				MaxIntensity = 4.5f,
				GammaCorrection = 1.4f,
			};
			
			_terrainScene = new Terrain.Scene (sceneGraph);
			_fighter = Entities.CreateScene (sceneGraph);

			_infoWindow = new Window<WindowVertex> (sceneGraph, true, Texture.FromBitmap (InfoWindow (0)));
			sceneGraph.Root.Add (_dirLight, _camera, _terrainScene.Root, _fighter, 
				_infoWindow.Offset (new Vec3 (-0.95f, 0.95f, 0f)));
			return sceneGraph;
		}

		private void AddShadowWindow ()
		{
			_shadowWindow = new Window<WindowVertex> (_sceneGraph, false, _shadows.DepthTexture);
			_sceneGraph.Root.Add (_shadowWindow.Offset (new Vec3 (0.5f, 0.95f, 0f))); 
		}

		private Bitmap InfoWindow (int fps)
		{
			return string.Format ("FPS: {0}", fps).TextToBitmapAligned (256, 128, 16f, 
				StringAlignment.Near, StringAlignment.Near);
		}

		private void SetupReactions ()
		{
			React.Propagate (
				React.By<double> (Render),
				React.By<float> (MoveFighter)
					.Aggregate<double, float> ((s, t) => s + (float)t * 25f, 0f))
				.WhenRendered (this)
				.Evoke ();

			React.By<Vec2> (ResizeViewport)
				.WhenResized (this)
				.Evoke ();

			React.By<Vec3> (RotateCamera)
				.Select<MouseMoveEventArgs, Vec3> (e =>
					new Vec3 (-e.YDelta.Radians () / 2f, -e.XDelta.Radians () / 2f, 0f))
				.Where (e => e.Mouse.IsButtonDown (MouseButton.Left))
				.WhenMouseMovesOn (this)
				.Evoke ();

			React.By<float> (MoveCamera)
				.Select (delta => delta * -0.2f)
				.WhenMouseWheelDeltaChangesOn (this)
				.Evoke ();
			
			React.By<float> (MoveCamera)
				.Select<Key, float> (key => key == Key.W ? 1f : -1f)
				.WhenKeyDown (this, Key.W, Key.S)
				.Evoke ();

			React.By<Vec3> (RotateCamera)
				.Select<Key, Vec3> (key => key == Key.A ? 
					new Vec3 (0f, 0.01f, 0f) : 
					new Vec3 (0f, -0.01f, 0f))
				.WhenKeyDown (this, Key.A, Key.D)
				.Evoke ();
		}

		private void Render (double time)
		{
			var meshes = _camera.NodesInView<Mesh<EntityVertex>> ().ToArray ();
			_shadows.Render (_camera, meshes);

			GL.Viewport (ClientSize);
			GL.ClearColor (_skyColor.X, _skyColor.Y, _skyColor.Z, 1f);
			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			
			_fpsTime += time;
			if (++_fpsCount == 10)
			{
				_infoWindow.Texture.UpdateBitmap (InfoWindow ((int)Math.Round (10.0 / _fpsTime)), 
					TextureTarget.Texture2D, 0);
				_fpsCount = 0;
				_fpsTime = 0.0;
			}
			_skybox.Render (_camera);
			_terrain.Render (_camera);
			_entities.Render (_camera, meshes);
			_windows.Render (_sceneGraph, new Vec2 (Width, Height));

			SwapBuffers ();
		}

		private void ResizeViewport (Vec2 size)
		{
			_camera.Frustum = new ViewingFrustum (FrustumKind.Perspective, size.X, size.Y, 1f, 400f);
			var viewMatrix = _camera.Frustum.CameraToScreen;
			_skybox.UpdateViewMatrix (viewMatrix);
			_terrain.UpdateViewMatrix (viewMatrix);
			_entities.UpdateViewMatrix (viewMatrix);
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
			var terrainHeight = _terrainScene.Height (cameraPos);
			_camera.Position = cameraPos.Y >= terrainHeight ? cameraPos :
				new Vec3 (cameraPos.X, terrainHeight, cameraPos.Z);
			_camera.Target = _camera.Position + lookVec;
		}

		private void MoveFighter (float x)
		{
			_fighter.Offset = new Vec3 (0f,
				Math.Max (_terrainScene.Height (_fighter.Offset) + 20f, _fighter.Offset.Y), x - 5000f);
			var angle = x * 0.03f;
			_fighter.Orientation = new Vec3 (0f, 0f, GLMath.Cos (angle));
			var rotation = Mat.RotationY<Mat4> (angle * 0.233f);
			_camera.Position = _fighter.Offset + rotation.Transform (new Vec3 (50f * GLMath.Cos (angle * 0.177f), 5f, 0f));
			_camera.Target = _fighter.Offset;
		}
	}
} 