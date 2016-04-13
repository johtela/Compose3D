namespace Compose3D.Textures
{
	using Maths;
	using GLTypes;
	using Geometry;
	using OpenTK.Graphics.OpenGL;
	using System;
	using System.IO;
	using System.Drawing;
	using Extensions;

	public class TextureParams : Params<TextureParameterName, object> { }

	public class Texture : GLObject
	{
		internal TextureTarget _target;
		internal int _glTexture;
		private PixelInternalFormat _pixelInternalFormat;
		private PixelFormat _pixelFormat;
		private PixelType _pixelType;

		public Texture (TextureTarget target)
		{
			_target = target;
			_glTexture = GL.GenTexture ();
		}

		public Texture (TextureTarget target, PixelInternalFormat internalFormat,
			int width, int height, PixelFormat format, PixelType type, IntPtr pixels)
			: this (target)
		{
			using (Scope ())
			{
				_pixelInternalFormat = internalFormat;
				_pixelFormat = format;
				_pixelType = type;
				GL.TexImage2D (target, 0, internalFormat, width, height, 0, format, type, pixels);
			}
		}

		public TextureTarget Target
		{
			get { return _target; }
		}

		public PixelFormat PixelFormat
		{
			get { return _pixelFormat; }
		}

		public PixelInternalFormat PixelInternalFormat
		{
			get { return _pixelInternalFormat; }
		}

		public PixelType PixelType
		{
			get { return _pixelType; }
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

		public Texture Parameters (TextureParams parameters)
		{
			if (parameters != null)
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
			return this;
		}

		public void LoadBitmap (Bitmap bitmap, TextureTarget target, int lodLevel)
		{
			using (Scope ())
			{
				var bitmapData = bitmap.LockBits (new Rectangle (0, 0, bitmap.Width, bitmap.Height),
				System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
				try
				{
					_pixelInternalFormat = MapPixelInternalFormat (bitmap.PixelFormat);
					_pixelFormat = MapPixelFormat (bitmap.PixelFormat);
					_pixelType = PixelType.UnsignedByte;
					GL.TexImage2D (target, lodLevel, _pixelInternalFormat, 
						bitmap.Width, bitmap.Height, 0, _pixelFormat, _pixelType, bitmapData.Scan0);
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
						_pixelType, bitmapData.Scan0);
				}
				finally
				{
					bitmap.UnlockBits (bitmapData);
				}
			}
		}
		
		public static Texture FromBitmap (Bitmap bitmap)
		{
			var result = new Texture (TextureTarget.Texture2D);
			using (result.Scope ())
				result.LoadBitmap (bitmap, result._target, 0);
			return result;
		}

		private static void CheckFileExists (string path)
		{
			if (!File.Exists (path))
				throw new ArgumentException ("Could not find texture file: " + path);
		}

		public static Texture FromFile (string path)
		{
			CheckFileExists (path);
			return FromBitmap (new Bitmap (path));
		}

		public static Texture CubeMapFromFiles (string[] paths, int lodLevel)
		{
			if (paths.Length > 6)
				throw new ArgumentException ("Too many file paths (cube has only 6 sides)", "paths");
			var result = new Texture (TextureTarget.TextureCubeMap);
			using (result.Scope ())
				result.AddCubeMapLodLevel (paths, lodLevel);
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

		public Texture MinLinearFiltering ()
		{
			using (Scope ())
				GL.TexParameter (_target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			return this;
		}

		public Texture MagLinearFiltering ()
		{
			using (Scope ())
				GL.TexParameter (_target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			return this;
		}

		public Texture LinearFiltering ()
		{
			return MinLinearFiltering ().MagLinearFiltering ();
		}

		public Texture ClampToEdges (Axes edgeAxes)
		{
			using (Scope ())
			{
				if ((edgeAxes & Axes.X) != 0)
					GL.TexParameter (_target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
				if ((edgeAxes & Axes.Y) != 0)
					GL.TexParameter (_target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
				if ((edgeAxes & Axes.Z) != 0)
					GL.TexParameter (_target, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
			}
			return this;
		}

		public Texture Mipmapped (int baseLevel = 0, int maxLevel = 10, bool linearFilteringWithinMipLevel = true,
			bool linearFilteringBetweenMipLevels = true)
		{
			using (Scope ())
			{
				GL.GenerateMipmap (MapMipmapTarget (_target));
				GL.TexParameter (_target, TextureParameterName.TextureBaseLevel, baseLevel);
				GL.TexParameter (_target, TextureParameterName.TextureMaxLevel, maxLevel);
				var filtering = linearFilteringWithinMipLevel ?
					linearFilteringBetweenMipLevels ? 
						TextureMinFilter.LinearMipmapLinear : 
						TextureMinFilter.LinearMipmapNearest :
					linearFilteringBetweenMipLevels ? 
						TextureMinFilter.NearestMipmapLinear : 
						TextureMinFilter.NearestMipmapNearest;
				GL.TexParameter (_target, TextureParameterName.TextureMinFilter, (int)filtering);
			}
			return this;
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