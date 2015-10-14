namespace Compose3D.Textures
{
	using GLTypes;
	using System.Collections.Generic;
	using OpenTK.Graphics.OpenGL;

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

		public Sampler (int texUnit, Params<SamplerParameterName> parameters)
			: this (texUnit)
		{
			SetParameters (parameters);
		}

		private void SetParameters (Params<SamplerParameterName> parameters)
		{
			foreach (var param in parameters)
			{
				if (param.Item2 is int)
					GL.SamplerParameter (_glSampler, param.Item1, (int)param.Item2);
				else if (param.Item2 is float)
					GL.SamplerParameter (_glSampler, param.Item1, (float)param.Item2);
				else
					throw new GLError ("Unsupported sampler parameter value type: " + param.Item2.GetType ());
			}
		}
	}
}

