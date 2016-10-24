namespace Compose3D.UI
{
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using OpenTK.Input;
	using Reactive;
	using Visuals;

	public class Container : Control
	{
		public readonly VisualDirection Direction;
		public readonly HAlign HorizAlign;
		public readonly VAlign VertAlign;
		public readonly bool Framed;
		public readonly bool WrapAround;
		public readonly List<Control> Controls;
		public readonly Reaction<Control> ControlSelected;

		private MouseRegions<Control> _mouseRegions;
		private Control _selected;

		public Container (VisualDirection direction, HAlign horizAlign,	VAlign vertAlign,  
			bool framed, bool wrapAround, Reaction<Control> controlSelected, 
			IEnumerable<Control> controls)
		{
			Direction = direction;
			HorizAlign = horizAlign;
			VertAlign = vertAlign;
			Framed = framed;
			WrapAround = wrapAround;
			Controls = new List<Control> (controls);
			if (controlSelected != null)
			{
				ControlSelected = controlSelected;
				_mouseRegions = new MouseRegions<Control> ();
			}
		}

		public Container (VisualDirection direction, HAlign horizAlign,	VAlign vertAlign,  
			bool framed, bool wrapAround, Reaction<Control> controlSelected, 
			params Control[] controls)
			: this (direction, horizAlign, vertAlign, framed, wrapAround, controlSelected, 
				(IEnumerable<Control>)controls)
		{}
		
		public override Visual ToVisual (SizeF panelSize)
		{
			if (ControlSelected != null)
				_mouseRegions.Clear ();
			var cvisuals = Controls.Select (c =>
			{
				var v = c.ToVisual (panelSize);
				if (c == _selected)
					v = Visual.Styled (v, new VisualStyle (Style,
						brush: new SolidBrush (Color.FromArgb (50, 150, 150, 255))));
				if (ControlSelected != null)
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
			if (ControlSelected != null)
			{
				var hit = _mouseRegions.ItemUnderMouse (relativeMousePos);
				if (InputState.MouseButtonPressed (MouseButton.Left))
				{
					_selected = hit != null ? hit.Item2 : null;
					ControlSelected (_selected);
				}
			}
			foreach (var control in Controls)
				control.HandleInput (relativeMousePos);
		}

		public static Container LabelAndControl (string label, Control control, bool framed)
		{
			return new Container (VisualDirection.Horizontal, HAlign.Left, VAlign.Center, framed, false, null,
				Label.Static (label, FontStyle.Regular), control);
		}

		public static Container Horizontal (bool framed, bool wrapAround, Reaction<Control> controlSelected,
			params Control[] controls)
		{
			return new Container (VisualDirection.Horizontal, HAlign.Left, VAlign.Top, framed, wrapAround, 
				controlSelected, controls);
		}

		public static Container Horizontal (bool framed, bool wrapAround, Reaction<Control> controlSelected,
			IEnumerable<Control> controls)
		{
			return new Container (VisualDirection.Horizontal, HAlign.Left, VAlign.Top, framed, wrapAround, 
				controlSelected, controls);
		}

		public static Container Horizontal (bool framed, bool wrapAround, params Control[] controls)
		{
			return new Container (VisualDirection.Horizontal, HAlign.Left, VAlign.Top, framed, wrapAround,
				null, controls);
		}

		public static Container Horizontal (bool framed, bool wrapAround, IEnumerable<Control> controls)
		{
			return new Container (VisualDirection.Horizontal, HAlign.Left, VAlign.Top, framed, wrapAround,
				null, controls);
		}

		public static Container Vertical (bool framed, bool wrapAround, Reaction<Control> controlSelected,
			params Control[] controls)
		{
			return new Container (VisualDirection.Vertical, HAlign.Left, VAlign.Top, framed, wrapAround,
				controlSelected, controls);
		}

		public static Container Vertical (bool framed, bool wrapAround, Reaction<Control> controlSelected,
			IEnumerable<Control> controls)
		{
			return new Container (VisualDirection.Vertical, HAlign.Left, VAlign.Top, framed, wrapAround,
				controlSelected, controls);
		}

		public static Container Vertical (bool framed, bool wrapAround, params Control[] controls)
		{
			return new Container (VisualDirection.Vertical, HAlign.Left, VAlign.Top, framed, wrapAround,
				null, controls);
		}

		public static Container Vertical (bool framed, bool wrapAround, IEnumerable<Control> controls)
		{
			return new Container (VisualDirection.Vertical, HAlign.Left, VAlign.Top, framed, wrapAround,
				null, controls);
		}

		public static Container Frame (Control control)
		{
			return new Container (VisualDirection.Horizontal, HAlign.Left, VAlign.Top, true, false, null, control);
		}
	}
}
