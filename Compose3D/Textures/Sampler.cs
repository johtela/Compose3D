namespace Compose3D.Textures
{
	using GLTypes;
	using System.Collections.Generic;
	using OpenTK.Graphics.OpenGL;

	public class SamplerParams : Params<SamplerParameterName, object> { }

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
					throw new GLError ("Unsupported sampler parameter value type: " + param.Item2.GetType ());
			}
		}
	}
}

