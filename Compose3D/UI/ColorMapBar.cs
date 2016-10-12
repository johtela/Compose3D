namespace Compose3D.UI
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Linq;
	using OpenTK.Input;
	using Maths;
	using Imaging;
	using Reactive;
	using Visuals;

	public class ColorMapBar : Control
	{
		public readonly Reaction<ColorMap<Vec3>> Changed;
		public readonly Reaction<Tuple<float, Color>> ItemSelected;
		public readonly float DomainMin;
		public readonly float DomainMax;
		public readonly SizeF MinSize;
		public readonly ColorMap<Vec3> ColorMap;

		// Click regions
		private MouseRegions<object> _mouseRegions;

		// Control state
		private float? _selected;
		private float? _dragging;

		public ColorMapBar (float domainMin, float domainMax, SizeF minSize, 
			ColorMap<Vec3> colorMap, Reaction<ColorMap<Vec3>> changed, 
			Reaction<Tuple<float, Color>> itemSelected)
		{
			if (colorMap.Count > 0 && (colorMap.MinKey < domainMin || colorMap.MaxKey > domainMax))
				throw new ArgumentException ("Color map values out of min/max range.");
			DomainMin = domainMin;
			DomainMax = domainMax;
			MinSize = minSize;
			ColorMap = colorMap;
			Changed = changed;
			ItemSelected = itemSelected;
			_mouseRegions = new MouseRegions<object> ();
		}

		private SizeF PaintBar (GraphicsContext context, SizeF size)
		{
			var cnt = ColorMap.Count;
			var blend = new ColorBlend (cnt + 2);
			blend.Colors[0] = ColorMap[DomainMin].ToColor ();
			blend.Positions[0] = 0f;
			var i = 1;
			foreach (var sp in ColorMap.NormalizedSamplePoints (DomainMin, DomainMax))
			{
				blend.Colors[i] = sp.Value.ToColor ();
				blend.Positions[i++] = sp.Key;
			}
			blend.Colors [i] = ColorMap[DomainMax].ToColor ();
			blend.Positions[i] = 1f;
			var rect = new RectangleF (new PointF (0f, 0f), new SizeF (size.Width, size.Height));
			var brush = new LinearGradientBrush (rect, Color.Black, Color.Black, LinearGradientMode.Vertical);
			brush.InterpolationColors = blend;
			context.Graphics.FillRectangle (brush, rect);
			return size;
		}

		private SizeF PaintKnob (Color color, GraphicsContext context, SizeF size)
		{
			context.Graphics.FillRectangle (new SolidBrush (color), 0f, 0f, size.Width, size.Height);
			context.Graphics.DrawRectangle (context.Style.Pen, 0f, 0f, size.Width, size.Height);
			return size;
		}

		private float NormalizeKey (float key)
		{
			return (key - DomainMin) / (DomainMax - DomainMin);
		}

		public override Visual ToVisual ()
		{
			_mouseRegions.Clear ();
			var knobSize = new SizeF (MinSize.Width / 2f, MinSize.Width / 4f);
			return Visual.Margin (
				Visual.HStack (VAlign.Top,
					Visual.Clickable (
						Visual.Custom (new SizeF (MinSize.Width / 2f, MinSize.Height), PaintBar),
						_mouseRegions.Add (ColorMap)),
					Visual.VPile (HAlign.Left, VAlign.Center,
						ColorMap.Select (sp =>
							Tuple.Create (NormalizeKey (sp.Key),
								Visual.Clickable (
									sp.Key == _selected ?
										Visual.Styled (KnobVisual (sp.Value, knobSize), SelectedStyle) :
										KnobVisual (sp.Value, knobSize),
									_mouseRegions.Add (sp.Key))
							)
						)
					)
				),
				top: knobSize.Height, bottom: knobSize.Height);
		}

		private Visual KnobVisual (Vec3 color, SizeF knobSize)
		{
			return Visual.Custom (knobSize, 
				(ctx, size) => PaintKnob (color.ToColor (), ctx, size));
		}

		private float KeyValueAtPos (RectangleF barRect, PointF pos)
		{
			var relPos = (pos.Y - barRect.Location.Y) / barRect.Height;
			return relPos * (DomainMax - DomainMin) + DomainMin;
		}

		public override void HandleInput (PointF relativeMousePos)
		{
			var hit = _mouseRegions.ItemUnderMouse (relativeMousePos);
			var leftMousePressed = MouseButtonPressed (MouseButton.Left);
			if (hit != null && hit.Item2 == ColorMap && leftMousePressed)
			{
				var key = KeyValueAtPos (hit.Item1, relativeMousePos);
				ColorMap.Add (key, ColorMap[key]);
				Changed (ColorMap);
				_selected = key;
			}
			else if (hit != null && hit.Item2 is float)
			{
				var hitKey = (float)hit.Item2;
				if (leftMousePressed)
				{
					if (hitKey != _selected)
					{
						_selected = hitKey;
						ItemSelected (Tuple.Create (hitKey, ColorMap[hitKey].ToColor ()));
					}
					_dragging = hitKey;
					return;
				}
				else if (ColorMap.Count > 1 && MouseButtonPressed (MouseButton.Right))
				{
					_selected = null;
					ItemSelected (null);
					ColorMap.Remove (hitKey);
					Changed (ColorMap);
				}
			}
			if (_dragging != null && MouseButtonDown (MouseButton.Left))
			{
				var newKey = KeyValueAtPos (_mouseRegions[0].Item1, relativeMousePos);
				if (newKey >= DomainMin && newKey <= DomainMax && 
					ColorMap.MoveKey (_dragging.Value, newKey))
				{
					Changed (ColorMap);
					_dragging = newKey;
					_selected = newKey;
				}
			}
			else
				_dragging = null;
		}
	}
}
