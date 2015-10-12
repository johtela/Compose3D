namespace Compose3D.GLTypes
{
	using OpenTK.Graphics.OpenGL;
	using System;
	using System.Collections.Generic;

	public class Texture
	{
		internal int _glTexture;

		public Texture (int glTexture)
		{
			_glTexture = glTexture;
		}

		public Texture (TextureTarget target, int level, PixelInternalFormat internalFormat,
			int width, int height, PixelFormat format, PixelType type, IntPtr pixels, 
			IDictionary<TextureParameterName, object> parameters)
		{
			_glTexture = GL.GenTexture ();
			GL.BindTexture (target, _glTexture);
			GL.TexImage2D (target, level, internalFormat, width, height, 0, format, type, pixels);
			SetParameters (target, parameters);
			GL.BindTexture (target, 0);
		}

		private void SetParameters (TextureTarget target, IDictionary<TextureParameterName, object> parameters)
		{
			foreach (var param in parameters)
			{
				if (param.Value is int)
				{
					var value = (int)param.Value;
					GL.TexParameterI (target, param.Key, ref value);
				}
				else if (param.Value is float)
					GL.TexParameter (target, param.Key, (float)param.Value);
				else
					throw new GLError ("Unsupported texture parameter value type: " + param.Value.GetType ());
			}
		}

	}
}

