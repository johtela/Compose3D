namespace Compose3D.Textures
{
	using System;
    using OpenTK.Graphics.OpenGL;

	public interface ITexture : IEquatable<ITexture>
    {
		int Width { get; set; }
		int Height { get; set; }
		PixelInternalFormat InternalFormat { get; set; }
		PixelFormat PixelFormat { get; set; }
		PixelType PixelType { get; set; }
		IntPtr Pixels { get; set; }
    }

	public static class TextureHelpers
    {
		private class Texture : ITexture
		{
			public bool Equals (ITexture other)
			{
				return other == this;
			}

			public int Width { get; set; }
			public int Height { get; set; }
			public PixelInternalFormat InternalFormat { get; set; }
			public PixelFormat PixelFormat { get; set; }
			public PixelType PixelType { get; set; }
			public IntPtr Pixels { get; set; }
		}

		public static void Foo ()
		{
		}
    }
}
