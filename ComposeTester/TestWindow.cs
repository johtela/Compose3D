namespace ComposeTester
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
		private Program _terrainShader;
		private ExampleShaders.Uniforms _uniforms;
		private ExampleShaders.TerrainUniforms _terrainUniforms;
		private ExampleShaders.ShadowUniforms _shadowUniforms;

		// Scene graph
		private SceneGraph _sceneGraph;
		private TransformNode[] _positions;
		private Camera _camera;
		private TransformNode _cameraTransform;
		private DirectionalLight _dirLight;

		public TestWindow ()
			: base (1366, 768, GraphicsMode.Default, "Compose3D", GameWindowFlags.Fullscreen)
		{
			_sceneGraph = CreateSceneGraph ();
			_positions = (from tx in _sceneGraph.Root.Traverse ().OfType<TransformNode> ()
						  where tx.Node is Mesh<Vertex>
						  select tx).ToArray ();

			_program = new Program (ExampleShaders.VertexShader (), ExampleShaders.FragmentShader ());
			_program.InitializeUniforms (_uniforms = new ExampleShaders.Uniforms ());

			_shadowShader = new Program (ExampleShaders.ShadowVertexShader (), ExampleShaders.ShadowFragmentShader ());
			_shadowShader.InitializeUniforms (_shadowUniforms = new ExampleShaders.ShadowUniforms ());

			_terrainShader = new Program (ExampleShaders.TerrrainVertexShader (), 
				ExampleShaders.TerrainFragmentShader ());
			_terrainShader.InitializeUniforms (_terrainUniforms = new ExampleShaders.TerrainUniforms ());
			
			_passthrough = new Program (
				GLShader.Create (ShaderType.VertexShader, 
					() =>
					from v in Shader.Inputs<PathNode> ()
					select new DiffuseFragment ()
					{
						gl_Position = new Vec4 (v.position.X, v.position.Y, -1f, 1f),
						vertexDiffuse = v.color
					} 
				),
				GLShader.Create (ShaderType.FragmentShader, 
					() =>
					from f in Shader.Inputs<DiffuseFragment> ()
					select new 
					{
						outputColor = f.vertexDiffuse
					}
				)
			);
		}

		public void Init ()
		{
			InitializeUniforms (_program, _uniforms);
			InitializeTerrainUniforms (_terrainShader, _terrainUniforms);
			SetupReactions ();
		}

		#region Setup

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

			var fighter = new FighterGeometry<Vertex, PathNode> ();
			var mesh1 = new Mesh<Vertex> (sceneGraph, fighter.Fighter)
				.OffsetOrientAndScale (new Vec3 (0f, 10f, -10f), new Vec3 (0f, MathHelper.Pi, 0f), new Vec3 (1f));

			var terrain = new SceneGroup (sceneGraph,
					from x in EnumerableExt.Range (0, 1000, 20)
					from y in EnumerableExt.Range (0, 1000, 20)
					select new TerrainMesh<TerrainVertex> (sceneGraph, new Vec2i (x, y), new Vec2i (21, 21)))
				.OffsetOrientAndScale (new Vec3 (-500f, -10f, -500f), new Vec3 (0f), new Vec3 (2f));
			
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
				_dirLight, pointLight1, pointLight2, _cameraTransform, mesh1, terrain);
			return sceneGraph;
		}

		private void InitializeTerrainUniforms (Program program, ExampleShaders.TerrainUniforms uniforms)
		{
			using (program.Scope ())
			{
				_sceneGraph.Root.Traverse ()
					.WhenOfType<SceneNode, GlobalLighting> (globalLight =>
						uniforms.globalLighting &= new Lighting.GlobalLight ()
					{
						ambientLightIntensity = globalLight.AmbientLightIntensity,
						maxintensity = globalLight.MaxIntensity,
						inverseGamma = 1f / globalLight.GammaCorrection
					})
					.WhenOfType<SceneNode, DirectionalLight> (dirLight =>
						uniforms.directionalLight &= new Lighting.DirectionalLight ()
					{
						direction = dirLight.Direction,
						intensity = dirLight.Intensity
					})
					.ToVoid ();
			}
		}
		
		private void InitializeUniforms (Program program, ExampleShaders.Uniforms uniforms)
		{
			InitializeTerrainUniforms (program, uniforms);
			var numPointLights = 0;
			var pointLights = new Lighting.PointLight[4];

			using (program.Scope ())
			{
				_sceneGraph.Root.Traverse ()
					.WhenOfType<SceneNode, PointLight> (pointLight =>
						pointLights [numPointLights++] = new Lighting.PointLight
					{
						position = pointLight.Position,
						intensity = pointLight.Intensity,
						linearAttenuation = pointLight.LinearAttenuation,
						quadraticAttenuation = pointLight.QuadraticAttenuation
					})
					.ToVoid ();

				uniforms.pointLights &= pointLights;

				var samplers = new Sampler2D[4];
				for (int i = 0; i < samplers.Length; i++)
					samplers [i] = new Sampler2D (i, new SamplerParams ()
						{
							{ SamplerParameterName.TextureMagFilter, All.Linear },
							{ SamplerParameterName.TextureMinFilter, All.Linear },
							{ SamplerParameterName.TextureWrapR, All.ClampToEdge },
							{ SamplerParameterName.TextureWrapS, All.ClampToEdge }
						});
				uniforms.samplers &= samplers;
			}
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
					key == Key.W ? 0.2f :
					key == Key.S ? -0.2f :
					0f)
				.Filter (key => key.In (Key.W, Key.S))
				.WhenKeyDown (this);

			React.By<Vec3> (RotateCamera)
				.Map<Key, Vec3> (key => 
					key == Key.A ? new Vec3 (0f, -0.1f, 0f) :
					key == Key.D ? new Vec3 (0f, 0.1f, 0f)  :
					new Vec3 (0f))
				.Filter (key => key.In (Key.A, Key.D))
				.WhenKeyDown (this);
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
			using ( _terrainShader.Scope ())
				foreach (var mesh in _sceneGraph.Index.Overlap (_camera.BoundingBox).Values ()
					.OfType <TerrainMesh<TerrainVertex>> ())
				{
					_terrainUniforms.worldMatrix &= _camera.WorldToCamera * mesh.Transform;
					_terrainUniforms.normalMatrix &= new Mat3 (mesh.Transform).Inverse.Transposed;
					_program.DrawElements (PrimitiveType.TriangleStrip, mesh.VertexBuffer, mesh.IndexBuffer);
					
				}
			using ( _program.Scope ())
				foreach (var mesh in _sceneGraph.Index.Overlap (_camera.BoundingBox).Values ()
					.OfType <Mesh<Vertex>> ())
				{
					Sampler.Bind (!_uniforms.samplers, mesh.Textures);
					_uniforms.worldMatrix &= _camera.WorldToCamera * mesh.Transform;
					_uniforms.normalMatrix &= new Mat3 (mesh.Transform).Inverse.Transposed;
					_program.DrawElements (PrimitiveType.Triangles, mesh.VertexBuffer, mesh.IndexBuffer);
					Sampler.Unbind (!_uniforms.samplers, mesh.Textures);
				}
			using (_passthrough.Scope ())
				foreach (var lines in _sceneGraph.Root.Traverse ().OfType <LineSegment<PathNode, Vec3>> ())
					_passthrough.DrawLinePath (lines.VertexBuffer);

			SwapBuffers ();
		}

		private void ResizeViewport (Vec2 size)
		{
			_camera.Frustum = new ViewingFrustum (FrustumKind.Perspective, size.X, size.Y, 1f, 200f);
			using (_program.Scope ())
				_uniforms.perspectiveMatrix &= _camera.Frustum.CameraToScreen;
			using (_terrainShader.Scope ())
				_terrainUniforms.perspectiveMatrix &= _camera.Frustum.CameraToScreen;
			GL.Viewport (ClientSize);
		}

		private void RotateCamera (Vec3 rot)
		{
			_cameraTransform.Orientation += rot;
		}

		private void ZoomView (float delta)
		{
//			foreach (var pos in _positions)
//				pos.Offset = pos.Offset.With (2, Math.Min (pos.Offset.Z + delta, 2f));
			_cameraTransform.Offset += new Vec3 (0f, 0f, delta);
		}

		#endregion
	}
}