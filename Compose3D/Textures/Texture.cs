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
			Params<TextureParameterName> parameters)
		{
			_target = target;
			_glTexture = GL.GenTexture ();
			GL.BindTexture (target, _glTexture);
			GL.TexImage2D (target, level, internalFormat, width, height, 0, format, type, pixels);
			SetParameters (parameters);
			GL.BindTexture (target, 0);
		}

		private void SetParameters (Params<TextureParameterName> parameters)
		{
			foreach (var param in parameters)
			{
				if (param.Item2 is int)
					GL.TexParameter (_target, param.Item1, (int)param.Item2);
				else if (param.Item2 is float)
					GL.TexParameter (_target, param.Item1, (float)param.Item2);
				else
					throw new GLError ("Unsupported texture parameter Item2 type: " + param.Item2.GetType ());
			}
		}
	}
}

