namespace Compose3D.Imaging
{
	using System.Linq;
	using Extensions;
	using Compiler;
	using GLTypes;
	using Maths;
	using Reactive;
	using Shaders;
	using OpenTK.Graphics.OpenGL4;
	using Filter = Reactive.Reaction<System.Tuple<Textures.Texture, Textures.Texture>>;

	public static class GaussianFilter
	{
		public class GaussianFragment : Fragment
		{
			[FixedArray (7)]
			public Vec2[] fragTexturePos;
		}

		public static Filter Horizontal ()
		{
			return TextureFilter.Renderer (new GLProgram (HorizontalVertexShader (), FragmentShader ()));
		}

		public static Filter Vertical ()
		{
			return TextureFilter.Renderer (new GLProgram (VerticalVertexShader (), FragmentShader ()));
		}

		public static Filter Both ()
		{
			return Horizontal ().And (Vertical ().MapInput (TupleExt.Swap));
		}

		private static GLShader HorizontalVertexShader ()
		{
			return GLShader.Create (ShaderType.VertexShader, () =>
				from v in Shader.Inputs<TexturedVertex> ()
				from u in Shader.Uniforms<TextureUniforms> ()
				let width = (!u.textureMap).Size (0).X
				let pixelSize = 1f / width
				select new GaussianFragment ()
				{
					gl_Position = new Vec4 (v.position, 1f),
					fragTexturePos = new Vec2 [7]
					{
						v.texturePos + new Vec2 (pixelSize * -3f, 0f),
						v.texturePos + new Vec2 (pixelSize * -2f, 0f),
						v.texturePos + new Vec2 (pixelSize * -1f, 0f),
						v.texturePos,
						v.texturePos + new Vec2 (pixelSize * 1f, 0f),
						v.texturePos + new Vec2 (pixelSize * 2f, 0f),
						v.texturePos + new Vec2 (pixelSize * 3f, 0f)
					}
				}
			);
		}

		private static GLShader VerticalVertexShader ()
		{
			return GLShader.Create (ShaderType.VertexShader, () =>
				from v in Shader.Inputs<TexturedVertex> ()
				from u in Shader.Uniforms<TextureUniforms> ()
				let height = (!u.textureMap).Size (0).Y
				let pixelSize = 1f / height
				select new GaussianFragment ()
				{
					gl_Position = new Vec4 (v.position, 1f),
					fragTexturePos = new Vec2[7]
					{
						v.texturePos + new Vec2 (0f, pixelSize * -3f),
						v.texturePos + new Vec2 (0f, pixelSize * -2f),
						v.texturePos + new Vec2 (0f, pixelSize * -1f),
						v.texturePos,
						v.texturePos + new Vec2 (0f, pixelSize * 1f),
						v.texturePos + new Vec2 (0f, pixelSize * 2f),
						v.texturePos + new Vec2 (0f, pixelSize * 3f)
					}
				}
			);
		}

		private static GLShader FragmentShader ()
		{
			return GLShader.Create (ShaderType.FragmentShader, () =>
				from f in Shader.Inputs<GaussianFragment> ()
				from u in Shader.Uniforms<TextureUniforms> ()
				from c in Shader.Constants (new
				{
					weights = new float[] { 0.00598f, 0.060626f, 0.241843f,	0.383103f, 0.241843f, 0.060626f, 0.00598f }
				})
				select new
				{
					outColor = Enumerable.Range (0, 7).Aggregate (new Vec4 (0f), (r, i) => 
						r + (!u.textureMap).Texture (f.fragTexturePos [i]) * c.weights[i])
				}
			);
		}
	}
}