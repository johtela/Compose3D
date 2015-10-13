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

		public Sampler (int texUnit, IDictionary<SamplerParameterName, object> parameters)
			: this (texUnit)
		{
			SetParameters (parameters);
		}

		private void SetParameters (IDictionary<SamplerParameterName, object> parameters)
		{
			foreach (var param in parameters)
			{
				if (param.Value is int)
					GL.SamplerParameter (_glSampler, param.Key, (int)param.Value);
				else if (param.Value is float)
					GL.SamplerParameter (_glSampler, param.Key, (float)param.Value);
				else
					throw new GLError ("Unsupported sampler parameter value type: " + param.Value.GetType ());
			}
		}
	}
}

