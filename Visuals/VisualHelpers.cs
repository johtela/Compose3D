namespace Visuals
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Drawing.Imaging;
	using System.Drawing.Text;
	using System.Windows.Forms;

	public static class VisualHelpers
	{
		public static Bitmap ToBitmap (this Visual visual, Size size,
			PixelFormat pixelFormat, VisualStyle style)
		{
			var result = new Bitmap (size.Width, size.Height, pixelFormat);
			var gfx = Graphics.FromImage (result);
			gfx.SmoothingMode = SmoothingMode.AntiAlias;
			gfx.TextRenderingHint = TextRenderingHint.AntiAlias;
			var ctx = new GraphicsContext (gfx, style);
			visual.Render (ctx, new VBox (size.Width, size.Height));
			return result;
		}

		public static void UpdateBitmap (this Visual visual, Bitmap bitmap, VisualStyle style)
		{
			var gfx = Graphics.FromImage (bitmap);
			gfx.Clear (Color.Transparent);
			gfx.SmoothingMode = SmoothingMode.AntiAlias;
			gfx.TextRenderingHint = TextRenderingHint.AntiAlias;
			var ctx = new GraphicsContext (gfx, style);
			visual.Render (ctx, new VBox (bitmap.Size));
		}
	}
}
