namespace Compose3D.Textures
{
	using GLTypes;
	using System.Collections.Generic;
	using OpenTK.Graphics.OpenGL;
	using System;
	using Extensions;	

	public class SamplerParams : Params<SamplerParameterName, object> 
	{
		public static SamplerParams Create (bool linear, bool wrap)
		{
			var result = new SamplerParams ();
			if (linear)
			{
				result.Add (SamplerParameterName.TextureMagFilter, All.Linear);
				result.Add (SamplerParameterName.TextureMinFilter, All.Linear);
			}
			else
			{
				result.Add (SamplerParameterName.TextureMagFilter, All.Nearest);
				result.Add (SamplerParameterName.TextureMinFilter, All.Nearest);
			}
			if (wrap)
			{
				result.Add (SamplerParameterName.TextureWrapR, All.Repeat);
				result.Add (SamplerParameterName.TextureWrapS, All.Repeat);
				result.Add (SamplerParameterName.TextureWrapT, All.Repeat);					
			}
			else
			{
				result.Add (SamplerParameterName.TextureWrapR, All.ClampToEdge);
				result.Add (SamplerParameterName.TextureWrapS, All.ClampToEdge);
				result.Add (SamplerParameterName.TextureWrapT, All.ClampToEdge);
			}
			return result;
		}
	}

	public abstract class Sampler
	{
		internal int _glSampler;
		internal int _texUnit;

		public Sampler () : this (0) {}

		public Sampler (int texUnit)
		{
			_glSampler = GL.GenSampler ();
			_texUnit = texUnit;
		}

		public Sampler (int texUnit, SamplerParams parameters)
			: this (texUnit)
		{
			SetParameters (parameters);
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

		private void SetParameters (SamplerParams parameters)
		{
			if (parameters == null)
				return;
			foreach (var param in parameters)
			{
				if (param.Item2 is int || param.Item2.GetType ().IsEnum)
					GL.SamplerParameter (_glSampler, param.Item1, (int)param.Item2);
				else if (param.Item2 is float)
					GL.SamplerParameter (_glSampler, param.Item1, (float)param.Item2);
				else
					throw new ArgumentException ("Unsupported sampler parameter value type: " + param.Item2.GetType ());
			}
		}
	}
}

