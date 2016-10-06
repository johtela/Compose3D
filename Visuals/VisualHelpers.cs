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

		public static Color ColorFromRGB (float red, float green, float blue)
		{
			return Color.FromArgb ((int)Math.Round (red * 255f), (int)Math.Round (green * 255f), 
				(int)Math.Round (blue * 255f));
		}

		public static Color ColorFromHSB (float hue, float saturation, float brightness)
		{
			if (hue < 0f || hue > 360f)
				throw new ArgumentOutOfRangeException ("hue", hue, "Value must be within range [0, 360].");
			if (saturation < 0f || saturation > 1f)
				throw new ArgumentOutOfRangeException ("saturation", saturation, "Value must be within range [0, 1].");
			if (brightness < 0f || brightness > 1f)
				throw new ArgumentOutOfRangeException ("brightness", brightness, "Value must be within range [0, 1].");

			if (saturation == 0)
				return ColorFromRGB (brightness, brightness, brightness);
			// the color wheel consists of 6 sectors. Figure out which sector you're in.
			float sectorPos = hue / 60f;
			int sectorNumber = (int)(Math.Floor (sectorPos));
			// get the fractional part of the sector
			float fractionalSector = sectorPos - sectorNumber;

			// calculate values for the three axes of the color.
			float p = brightness * (1f - saturation);
			float q = brightness * (1f - (saturation * fractionalSector));
			float t = brightness * (1f - (saturation * (1f - fractionalSector)));

			// assign the fractional colors to r, g, and b based on the sector
			// the angle is in.
			switch (sectorNumber)
			{
				case 0: return ColorFromRGB (brightness, t, p);
				case 1: return ColorFromRGB (q, brightness, p);
				case 2: return ColorFromRGB (p, brightness, t);
				case 3: return ColorFromRGB (p, q, brightness);
				case 4: return ColorFromRGB (t, p, brightness);
				default: return ColorFromRGB (brightness, p, q);
			}
		}

		public static Color ChangeHue (this Color color, float hue)
		{
			return ColorFromHSB (hue, color.GetSaturation (), color.GetBrightness ());
		}

		public static Color ChangeSaturation (this Color color, float saturation)
		{
			return ColorFromHSB (color.GetHue (), saturation, color.GetBrightness ());
		}

		public static Color ChangeBrightness (this Color color, float brightness)
		{
			return ColorFromHSB (color.GetHue (), color.GetSaturation (), brightness);
		}

		public static VisualDirection Opposite (this VisualDirection direction)
		{
			return direction == VisualDirection.Horizontal ?
				VisualDirection.Vertical :
				VisualDirection.Horizontal;
		}
	}
}
