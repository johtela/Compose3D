namespace Compose3D.Textures
{
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Drawing.Imaging;

	public static class BitmapHelper
	{
		public static Bitmap TextToBitmap (this string text, int width, int height, PixelFormat pixelFormat,
			Font font, Brush brush, StringFormat stringFormat)
		{
			var bitmap = new Bitmap (width, height, pixelFormat);
			using (var gfx = Graphics.FromImage (bitmap))
			{
				gfx.SmoothingMode = SmoothingMode.AntiAlias;
				gfx.Clear (Color.Transparent);
				gfx.FillRectangle (new SolidBrush (Color.FromArgb (50, 50, 50, 50)), new Rectangle (0, 0, width, height));
				gfx.DrawString (text, font, brush, new RectangleF (0f, 0f, width, height), stringFormat);
			}
			return bitmap;
		}

		public static Bitmap TextToBitmapAligned (this string text, int width, int height, float fontSize,
			StringAlignment horizAlign, StringAlignment vertAlign)
		{
			var font = new Font ("Arial", fontSize);
			var brush = new SolidBrush (Color.Black);
			var stringFormat = new StringFormat ();
			stringFormat.Alignment = horizAlign;
			stringFormat.LineAlignment = vertAlign;
			return TextToBitmap (text, width, height, PixelFormat.Format32bppArgb, font, brush, stringFormat);
		}
	}
}

