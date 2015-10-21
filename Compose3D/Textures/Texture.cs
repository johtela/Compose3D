namespace Compose3D.Textures
{
	using GLTypes;
	using OpenTK.Graphics.OpenGL;
	using System;
	using System.IO;
	using System.Drawing;

	public class TextureParams : Params<TextureParameterName, object> { }

	public class Texture
	{
		internal TextureTarget _target;
		internal int _glTexture;

		public Texture (TextureTarget target, int glTexture)
		{
			_target = target;
			_glTexture = glTexture;
		}

		public Texture (TextureTarget target, int level, PixelInternalFormat internalFormat,
			int width, int height, PixelFormat format, PixelType type, IntPtr pixels, 
			TextureParams parameters)
		{
			_target = target;
			_glTexture = GL.GenTexture ();
			GL.BindTexture (target, _glTexture);
			GL.TexImage2D (target, level, internalFormat, width, height, 0, format, type, pixels);
			SetParameters (parameters);
			GL.BindTexture (target, 0);
		}

		private void SetParameters (TextureParams parameters)
		{
			if (parameters == null)
				return;
			foreach (var param in parameters)
			{
				if (param.Item2 is int || param.Item2.GetType ().IsEnum)
					GL.TexParameter (_target, param.Item1, (int)param.Item2);
				else if (param.Item2 is float)
					GL.TexParameter (_target, param.Item1, (float)param.Item2);
				else
					throw new ArgumentException ("Unsupported texture parameter Item2 type: " + param.Item2.GetType ());
			}
		}

		public static Texture FromBitmap (Bitmap bitmap, TextureParams parameters)
		{
			var bitmapData = bitmap.LockBits (new Rectangle (0, 0, bitmap.Width, bitmap.Height),
				System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
			try
			{
				return new Texture (TextureTarget.Texture2D, 0, MapPixelInternalFormat (bitmap.PixelFormat),
					bitmap.Width, bitmap.Height, MapPixelFormat (bitmap.PixelFormat), PixelType.UnsignedByte,
					bitmapData.Scan0, parameters);
			}
			finally
			{
				bitmap.UnlockBits (bitmapData);
			}
		}

		public static Texture FromFile (string path, TextureParams parameters)
		{
			if (!File.Exists (path))
				throw new ArgumentException ("Could not find texture file: " + path);
			return FromBitmap (new Bitmap (path), parameters);
		}

		private static PixelInternalFormat MapPixelInternalFormat (System.Drawing.Imaging.PixelFormat pixelFormat)
		{
			switch (pixelFormat)
			{
				case System.Drawing.Imaging.PixelFormat.Alpha: return PixelInternalFormat.Alpha;
				case System.Drawing.Imaging.PixelFormat.Canonical: return PixelInternalFormat.Rgba;
				case System.Drawing.Imaging.PixelFormat.Format16bppRgb565: return PixelInternalFormat.R5G6B5IccSgix;
				case System.Drawing.Imaging.PixelFormat.Format24bppRgb: return PixelInternalFormat.Rgb;
				case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
				case System.Drawing.Imaging.PixelFormat.Format32bppArgb: return PixelInternalFormat.Rgba;
				case System.Drawing.Imaging.PixelFormat.Format48bppRgb: return PixelInternalFormat.Rgb16;
				case System.Drawing.Imaging.PixelFormat.Format64bppPArgb:
				case System.Drawing.Imaging.PixelFormat.Format64bppArgb: return PixelInternalFormat.Rgba16;
				default: throw new ArgumentException ("Unsupported pixel format: " + pixelFormat.ToString ());
			}
		}

		private static PixelFormat MapPixelFormat (System.Drawing.Imaging.PixelFormat pixelFormat)
		{
			switch (pixelFormat)
			{
				case System.Drawing.Imaging.PixelFormat.Alpha: return PixelFormat.Alpha;
				case System.Drawing.Imaging.PixelFormat.Canonical: return PixelFormat.Bgra;
				case System.Drawing.Imaging.PixelFormat.Format16bppRgb565: return PixelFormat.R5G6B5IccSgix;
				case System.Drawing.Imaging.PixelFormat.Format24bppRgb: return PixelFormat.Bgr;
				case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
				case System.Drawing.Imaging.PixelFormat.Format32bppArgb: return PixelFormat.Bgra;
				case System.Drawing.Imaging.PixelFormat.Format48bppRgb: return PixelFormat.Bgr;
				case System.Drawing.Imaging.PixelFormat.Format64bppPArgb:
				case System.Drawing.Imaging.PixelFormat.Format64bppArgb: return PixelFormat.Bgra;
				default: throw new ArgumentException ("Unsupported pixel format: " + pixelFormat.ToString ());
			}
		}
	}
}