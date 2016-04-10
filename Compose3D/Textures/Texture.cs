namespace Compose3D.Textures
{
	using Maths;
	using GLTypes;
	using OpenTK.Graphics.OpenGL;
	using System;
	using System.IO;
	using System.Drawing;
	using Extensions;

	public class TextureParams : Params<TextureParameterName, object> 
	{
		public static TextureParams Create (bool linear, bool wrap)
		{
			var result = new TextureParams ();
			if (linear)
			{
				result.Add (TextureParameterName.TextureMagFilter, All.Linear);
				result.Add (TextureParameterName.TextureMinFilter, All.Linear);
			}
			else
			{
				result.Add (TextureParameterName.TextureMagFilter, All.Nearest);
				result.Add (TextureParameterName.TextureMinFilter, All.Nearest);
			}
			if (wrap)
			{
				result.Add (TextureParameterName.TextureWrapR, All.Repeat);
				result.Add (TextureParameterName.TextureWrapS, All.Repeat);
				result.Add (TextureParameterName.TextureWrapT, All.Repeat);					
			}
			else
			{
				result.Add (TextureParameterName.TextureWrapR, All.ClampToEdge);
				result.Add (TextureParameterName.TextureWrapS, All.ClampToEdge);
				result.Add (TextureParameterName.TextureWrapT, All.ClampToEdge);
			}
			return result;
		}
	}

	public class Texture : GLObject
	{
		internal TextureTarget _target;
		internal int _glTexture;
		private PixelFormat _pixelFormat;

		public Texture (TextureTarget target)
		{
			_target = target;
			_glTexture = GL.GenTexture ();
		}

		public Texture (TextureTarget target, bool useMipmap, PixelInternalFormat internalFormat,
			int width, int height, PixelFormat format, PixelType type, IntPtr pixels, 
			TextureParams parameters)
			: this (target)
		{
			using (Scope ())
			{
				_pixelFormat = format;
				GL.TexImage2D (target, 0, internalFormat, width, height, 0, format, type, pixels);
				if (useMipmap)
					GL.GenerateMipmap (MapMipmapTarget (target));
				SetParameters (parameters);
			}
		}

		public Vec2i Size
		{
			get
			{
				Vec2i result = new Vec2i (0);
				using (Scope ())
				{
					GL.GetTexLevelParameter (_target, 0, GetTextureParameter.TextureWidth, out result.X);
					GL.GetTexLevelParameter (_target, 0, GetTextureParameter.TextureHeight, out result.Y);
				}
				return result;
			}
		}

		public override void Use ()
		{
			GL.BindTexture (_target, _glTexture);
		}

		public override void Release ()
		{
			GL.BindTexture (_target, 0);
		}

		public void SetParameters (TextureParams parameters)
		{
			if (parameters == null)
				return;
			using (Scope ())
			{
				foreach (var param in parameters)
				{
					if (param.Item2 is int || param.Item2.GetType ().IsEnum)
						GL.TexParameter (_target, param.Item1, (int)param.Item2);
					else if (param.Item2 is float)
						GL.TexParameter (_target, param.Item1, (float)param.Item2);
					else
						throw new ArgumentException ("Unsupported texture parameter Item2 type: " +
							param.Item2.GetType ());
				}
			}
		}

		public void LoadBitmap (Bitmap bitmap, TextureTarget target, int lodLevel)
		{
			using (Scope ())
			{
				var bitmapData = bitmap.LockBits (new Rectangle (0, 0, bitmap.Width, bitmap.Height),
				System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
				try
				{
					_pixelFormat = MapPixelFormat (bitmap.PixelFormat);
					GL.TexImage2D (target, lodLevel, MapPixelInternalFormat (bitmap.PixelFormat), 
						bitmap.Width, bitmap.Height, 0, _pixelFormat, PixelType.UnsignedByte, bitmapData.Scan0);
				}
				finally
				{
					bitmap.UnlockBits (bitmapData);
				}
			}
		}

		public void UpdateBitmap (Bitmap bitmap, TextureTarget target, int lodLevel)
		{
			using (Scope ())
			{
				var size = Size;
				if (bitmap.Width != size.X || bitmap.Height != size.Y)
					throw new ArgumentException ("The bitmap dimensions must be same as the texture's");
				if (MapPixelFormat (bitmap.PixelFormat) != _pixelFormat)
					throw new ArgumentException ("The pixel format of the bitmap must be same as the texture's");
				
				var bitmapData = bitmap.LockBits (new Rectangle (0, 0, bitmap.Width, bitmap.Height),
					System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
				try
				{
					GL.TexSubImage2D (target, lodLevel, 0, 0, size.X, size.Y, _pixelFormat,
						PixelType.UnsignedByte, bitmapData.Scan0);
				}
				finally
				{
					bitmap.UnlockBits (bitmapData);
				}
			}
		}
		
		public static Texture FromBitmap (Bitmap bitmap, bool useMipmap, TextureParams parameters)
		{
			var result = new Texture (TextureTarget.Texture2D);
			using (result.Scope ())
			{
				result.LoadBitmap (bitmap, result._target, 0);
				result.SetParameters (parameters);
				if (useMipmap)
					GL.GenerateMipmap (MapMipmapTarget (TextureTarget.Texture2D));
			}
			return result;
		}

		private static void CheckFileExists (string path)
		{
			if (!File.Exists (path))
				throw new ArgumentException ("Could not find texture file: " + path);
		}

		public static Texture FromFile (string path, bool useMipmap, TextureParams parameters)
		{
			CheckFileExists (path);
			return FromBitmap (new Bitmap (path), useMipmap, parameters);
		}

		public static Texture CubeMapFromFiles (string[] paths, int lodLevel, TextureParams parameters)
		{
			if (paths.Length > 6)
				throw new ArgumentException ("Too many file paths (cube has only 6 sides)", "paths");
			var result = new Texture (TextureTarget.TextureCubeMap);
			using (result.Scope ())
			{
				result.AddCubeMapLodLevel (paths, lodLevel);
				result.SetParameters (parameters);
			}
			return result;
		}
		
		public void AddCubeMapLodLevel (string[] paths, int lodLevel)
		{
			using (Scope ())
			{
				for (int i = 0; i < paths.Length; i++)
				{
					var path = paths[i];
					if (!string.IsNullOrEmpty (path))
					{
						CheckFileExists (path);
						LoadBitmap (new Bitmap (path), TextureTarget.TextureCubeMapPositiveX + i, lodLevel);
					}
				}
			}
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

		private static GenerateMipmapTarget MapMipmapTarget (TextureTarget target)
		{
			switch (target)
			{
				case TextureTarget.Texture2D:
					return GenerateMipmapTarget.Texture2D;
				case TextureTarget.Texture2DArray:
					return GenerateMipmapTarget.Texture2DArray;
				case TextureTarget.Texture2DMultisample:
					return GenerateMipmapTarget.Texture2DMultisample;
				case TextureTarget.TextureCubeMap:
					return GenerateMipmapTarget.TextureCubeMap;
				case TextureTarget.TextureCubeMapArray:
					return GenerateMipmapTarget.TextureCubeMapArray;
				default:
					throw new ArgumentException ("Unsupported texture target: " + target.ToString (), "target");
			}
		}
	}
}