namespace Compose3D.UI
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using OpenTK.Input;
	using Reactive;
	using Visuals;
	using Extensions;

	public class Container : Control
	{
		public readonly VisualDirection Direction;
		public readonly HAlign HorizAlign;
		public readonly VAlign VertAlign;
		public readonly bool Framed;
		public readonly bool WrapAround;
		public readonly List<Tuple<Control, Reaction<Control>>> Controls;

		private MouseRegions<Tuple<Control, Reaction<Control>>> _mouseRegions;
		private Control _selected;

		public Container (VisualDirection direction, HAlign horizAlign,	VAlign vertAlign,  
			bool framed, bool wrapAround, IEnumerable<Tuple<Control, Reaction<Control>>> controls)
		{
			Direction = direction;
			HorizAlign = horizAlign;
			VertAlign = vertAlign;
			Framed = framed;
			WrapAround = wrapAround;
			Controls = new List<Tuple<Control, Reaction<Control>>> (controls);
			if (Controls.Select (TupleExt.Second).Any (r => r != null))
				_mouseRegions = new MouseRegions<Tuple<Control, Reaction<Control>>> ();
		}

		public Container (VisualDirection direction, HAlign horizAlign, VAlign vertAlign,
			bool framed, bool wrapAround, params Tuple<Control, Reaction<Control>>[] controls)
			: this (direction, horizAlign, vertAlign, framed, wrapAround, 
				  (IEnumerable<Tuple<Control, Reaction<Control>>>)controls)
		{ }

		public Container (VisualDirection direction, HAlign horizAlign, VAlign vertAlign,
			bool framed, bool wrapAround, IEnumerable<Control> controls)
			: this (direction, horizAlign, vertAlign, framed, wrapAround, 
				controls.Select (c => new Tuple<Control, Reaction<Control>> (c, null)))
		{ }

		public Container (VisualDirection direction, HAlign horizAlign, VAlign vertAlign,
			bool framed, bool wrapAround, params Control[] controls)
			: this (direction, horizAlign, vertAlign, framed, wrapAround, 
				(IEnumerable<Control>)controls)
		{ }

		public override Visual ToVisual (SizeF panelSize)
		{
			if (_mouseRegions != null)
				_mouseRegions.Clear ();
			var cvisuals = Controls.Select (c =>
			{
				var v = c.Item1.ToVisual (panelSize);
				if (c.Item1 == _selected)
					v = Visual.Styled (v, new VisualStyle (
						brush: new SolidBrush (Color.FromArgb (50, 150, 150, 255))));
				if (c.Item2 != null)
					v = Visual.Clickable (v, _mouseRegions.Add (c));
				return v;
			});
			var flowSize = new SizeF (panelSize.Width - 8f, panelSize.Height - 8f);
			var visual = Visual.Margin (
				Direction == VisualDirection.Horizontal ?
					(WrapAround ?
						Visual.HFlow (VertAlign, flowSize, cvisuals.Select (v => Visual.Margin (v, 2f))) :
						Visual.HStack (VertAlign, cvisuals.Select (v => Visual.Margin (v, left: 2f, right: 2f)))) :
					(WrapAround ?
						Visual.VFlow (HorizAlign, flowSize, cvisuals.Select (v => Visual.Margin (v, 2f))) :
						Visual.VStack (HorizAlign, cvisuals.Select (v => Visual.Margin (v, top: 2f, bottom: 2f)))),
				2f);
			return Framed ? Visual.Frame (visual, FrameKind.RoundRectangle, true) : visual;
		}

		public override void HandleInput (PointF relativeMousePos)
		{
			if (_mouseRegions != null)
			{
				var hit = _mouseRegions.ItemUnderMouse (relativeMousePos);
				if (InputState.MouseButtonPressed (MouseButton.Left))
				{
					if (hit != null && _selected != hit.Item2.Item1)
					{
						_selected = hit.Item2.Item1;
						hit.Item2.Item2 (_selected);
					}
					else if (hit == null)
						_selected = null;
				}
			}
			foreach (var control in Controls.Select (TupleExt.First))
				control.HandleInput (relativeMousePos);
		}

		public static Container LabelAndControl (string label, Control control, bool framed)
		{
			return new Container (VisualDirection.Horizontal, HAlign.Left, VAlign.Center, framed, false,
				Label.Static (label, FontStyle.Regular), control);
		}

		public static Container Horizontal (bool framed, bool wrapAround, 
			params Tuple<Control, Reaction<Control>>[] controls)
		{
			return new Container (VisualDirection.Horizontal, HAlign.Left, VAlign.Top, framed, wrapAround, 
				controls);
		}

		public static Container Horizontal (bool framed, bool wrapAround, 
			IEnumerable<Tuple<Control, Reaction<Control>>> controls)
		{
			return new Container (VisualDirection.Horizontal, HAlign.Left, VAlign.Top, framed, wrapAround, 
				controls);
		}

		public static Container Horizontal (bool framed, bool wrapAround, params Control[] controls)
		{
			return new Container (VisualDirection.Horizontal, HAlign.Left, VAlign.Top, framed, wrapAround,
				controls);
		}

		public static Container Horizontal (bool framed, bool wrapAround, IEnumerable<Control> controls)
		{
			return new Container (VisualDirection.Horizontal, HAlign.Left, VAlign.Top, framed, wrapAround,
				controls);
		}

		public static Container Vertical (bool framed, bool wrapAround, 
			params Tuple<Control, Reaction<Control>>[] controls)
		{
			return new Container (VisualDirection.Vertical, HAlign.Left, VAlign.Top, framed, wrapAround,
				controls);
		}

		public static Container Vertical (bool framed, bool wrapAround,
			IEnumerable<Tuple<Control, Reaction<Control>>> controls)
		{
			return new Container (VisualDirection.Vertical, HAlign.Left, VAlign.Top, framed, wrapAround,
				controls);
		}

		public static Container Vertical (bool framed, bool wrapAround, params Control[] controls)
		{
			return new Container (VisualDirection.Vertical, HAlign.Left, VAlign.Top, framed, wrapAround,
				controls);
		}

		public static Container Vertical (bool framed, bool wrapAround, IEnumerable<Control> controls)
		{
			return new Container (VisualDirection.Vertical, HAlign.Left, VAlign.Top, framed, wrapAround,
				controls);
		}

		public static Container Frame (Control control)
		{
			return new Container (VisualDirection.Horizontal, HAlign.Left, VAlign.Top, true, false, 
				control);
		}
	}
}
