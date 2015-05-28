namespace Compose3D.GLTypes
{
	using System;

	public class ShaderState
	{
		private object _inputs;
		private object _uniforms;

		public ShaderState(object inputs, object uniforms)
		{
			_inputs = inputs;
			_uniforms = uniforms;
		}

		public T Inputs<T> ()
		{
			return (T)_inputs;
		}

		public T Uniforms<T> ()
		{
			return (T)_uniforms;
		}
	}
}