namespace Compose3D.UI
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using OpenTK.Input;
	using Maths;
	using Imaging;
	using Reactive;
	using Visuals;

	class ColorMapBar : Control
	{
		public readonly Reaction<ColorMap<Vec3>> Changed;
		public readonly Reaction<Tuple<float, Color>> ItemSelected;
		public readonly float DomainMin;
		public readonly float DomainMax;
		public readonly SizeF MinSize;
		public readonly ColorMap<Vec3> Value;

		// Click regions
		private MouseRegions<IVisualizable> _mouseRegions;

		// Control state
		private IVisualizable _pressed;
		private IVisualizable _highlighted;

		public ColorMapBar (float domainMin, float domainMax, SizeF minSize, 
			ColorMap<Vec3> value, Reaction<ColorMap<Vec3>> changed, 
			Reaction<Tuple<float, Color>> itemSelected)
		{
			DomainMin = domainMin;
			DomainMax = domainMax;
			MinSize = minSize;
			Value = value;
			Changed = changed;
			ItemSelected = itemSelected;
		}

		public override void HandleInput (PointF relativeMousePos)
		{
		}

		private SizeF Paint (GraphicsContext context, SizeF size)
		{
			return MinSize;
		}

		public override Visual ToVisual ()
		{
			return null;
		}
	}
}
