﻿namespace ComposeTester
{
	using System;
	using System.Linq;
	using Extensions;
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
		// Scene graph
		private Camera _camera;
		private Mesh<MaterialVertex> _mesh;
		private Vec2 _rotation;
		private float _zoom;
		private SceneGraph _sceneGraph;
		private Texture _signalTexture;
		private DelayedReactionUpdater _updater;

		public MaterialWindow ()
			: base (800, 600, GraphicsMode.Default, "Compose3D", GameWindowFlags.Default, 
				DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
		{
			_rotation = new Vec2 ();
			_zoom = 200f;
			_updater = new DelayedReactionUpdater (this);
			CreateSceneGraph ();
			SetupRendering ();
			SetupCameraMovement ();
		}

		private Control SignalTextureUI ()
		{
			var size = new Vec2i (256);

			//var sine = new Signal<Vec2, float> (v => v.X.Sin () * v.Y.Sin ())
			//	.MapInput ((Vec2 v) => v * MathHelper.Pi * 4f)
			//	.ToSignalEditor ("sine");
			//var worley = SignalEditor.Worley ("worley", WorleyNoiseKind.F1, ControlPointKind.Random, 
			//	10, 0, DistanceFunctionKind.Euclidean, 0f, true);
			//var transform = worley.Transform ("transform", -30f, 0.5f);
			//var dv = new Vec2 (1f) / new Vec2 (size.X, size.Y);
			//var perlin = SignalEditor.Perlin ("perlin", 0, new Vec2 (10f), false);
			//var spectral = perlin.SpectralControl ("spectral", 0, 2, 1f, 0.5f, 0.2f);
			//var warp = transform.Warp ("warp", spectral, 0.001f, dv);
			//var signal = warp.Colorize ("signal", ColorMap<Vec3>.GrayScale ());
			//var normal = warp.NormalMap ("normal", 1f, dv);

			var worley = SignalEditor.Worley ("worley", WorleyNoiseKind.F1, ControlPointKind.Random, 10, 0, DistanceFunctionKind.Euclidean, 0f, true);
			var perlin = SignalEditor.Perlin ("perlin", 0, new Vec2 (10f, 10f), false);
			var transform = worley.Transform ("transform", -30f, 0.5f);
			var spectral = perlin.SpectralControl ("spectral", 0, 2, 1f, 0.5f, 0.2f);
			var warp = transform.Warp ("warp", spectral, 0.001f, new Vec2 (0.00390625f, 0.00390625f));
			var normal = warp.NormalMap ("normal", 1f, new Vec2 (0.00390625f, 0.00390625f));
			var signal = warp.Colorize ("signal", new ColorMap<Vec3> { { -1f, new Vec3 (0f, 0f, 0f) }, { 0.1f, new Vec3 (1f, 1f, 1f) } });

			return SignalEditor.EditorTree (_signalTexture, size, _updater, normal, signal);
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

			var mesh = Quadrilateral<MaterialVertex>.Rectangle (100f, 100f);

			_signalTexture = new Texture (TextureTarget.Texture2D);
			var infoWindow = ControlPanel<TexturedVertex>.Movable (_sceneGraph, SignalTextureUI (), 
				new Vec2i (600, 600), new Vec2 (-1f, 1f));
			var textureWindow = Panel<TexturedVertex>.Movable (_sceneGraph, false, _signalTexture, 
				new Vec2 (0.25f, 0.75f), new Vec2i (2));

			_mesh = new Mesh<MaterialVertex> (_sceneGraph, mesh);
			_sceneGraph.Root.Add (_camera, _mesh.Scale (new Vec3 (10f)), 
				infoWindow, textureWindow);
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