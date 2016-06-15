namespace Compose3D.Textures
{
	using OpenTK.Graphics.OpenGL;
	using Geometry;
	using Maths;
	using System;
	using System.Collections.Generic;
	using Extensions;

	public class SamplerParams : Params<SamplerParameter, object> { }

	public abstract class Sampler
	{
		internal int _glSampler;
		internal int _texUnit;

		public Sampler () : this (0) { }

		public Sampler (int texUnit)
		{
			_glSampler = GL.GenSampler ();
			_texUnit = texUnit;
		}

		public void Bind (Texture texture)
		{
			GL.ActiveTexture (TextureUnit.Texture0 + _texUnit);
			GL.BindTexture (texture._target, texture._glTexture);
			GL.BindSampler (_texUnit, _glSampler);
		}

		public void Unbind (Texture texture)
		{
			GL.BindSampler (_texUnit, 0);
			GL.BindTexture (texture._target, 0);
		}

		public static void Bind (Sampler[] samplers, Texture[] textures)
		{
			CheckEnoughSamplers (samplers, textures);
			for (int i = 0; i < textures.Length; i++)
				samplers[i].Bind (textures[i]);
		}

		public static void Unbind (Sampler[] samplers, Texture[] textures)
		{
			CheckEnoughSamplers (samplers, textures);
			for (int i = 0; i < textures.Length; i++)
				samplers[i].Unbind (textures[i]);
		}

		private static void CheckEnoughSamplers (Sampler[] samplers, Texture[] textures)
		{
			if (textures.Length > samplers.Length)
				throw new ArgumentException ("The number of textures exceeds the number of samplers");
		}

		public static void Bind (IDictionary<Sampler, Texture> bindings)
		{
			foreach (var binding in bindings)
				binding.Key.Bind (binding.Value);
		}

		public static void Unbind (IDictionary<Sampler, Texture> bindings)
		{
			foreach (var binding in bindings)
				binding.Key.Unbind (binding.Value);
		}
	}

	public static class SamplerModifiers
	{
		public static S Parameters<S> (this S sampler, SamplerParams parameters)
			where S : Sampler
		{
			foreach (var param in parameters)
			{
				if (param.Item2 is int || param.Item2.GetType ().IsEnum)
					GL.SamplerParameter (sampler._glSampler, param.Item1, (int)param.Item2);
				else if (param.Item2 is float)
					GL.SamplerParameter (sampler._glSampler, param.Item1, (float)param.Item2);
				else
					throw new ArgumentException ("Unsupported sampler parameter value type: " + param.Item2.GetType ());
			}
			return sampler;
		}

		public static S MinNearestColor<S> (this S sampler)
			where S : Sampler
		{
			GL.SamplerParameter (sampler._glSampler, SamplerParameter.TextureMinFilter, (int)TextureMinFilter.Nearest);
			return sampler;
		}

		public static S MagNearestColor<S> (this S sampler)
			where S : Sampler
		{
			GL.SamplerParameter (sampler._glSampler, SamplerParameter.TextureMagFilter, (int)TextureMagFilter.Nearest);
			return sampler;
		}

		public static S NearestColor<S> (this S sampler)
			where S : Sampler
		{
			return sampler.MinNearestColor ().MagNearestColor ();
		}

		public static S MinLinearFiltering<S> (this S sampler)
			where S : Sampler
		{
			GL.SamplerParameter (sampler._glSampler, SamplerParameter.TextureMinFilter, (int)TextureMinFilter.Linear);
			return sampler;
		}

		public static S MagLinearFiltering<S> (this S sampler)
			where S : Sampler
		{
			GL.SamplerParameter (sampler._glSampler, SamplerParameter.TextureMagFilter, (int)TextureMagFilter.Linear);
			return sampler;
		}

		public static S LinearFiltering<S> (this S sampler)
			where S : Sampler
		{
			return sampler.MinLinearFiltering ().MagLinearFiltering ();
		}

		public static S ClampToEdges<S> (this S sampler, Axes edgeAxes)
			where S : Sampler
		{
			if ((edgeAxes & Axes.X) != 0)
				GL.SamplerParameter (sampler._glSampler, SamplerParameter.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
			if ((edgeAxes & Axes.Y) != 0)
				GL.SamplerParameter (sampler._glSampler, SamplerParameter.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
			if ((edgeAxes & Axes.Z) != 0)
				GL.SamplerParameter (sampler._glSampler, SamplerParameter.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
			return sampler;
		}

		public static S ClampToBorder<S> (this S sampler, Vec4 color, Axes edgeAxes)
			where S : Sampler
		{
			GL.SamplerParameter (sampler._glSampler, SamplerParameter.TextureBorderColor, 
				color.ToArray<Vec4, float>());
			if ((edgeAxes & Axes.X) != 0)
				GL.SamplerParameter (sampler._glSampler, SamplerParameter.TextureWrapS, 
					(int)TextureWrapMode.ClampToBorder);
			if ((edgeAxes & Axes.Y) != 0)
				GL.SamplerParameter (sampler._glSampler, SamplerParameter.TextureWrapT, 
					(int)TextureWrapMode.ClampToBorder);
			if ((edgeAxes & Axes.Z) != 0)
				GL.SamplerParameter (sampler._glSampler, SamplerParameter.TextureWrapR, 
					(int)TextureWrapMode.ClampToBorder);
			return sampler;
		}

		public static S Mipmapped<S> (this S sampler, bool linearFilteringWithinMipLevel = true,
			bool linearFilteringBetweenMipLevels = true)
			where S : Sampler
		{
			var filtering = linearFilteringWithinMipLevel ?
				linearFilteringBetweenMipLevels ?
					TextureMinFilter.LinearMipmapLinear :
					TextureMinFilter.LinearMipmapNearest :
				linearFilteringBetweenMipLevels ?
					TextureMinFilter.NearestMipmapLinear :
					TextureMinFilter.NearestMipmapNearest;
			GL.SamplerParameter (sampler._glSampler, SamplerParameter.TextureMinFilter, (int)filtering);
			return sampler;
		}
	}
}
