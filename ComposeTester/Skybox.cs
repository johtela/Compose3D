namespace ComposeTester
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;
	using Extensions;
	using Compose3D.GLTypes;
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.Reactive;
	using Compose3D.SceneGraph;
	using Compose3D.Shaders;
	using Compose3D.Textures;
	using OpenTK.Graphics.OpenGL;

	public class Skybox : Uniforms
	{
		public class SkyboxFragment : Fragment
		{
			public Vec3 texturePos;
		}

		public Uniform<Mat4> worldMatrix;
		public Uniform<Mat4> perspectiveMatrix;
		public Uniform<SamplerCube> cubeMap;

		private Skybox (Program program) : base (program)
		{
			using (program.Scope ())
				cubeMap &= new SamplerCube (0).LinearFiltering ().ClampToEdges (Axes.All);
		}

		private static Program _skyboxShader;
		private static VBO<PositionalVertex> _vertices;
		private static VBO<int> _indices;
		private static Skybox _skybox;
		private static Vec3 _skyColor;
		
		private const float _cubeSize = 20f;
		private static readonly string[] _paths = new string[] 
			{ "sky_right", "sky_left", "sky_top", "sky_bottom", "sky_front", "sky_back" };

		public static Reaction<Camera> Renderer (SceneGraph sceneGraph, Vec3 skyColor)
		{
			_skyboxShader = new Program (VertexShader (), FragmentShader ());
			_skybox = new Skybox (_skyboxShader);
			_skyColor = skyColor;
			var cube = Extrusion.Cube<PositionalVertex> (_cubeSize, _cubeSize, _cubeSize).Center ();
			_vertices = new VBO<PositionalVertex> (cube.Vertices, BufferTarget.ArrayBuffer);
			_indices = new VBO<int> (cube.Indices, BufferTarget.ElementArrayBuffer);
			var environmentMap = Texture.CubeMapFromFiles (
				_paths.Map (s => string.Format (@"Textures/{0}.bmp", s)), 0)
				.LinearFiltering ().ClampToEdges (Axes.All);
			sceneGraph.GlobalLighting.DiffuseMap = environmentMap;
			
			return React.By<Camera> (_skybox.Render)
				.BindSamplers (new Dictionary<Sampler, Texture> () 
				{
					{ !_skybox.cubeMap, environmentMap } 
				})
				.Culling (CullFaceMode.Front)
				.Program (_skyboxShader);
		}

		private void Render (Camera camera)
		{
			GL.ClearColor (_skyColor.X, _skyColor.Y, _skyColor.Z, 1f);
			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			
			worldMatrix &= camera.WorldToCamera.RemoveTranslation ();
			_skyboxShader.DrawElements (BeginMode.Triangles, _vertices, _indices);
		}

		public static Reaction<Mat4> UpdatePerspectiveMatrix ()
		{
			return React.By<Mat4> (matrix => _skybox.perspectiveMatrix &= matrix)
				.Program (_skyboxShader);
		}

		private static GLShader VertexShader ()
		{
			return GLShader.Create (ShaderType.VertexShader, () =>
				from v in Shader.Inputs<PositionalVertex> ()
				from u in Shader.Uniforms<Skybox> ()
				select new SkyboxFragment ()
				{
					gl_Position = !u.perspectiveMatrix * !u.worldMatrix * new Vec4 (v.position, 1f),
					texturePos = v.position + new Vec3 (0f, 0.5f, 0f)
				});
		}

		private static GLShader FragmentShader ()
		{
			return GLShader.Create (ShaderType.FragmentShader, () =>
				from f in Shader.Inputs<SkyboxFragment> ()
				from u in Shader.Uniforms<Skybox> ()
				select new
				{
					outputColor = (!u.cubeMap).Texture (f.texturePos)[Coord.x, Coord.y, Coord.z]
				});
		}
	}
}