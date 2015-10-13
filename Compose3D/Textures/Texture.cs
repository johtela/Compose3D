namespace Compose3D.Textures
{
	using GLTypes;
	using OpenTK.Graphics.OpenGL;
	using System;
	using System.Collections.Generic;

	public class Texture
	{
		private TextureTarget _target;
		internal int _glTexture;

		public Texture (TextureTarget target, int glTexture)
		{
			_target = target;
			_glTexture = glTexture;
		}

		public Texture (TextureTarget target, int level, PixelInternalFormat internalFormat,
			int width, int height, PixelFormat format, PixelType type, IntPtr pixels, 
			IDictionary<TextureParameterName, object> parameters)
		{
			_target = target;
			_glTexture = GL.GenTexture ();
			GL.BindTexture (target, _glTexture);
			GL.TexImage2D (target, level, internalFormat, width, height, 0, format, type, pixels);
			SetParameters (parameters);
			GL.BindTexture (target, 0);
		}

		private void SetParameters (IDictionary<TextureParameterName, object> parameters)
		{
			foreach (var param in parameters)
			{
				if (param.Value is int)
					GL.TexParameter (_target, param.Key, (int)param.Value);
				else if (param.Value is float)
					GL.TexParameter (_target, param.Key, (float)param.Value);
				else
					throw new GLError ("Unsupported texture parameter value type: " + param.Value.GetType ());
			}
		}
	}
}

