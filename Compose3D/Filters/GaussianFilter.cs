namespace Compose3D.Filters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Geometry;
	using GLTypes;
	using Maths;
	using Reactive;
	using Shaders;
	using Textures;
	using OpenTK.Graphics.OpenGL;

	public static class GaussianFilter
	{
		public class GaussianFragment : Fragment
		{
			[GLArray (5)]
			public Vec2[] fragTexturePos;
		}

		public static Reaction<Tuple<Texture, Texture>> HorizontalGaussianFilter ()
		{
			return TextureFilter.Renderer (new Program (HorizontalVertexShader (), FragmentShader ()));
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
					fragTexturePos = new Vec2 [5]
					{
						v.texturePos + new Vec2 (pixelSize * -2f, 0f),
						v.texturePos + new Vec2 (pixelSize * -1f, 0f),
						v.texturePos,
						v.texturePos + new Vec2 (pixelSize * 1f, 0f),
						v.texturePos + new Vec2 (pixelSize * 2f, 0f)
					}
				}
			);
		}

		private static GLShader VerticelVertexShader ()
		{
			return GLShader.Create (ShaderType.VertexShader, () =>
				from v in Shader.Inputs<TexturedVertex> ()
				from u in Shader.Uniforms<TextureUniforms> ()
				let height = (!u.textureMap).Size (0).Y
				let pixelSize = 1f / height
				select new GaussianFragment ()
				{
					gl_Position = new Vec4 (v.position, 1f),
					fragTexturePos = new Vec2[5]
					{
						v.texturePos + new Vec2 (0f, pixelSize * -2f),
						v.texturePos + new Vec2 (0f, pixelSize * -1f),
						v.texturePos,
						v.texturePos + new Vec2 (0f, pixelSize * 1f),
						v.texturePos + new Vec2 (0f, pixelSize * 2f)
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
					weights = new float[] { 0.06136f, 0.24477f, 0.38774f, 0.24477f, 0.06136f }
				})
				let res = Enumerable.Range (0, 5).Aggregate (new Vec4 (0f), (r, i) => 
					r + (!u.textureMap).Texture (f.fragTexturePos [i]) * c.weights[i])
				select new
				{
					outColor = res
				}
			);
		}
	}
}