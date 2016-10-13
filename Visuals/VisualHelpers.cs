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

		public static Color ColorFromRGBA (float red, float green, float blue, float alpha)
		{
			return Color.FromArgb ((int)Math.Round (alpha * 255f), (int)Math.Round (red * 255f), 
				(int)Math.Round (green * 255f),	(int)Math.Round (blue * 255f));
		}

		public static Color ColorFromHSB (float hue, float saturation, float brightness)
		{
			if (hue < 0f || hue > 1f)
				throw new ArgumentOutOfRangeException ("hue", hue, "Value must be within range [0, 1].");
			if (saturation < 0f || saturation > 1f)
				throw new ArgumentOutOfRangeException ("saturation", saturation, "Value must be within range [0, 1].");
			if (brightness < 0f || brightness > 1f)
				throw new ArgumentOutOfRangeException ("brightness", brightness, "Value must be within range [0, 1].");

			if (saturation == 0)
				return ColorFromRGB (brightness, brightness, brightness);
			// the color wheel consists of 6 sectors. Figure out which sector you're in.
			float sectorPos = (hue % 1f) * 6f;
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

		public static Tuple<float, float, float> ToHSB(this Color color) 
		{
			var cmax = Math.Max (Math.Max(color.R, color.G), color.B);
			var cmin = Math.Min (Math.Min(color.R, color.G), color.B);
			var brightness = ((float) cmax) / 255.0f;
			var saturation = cmax == 0 ? 0 : 
				((float) (cmax - cmin)) / ((float) cmax);
			float hue;
			if (saturation == 0)
				hue = 0;
			else 
			{
				float redc = ((float) (cmax - color.R)) / ((float) (cmax - cmin));
				float greenc = ((float) (cmax - color.G)) / ((float) (cmax - cmin));
				float bluec = ((float) (cmax - color.B)) / ((float) (cmax - cmin));
				if (color.R == cmax)
					hue = bluec - greenc;
				else if (color.G == cmax)
					hue = 2.0f + redc - bluec;
				else
					hue = 4.0f + greenc - redc;
				hue /= 6.0f;
				if (hue < 0f)
					hue += 1.0f;
			}
			return Tuple.Create (hue, saturation, brightness);
		}

		public static VisualDirection Opposite (this VisualDirection direction)
		{
			return direction == VisualDirection.Horizontal ?
				VisualDirection.Vertical :
				VisualDirection.Horizontal;
		}
	}
}
