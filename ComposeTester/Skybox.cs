﻿namespace ComposeTester
{
	using System;
	using System.Linq;
	using System.Runtime.InteropServices;
	using Extensions;
	using Compose3D.GLTypes;
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.SceneGraph;
	using Compose3D.Shaders;
	using Compose3D.Textures;
	using OpenTK.Graphics.OpenGL;

	public class Skybox
	{
		public class SkyboxFragment : Fragment
		{
			public Vec3 texturePos;
		}

		public class SkyboxUniforms : Uniforms
		{
			public Uniform<Mat4> worldMatrix;
			public Uniform<Mat4> perspectiveMatrix;
			public Uniform<SamplerCube> cubeMap;

			public SkyboxUniforms (Program program)
				: base (program)
			{
				using (program.Scope ())
					cubeMap &= new SamplerCube (0).LinearFiltering ().ClampToEdges (Axes.All);
			}
		}

		public readonly Program SkyboxShader;
		public readonly SkyboxUniforms Uniforms;
		public readonly Texture EnvironmentMap;

		private VBO<PositionalVertex> _vertices;
		private VBO<int> _indices;

		private const float _cubeSize = 20f;
		private readonly string[] _paths = new string[] 
			{ "sky_right", "sky_left", "sky_top", "sky_bottom", "sky_front", "sky_back" };

		public Skybox (SceneGraph sceneGraph)
		{
			SkyboxShader = new Program (VertexShader (), FragmentShader ());
			Uniforms = new SkyboxUniforms (SkyboxShader);
			var cube = Extrusion.Cube<PositionalVertex> (_cubeSize, _cubeSize, _cubeSize).Center ();
			_vertices = new VBO<PositionalVertex> (cube.Vertices, BufferTarget.ArrayBuffer);
			_indices = new VBO<int> (cube.Indices, BufferTarget.ElementArrayBuffer);
			EnvironmentMap = Texture.CubeMapFromFiles (
				_paths.Map (s => string.Format (@"Textures/{0}.bmp", s)), 0)
				.LinearFiltering ().ClampToEdges (Axes.All);
			sceneGraph.GlobalLighting.DiffuseMap = Texture.CubeMapFromFiles (
				_paths.Map (s => string.Format (@"Textures/{0}_scaled.bmp", s)), 0)
				.LinearFiltering ().ClampToEdges (Axes.All);
		}

		public void Render (Camera camera)
		{
			using (SkyboxShader.Scope ())
			{
				GL.Enable (EnableCap.CullFace);
				GL.CullFace (CullFaceMode.Front);
				GL.FrontFace (FrontFaceDirection.Cw);
				GL.Disable (EnableCap.DepthTest);
				GL.Disable (EnableCap.Blend);
				GL.DrawBuffer (DrawBufferMode.Back);

				(!Uniforms.cubeMap).Bind (EnvironmentMap);
				Uniforms.worldMatrix &= camera.WorldToCamera.RemoveTranslation ();
				SkyboxShader.DrawElements (PrimitiveType.Triangles, _vertices, _indices);
				(!Uniforms.cubeMap).Unbind (EnvironmentMap);
			}
		}

		public void UpdateViewMatrix (Mat4 matrix)
		{
			using (SkyboxShader.Scope ())
				Uniforms.perspectiveMatrix &= matrix;
		}

		public static GLShader VertexShader ()
		{
			return GLShader.Create (ShaderType.VertexShader, () =>
				from v in Shader.Inputs<PositionalVertex> ()
				from u in Shader.Uniforms<SkyboxUniforms> ()
				select new SkyboxFragment ()
				{
					gl_Position = !u.perspectiveMatrix * !u.worldMatrix * new Vec4 (v.position, 1f),
					texturePos = v.position + new Vec3 (0f, 0.5f, 0f)
				});
		}

		public static GLShader FragmentShader ()
		{
			return GLShader.Create (ShaderType.FragmentShader, () =>
				from f in Shader.Inputs<SkyboxFragment> ()
				from u in Shader.Uniforms<SkyboxUniforms> ()
				select new
				{
					outputColor = (!u.cubeMap).Texture (f.texturePos)[Coord.x, Coord.y, Coord.z]
				});
		}
	}
}