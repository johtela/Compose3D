﻿namespace ComposeTester
{
	using Compose3D;
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.GLTypes;
	using Compose3D.Reactive;
	using Compose3D.SceneGraph;
	using Compose3D.Shaders;
	using Compose3D.Textures;
	using OpenTK;
	using OpenTK.Graphics;
	using OpenTK.Graphics.OpenGL;
	using OpenTK.Input;
	using System;
	using System.Linq;
	using LinqCheck;
	using Extensions;

	public class TestWindow : GameWindow
	{
		// OpenGL objects
		private Program _program;
		private	Program _passthrough;
		private Program _shadowShader;
		private ExampleShaders.Uniforms _uniforms;
		private ExampleShaders.ShadowUniforms _shadowUniforms;

		// Scene graph
		private SceneGraph _sceneGraph;
		private TransformNode[] _positions;
		private Camera _camera;
		private DirectionalLight _dirLight;

		public TestWindow ()
			: base (800, 600, GraphicsMode.Default, "Compose3D")
		{
			_sceneGraph = CreateSceneGraph ();
			_positions = _sceneGraph.Root.Traverse ().OfType<TransformNode> ().ToArray ();

			_program = new Program (ExampleShaders.VertexShader (), ExampleShaders.FragmentShader ());
			_program.InitializeUniforms (_uniforms = new ExampleShaders.Uniforms ());

			_shadowShader = new Program (ExampleShaders.ShadowVertexShader (), ExampleShaders.ShadowFragmentShader ());
			_shadowShader.InitializeUniforms (_shadowUniforms = new ExampleShaders.ShadowUniforms ());

			_passthrough = new Program (
				GLShader.Create (ShaderType.VertexShader, 
					() =>
					from v in Shader.Inputs<PathNode> ()
					select new ColoredFragment ()
					{
						gl_Position = new Vec4 (v.position.X, v.position.Y, -1f, 1f),
						vertexDiffuse = v.color
					} 
				),
				GLShader.Create (ShaderType.FragmentShader, 
					() =>
					from f in Shader.Inputs<ColoredFragment> ()
					select new 
					{
						outputColor = f.vertexDiffuse
					}
				)
			);
		}

		public void Init ()
		{
			InitializeUniforms ();
			SetupReactions ();
		}

		#region Setup

		private SceneGraph CreateSceneGraph ()
		{
			var sceneGraph = new SceneGraph ();
			_dirLight = new DirectionalLight (sceneGraph,
				intensity: new Vec3 (0.2f), 
				direction: new Vec3 (1f, 1f, 1f),
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

			var textTexture = Texture.FromBitmap (
				"This is a test".TextToBitmapCentered (1024, 1024, 100),
				new TextureParams ()
				{
					{ TextureParameterName.TextureBaseLevel, 0 },
					{ TextureParameterName.TextureMaxLevel, 0 }
				});
			//			var geometry = Solids.Cube<Vertex> (30f, 30f, 1f).Color (VertexColor<Vec3>.Chrome);
			//			geometry.ApplyTextureFront<Vertex> (1f, new Vec2 (0f), new Vec2 (1f));
			//			var mesh1 = new Mesh<Vertex> (geometry, tulipTexture)
			//				.OffsetOrientAndScale (new Vec3 (15f, 0f, -20f), new Vec3 (0f), new Vec3 (1f));

			//			var plasticTexture = Texture.FromFile ("Textures/Plastic.jpg", new TextureParams () 
			//			{
			//				{ TextureParameterName.TextureBaseLevel, 0 },
			//				{ TextureParameterName.TextureMaxLevel, 0 }
			//			});

			var fighter = new FighterGeometry<Vertex, PathNode> ();
			var mesh1 = new Mesh<Vertex> (sceneGraph, fighter.Fighter).Offset (new Vec3 (0f, 0f, -10f));

			var pipe = Geometries.Pipe ().Color (VertexColor<Vec3>.Brass);
			pipe.ApplyTextureTop (1f, new Vec2 (0f), new Vec2 (1f));
			var mesh2 = new Mesh<Vertex> (sceneGraph, pipe, textTexture)
				.OffsetOrientAndScale (new Vec3 (-10f, 0f, -10f), new Vec3 (0f), new Vec3 (0.2f));

			_camera = new Camera (sceneGraph,
				position: new Vec3 (10f, 10f, 10f), 
				target: new Vec3 (0f, 0f, 0f), 
				upDirection: new Vec3 (0f, 1f, 0f),
				frustum: new ViewingFrustum (FrustumKind.Perspective, 1f, 1f, 1f, 100f),
				aspectRatio: 1f);
			sceneGraph.Root.Add (new GlobalLighting (sceneGraph,
				ambientLightIntensity: new Vec3 (0.1f), 
				maxIntensity: 2f, 
				gammaCorrection: 1.2f),
				_dirLight, pointLight1, pointLight2, _camera, mesh1, mesh2);
			return sceneGraph;
		}

		private void InitializeUniforms ()
		{
			var numPointLights = 0;
			var pointLights = new Lighting.PointLight[4];

			using (_program.Scope ())
			{
				_sceneGraph.Root.Traverse ()
					.WhenOfType<SceneNode, GlobalLighting> (globalLight =>
						_uniforms.globalLighting &= new Lighting.GlobalLight ()
						{
							ambientLightIntensity = globalLight.AmbientLightIntensity,
							maxintensity = globalLight.MaxIntensity,
							inverseGamma = 1f / globalLight.GammaCorrection
						})
					.WhenOfType<SceneNode, DirectionalLight> (dirLight =>
						_uniforms.directionalLight &= new Lighting.DirectionalLight ()
						{
							direction = dirLight.Direction,
							intensity = dirLight.Intensity
						})
					.WhenOfType<SceneNode, PointLight> (pointLight =>
						pointLights[numPointLights++] = new Lighting.PointLight
						{
							position = pointLight.Position,
							intensity = pointLight.Intensity,
							linearAttenuation = pointLight.LinearAttenuation,
							quadraticAttenuation = pointLight.QuadraticAttenuation
						})
					.ToVoid ();
				_uniforms.pointLights &= pointLights;

				var samplers = new Sampler2D[4];
				for (int i = 0; i < samplers.Length; i++)
					samplers [i] = new Sampler2D (i, new SamplerParams () 
						{
							{ SamplerParameterName.TextureMagFilter, All.Linear },
							{ SamplerParameterName.TextureMinFilter, All.Linear },
							{ SamplerParameterName.TextureWrapR, All.ClampToEdge },
							{ SamplerParameterName.TextureWrapS, All.ClampToEdge }
						});
				_uniforms.samplers &= samplers;
			}
		}

		private void SetupReactions ()
		{
			React.By<double> (Render)
				.WhenRendered (this);

			React.By<Vec2> (ResizeViewport)
				.WhenResized (this);

			React.By<Vec3> (RotateView)
				.Map<MouseMoveEventArgs, Vec3> (e =>
					new Vec3 (e.YDelta.Radians () / 2f, e.XDelta.Radians () / 2f, 0f))
				.Filter (e => e.Mouse.IsButtonDown (MouseButton.Left))
				.WhenMouseMovesOn (this);

			React.By<float> (ZoomView)
				.Map (delta => delta * -0.2f)
				.WhenMouseWheelDeltaChangesOn (this);
		}

		#endregion

		#region Update operations

		private void Render (double time)
		{
			GL.ClearColor (new Color4 (0, 50, 150, 255));
			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			GL.Enable (EnableCap.CullFace);
			GL.CullFace (CullFaceMode.Back);
			GL.FrontFace (FrontFaceDirection.Cw);
			GL.Enable (EnableCap.DepthTest);
			GL.DepthMask (true);
			GL.DepthFunc (DepthFunction.Less);
			var cameraBox = _camera.BoundingBox;
			using ( _program.Scope ())
				_sceneGraph.Index.Overlap (cameraBox).Values ().OfType <Mesh<Vertex>> ()
					.ForEach (mesh =>
					{
						Sampler.Bind (!_uniforms.samplers, mesh.Textures);
						_uniforms.worldMatrix &= _camera.WorldToCamera * mesh.Transform;
						_uniforms.normalMatrix &= new Mat3 (mesh.Transform).Inverse.Transposed;
						_program.DrawTriangles (mesh.VertexBuffer, mesh.IndexBuffer);
						Sampler.Unbind (!_uniforms.samplers, mesh.Textures);
					});
			using (_passthrough.Scope ())
				_sceneGraph.Root.Traverse ().OfType <LineSegment<PathNode, Vec3>> ().ForEach (
					lines => _passthrough.DrawLinePath (lines.VertexBuffer));
			SwapBuffers ();
		}

		private void ResizeViewport (Vec2 size)
		{
			using (_program.Scope ())
			{
				_camera.Frustum = new ViewingFrustum (FrustumKind.Perspective, size.X, size.Y, 1f, 50f);
				_uniforms.perspectiveMatrix &= _camera.Frustum.CameraToScreen;
				GL.Viewport (ClientSize);
			}
		}

		private void RotateView (Vec3 rot)
		{
			foreach (var pos in _positions)
				pos.Orientation += rot;
		}

		private void ZoomView (float delta)
		{
			foreach (var pos in _positions)
				pos.Offset = pos.Offset.With (2, Math.Min (pos.Offset.Z + delta, 2f));
		}

		#endregion
	}
}