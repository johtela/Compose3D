namespace Compose3D.UI
{
	using System.Drawing;
	using Visuals;

	public abstract class Control
	{
		public static VisualStyle Style =
			new VisualStyle (
				VisualStyle.Default,
				font: new Font ("Calibri", 10f),
				pen: new Pen (Color.Black, 1.5f), 
				brush: new SolidBrush (Color.FromArgb (150, 140, 140, 140)));

		public static VisualStyle SelectedStyle = 
			new VisualStyle (
				Style,
				pen: new Pen (Color.White, 1.5f),
				textBrush: Brushes.White,
				brush: Brushes.LightSteelBlue);

		public abstract Visual ToVisual (SizeF panelSize);

		public abstract void HandleInput (PointF relativeMousePos);
	}
}
