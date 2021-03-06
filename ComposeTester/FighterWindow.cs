﻿namespace ComposeTester
{
	using System;
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
	using Visuals;
	using Compose3D.UI;
	using System.Drawing;

	public class FighterWindow : GameWindow
	{
		// Scene graph
		private SceneGraph _sceneGraph;
		private Terrain.Scene _terrainScene;
		private Camera _camera;
		private DirectionalLight _dirLight;
		private Vec2 _rotation;
		private float _zoom;
		private TransformNode<Mesh<EntityVertex>> _fighter;
		private ControlPanel<TexturedVertex> _infoWindow;
		private Panel<TexturedVertex> _shadowWindow;
		private int _fpsCount;
		private int _fps;
		private double _fpsTime;
		
		private readonly Vec3 _skyColor = new Vec3 (0.84f, 0.79f, 0.69f);

		public FighterWindow ()
			: base (640, 400, GraphicsMode.Default, "Compose3D", GameWindowFlags.Default, 
				DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
		{
			_rotation = new Vec2 ();
			_zoom = 20f;
			_sceneGraph = CreateSceneGraph ();
			SetupRendering ();
			SetupCameraMovement ();
			//AddShadowWindow ();
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
			
			_terrainScene = new Terrain.Scene (sceneGraph);
			var fighterGeometry = new FighterGeometry<EntityVertex, PathNode> ();
			_fighter = new Mesh<EntityVertex> (sceneGraph, fighterGeometry.Fighter.RotateY (0f).Compact ())
				.OffsetOrientAndScale (new Vec3 (0f, 15f, -10f), new Vec3 (0f, 0f, 0f), new Vec3 (1f));

			_infoWindow = new ControlPanel<TexturedVertex> (sceneGraph,
				Container.Vertical (true, false, 
					Label.Static ("Options", FontStyle.Bold),
					new ListView (React.Ignore <IVisualizable> (),
						new Visualizable (() => Visual.Label (string.Format ("FPS: {0}", _fps))),
						new Visualizable (() => Visual.Label (
							string.Format ("Mouse: {0}", new Vec2i (Mouse.X , Mouse.Y)))))),
				new Vec2i (180, 64), false);
			sceneGraph.Root.Add (_dirLight, _camera, _terrainScene.Root, _fighter, 
				_infoWindow.Offset (new Vec3 (-0.95f, 0.95f, 0f)));
			return sceneGraph;
		}

		private void SetupRendering ()
		{
			var shadowRender= Shadows.Renderer (_sceneGraph, 2500, ShadowMapType.Depth, true)
				.MapInput ((double _) => _camera);

			var skyboxRender = Skybox.Renderer (_sceneGraph, _skyColor);
			var terrainRender = Terrain.Renderer (_sceneGraph, _skyColor, Shadows.Instance.csmUniforms);
			var entityRender = Entities.Renderer (_sceneGraph, Shadows.Instance.csmUniforms);
			var panelRender = Panels.Renderer (_sceneGraph)
				.And (React.By ((Vec2i vp) => ControlPanel<TexturedVertex>.UpdateAll (_sceneGraph, this, vp)))
				.MapInput ((double _) => new Vec2i (ClientSize.Width, ClientSize.Height));

			var moveFighter = React.By<float> (UpdateFighterAndCamera)
				.Aggregate ((float s, double t) => s + (float)t * 25f, 0f);

			React.By<double> (UpdateFPS)
			.And (shadowRender
				.And (skyboxRender
					.And (terrainRender)
					.And (entityRender)
					.MapInput ((double _) => _camera)
				.And (panelRender)
				.Viewport (this)))
			.And (moveFighter)
			.SwapBuffers (this)
			.WhenRendered (this).Evoke ();

			Entities.UpdatePerspectiveMatrix()
			.And (Skybox.UpdatePerspectiveMatrix ())
			.And (Terrain.UpdatePerspectiveMatrix ())
			.MapInput ((Vec2 size) =>
				(_camera.Frustum = new ViewingFrustum (FrustumKind.Perspective, size.X, size.Y, -1f, -400f))
				.CameraToScreen)
			.WhenResized (this).Evoke ();
		}

		private void SetupCameraMovement ()
		{
			React.By<Vec2> (RotateCamera)
				.MapInput ((MouseMoveEventArgs e) =>
					new Vec2 (-e.XDelta.Radians (), -e.YDelta.Radians ()) * 0.2f)
				.Filter (_ => Mouse[MouseButton.Left])
				.WhenMouseMovesOn (this)
				.Evoke ();

			React.By<float> (ZoomCamera)
				.MapInput (delta => delta * -0.5f)
				.WhenMouseWheelDeltaChangesOn (this)
				.Evoke ();
		}

		private void AddShadowWindow ()
		{
			_shadowWindow = new Panel<TexturedVertex> (_sceneGraph, false, false, _sceneGraph.GlobalLighting.ShadowMap);
			_sceneGraph.Root.Add (_shadowWindow.Offset (new Vec3 (0.5f, 0.95f, 0f)));
		}

		private void UpdateFPS (double time)
		{
			_fpsTime += time;
			if (++_fpsCount == 10)
			{
				_fps = (int)Math.Round (10.0 / _fpsTime);
				_fpsCount = 0;
				_fpsTime = 0.0;
			}
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

		private void UpdateFighterAndCamera (float x)
		{
			_fighter.Offset = new Vec3 (0f,
				Math.Max (_terrainScene.Height (_fighter.Offset) + 20f, _fighter.Offset.Y), x - 5000f);
			var angle = x * 0.03f;
			_fighter.Orientation = new Vec3 (0f, 0f, FMath.Cos (angle));
			_camera.Position = _fighter.Offset + LookVec ();
			_camera.Target = _fighter.Offset;
		}
	}
} 