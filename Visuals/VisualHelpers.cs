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
			PixelFormat pixelFormat)
		{
			var result = new Bitmap (size.Width, size.Height, pixelFormat);
			var ctx = new GraphicsContext (Graphics.FromImage (result), 
				VisualStyle.Default);
			visual.Render (ctx, new VBox (size.Width, size.Height));
			return result;
		}
	}
}
