namespace Compose3D.Textures
{
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;

	public static class BitmapHelper
	{
		public static Bitmap TextToBitmap (this string text, int width, int height, PixelFormat pixelFormat,
			Font font, Brush brush, StringFormat stringFormat)
		{
			var bitmap = new Bitmap (width, height, pixelFormat);
			using (var gfx = Graphics.FromImage (bitmap))
			{
				gfx.Clear (Color.Transparent);
				gfx.DrawString (text, font, brush, new RectangleF (0f, 0f, width, height), stringFormat);
			}
			return bitmap;
		}

		public static Bitmap TextToBitmapCentered (this string text, int width, int height, float fontSize)
		{
			var font = new Font ("Monospace", fontSize);
			var brush = new SolidBrush (Color.White);
			var stringFormat = new StringFormat ();
			stringFormat.Alignment = StringAlignment.Center;
			stringFormat.LineAlignment = StringAlignment.Center;
			return TextToBitmap (text, width, height, PixelFormat.Format24bppRgb, font, brush, stringFormat);
		}
	}
}

