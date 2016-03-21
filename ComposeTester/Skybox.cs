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
	using Compose3D.SceneGraph;
	using Compose3D.Shaders;
	using Compose3D.Textures;
	using OpenTK.Graphics.OpenGL;

	[StructLayout (LayoutKind.Sequential, Pack = 4)]
	public struct SkyboxVertex : IVertex
	{
		internal Vec3 position;
		[OmitInGlsl]
		internal Vec3 normal;

		Vec3 IPositional<Vec3>.Position
		{
			get { return position; }
			set { position = value; }
		}

		Vec3 IPlanar<Vec3>.Normal
		{
			get { return normal; }
			set
			{
				if (value.IsNan ())
					throw new ArgumentException ("Normal component NaN");
				normal = value;
			}
		}

		public override string ToString ()
		{
			return string.Format ("[Vertex: Position={0}, Normal={3}]",
				position, normal);
		}
	}

	public class Skybox
	{
		public class SkyboxFragment : Fragment
		{
			public Vec3 texturePos;
		}

		public class SkyboxUniforms
		{
			public Uniform<Mat4> worldMatrix;
			public Uniform<Mat4> perspectiveMatrix;
			public Uniform<SamplerCube> cubeMap;

			public void Initialize (Program program)
			{
				using (program.Scope ())
					cubeMap &= new SamplerCube (0, new SamplerParams ()
					{
						{ SamplerParameterName.TextureMagFilter, All.Linear },
						{ SamplerParameterName.TextureMinFilter, All.Linear },
						{ SamplerParameterName.TextureWrapR, All.ClampToEdge },
						{ SamplerParameterName.TextureWrapS, All.ClampToEdge },
						{ SamplerParameterName.TextureWrapT, All.ClampToEdge }
					});
			}
		}

		public readonly Program SkyboxShader;
		public readonly SkyboxUniforms Uniforms;

		private Texture _texture;
		private VBO<SkyboxVertex> _vertices;
		private VBO<int> _indices;

		private const float _cubeSize = 20f;
		private readonly string[] _paths = new string[] 
			{ "sky_right", "sky_left", "sky_top", "sky_right", "sky_front", "sky_back" };

		public Skybox ()
		{
			SkyboxShader = new Program (VertexShader (), FragmentShader ());
			SkyboxShader.InitializeUniforms (Uniforms = new SkyboxUniforms ());
			Uniforms.Initialize (SkyboxShader);
			var cube = Extrusion.Cube<SkyboxVertex> (_cubeSize, _cubeSize, _cubeSize).Center ()
				.Translate (0f, -5f);
			_vertices = new VBO<SkyboxVertex> (cube.Vertices, BufferTarget.ArrayBuffer);
			_indices = new VBO<int> (cube.Indices, BufferTarget.ElementArrayBuffer);
			_texture = Texture.CubeMapFromFiles (
				_paths.Map (s => s == null ? null : string.Format (@"Textures/{0}.bmp", s)),
				new TextureParams ()
				{
					{ TextureParameterName.TextureMagFilter, All.Linear },
					{ TextureParameterName.TextureMinFilter, All.Linear },
					{ TextureParameterName.TextureWrapR, All.ClampToEdge },
					{ TextureParameterName.TextureWrapS, All.ClampToEdge },
					{ TextureParameterName.TextureWrapT, All.ClampToEdge }
				});
		}

		public void Render (Camera camera)
		{
			GL.Enable (EnableCap.CullFace);
			GL.CullFace (CullFaceMode.Front);
			GL.FrontFace (FrontFaceDirection.Cw);
			GL.Disable (EnableCap.DepthTest);

			using (SkyboxShader.Scope ())
			{
				(!Uniforms.cubeMap).Bind (_texture);
				Uniforms.worldMatrix &= camera.WorldToCamera.RemoveTranslation ();
				SkyboxShader.DrawElements (PrimitiveType.Triangles, _vertices, _indices);
				(!Uniforms.cubeMap).Unbind (_texture);
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
				from v in Shader.Inputs<SkyboxVertex> ()
				from u in Shader.Uniforms<SkyboxUniforms> ()
				select new SkyboxFragment ()
				{
					gl_Position = !u.perspectiveMatrix * !u.worldMatrix * new Vec4 (v.position, 1f),
					texturePos = v.position / 10f
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