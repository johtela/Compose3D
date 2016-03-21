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
			public Uniform<SamplerCube> cubeSampler;

			public void Initialize (Program program)
			{
				using (program.Scope ())
					cubeSampler &= new SamplerCube (0, new SamplerParams ()
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
		private Geometry<SkyboxVertex> _geometry;

		private const float _cubeSize = 100f;
		private readonly string[] _paths = new string[] 
			{ "sky_right", "sky_left", "sky_top", "sky_bottom", "sky_back", "sky_front" };

		public Skybox ()
		{
			SkyboxShader = new Program (VertexShader (), FragmentShader ());
			SkyboxShader.InitializeUniforms (Uniforms = new SkyboxUniforms ());
			_geometry = 
				Extrusion.Cube<SkyboxVertex> (_cubeSize, _cubeSize, _cubeSize)
				.Center ()
				.ReverseWinding ();
			_texture = Texture.CubeMapFromFiles (
				_paths.Map (s => string.Format (@"Textures\{0}.bmp", s)),
				new TextureParams ()
				{
					{ TextureParameterName.TextureMagFilter, All.Linear },
					{ TextureParameterName.TextureMinFilter, All.Linear },
					{ TextureParameterName.TextureWrapR, All.ClampToEdge },
					{ TextureParameterName.TextureWrapS, All.ClampToEdge },
					{ TextureParameterName.TextureWrapT, All.ClampToEdge }
				});
		}

		public static GLShader VertexShader ()
		{
			return null;
		}

		public static GLShader FragmentShader ()
		{
			return null;
		}
	}
}