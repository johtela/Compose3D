namespace Compose3D.UI
{
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using OpenTK.Input;
	using Visuals;
	using Reactive;

	public class ListView : Control
	{
		public readonly Reaction<IVisualizable> ItemClicked;
		public readonly IEnumerable<IVisualizable> Items;

		// Click regions
		private MouseRegions<IVisualizable> _mouseRegions;

		// Control state
		private IVisualizable _pressed;
		private IVisualizable _highlighted;

		public ListView (Reaction<IVisualizable> itemClicked, 
			IEnumerable<IVisualizable> items)
		{
			Items = items;
			ItemClicked = itemClicked;
			_mouseRegions = new MouseRegions<IVisualizable> ();
		}

		public ListView (Reaction<IVisualizable> itemClicked, params IVisualizable[] items) 
			: this (itemClicked, (IEnumerable<IVisualizable>)items)
		{ }

		public override Visual ToVisual (SizeF panelSize)
		{
			_mouseRegions.Clear ();
			return Visual.VStack (HAlign.Left,
				Items.Select (i =>
				{
					var visual = Visual.Clickable (i.ToVisual (), _mouseRegions.Add (i));
					return i != _highlighted ?
						visual :
						Visual.Styled (Visual.Frame (visual, FrameKind.RoundRectangle, true), SelectedStyle);
				}
			));
		}

		public override void HandleInput (PointF relativeMousePos)
		{
			var hit = _mouseRegions.ItemUnderMouse (relativeMousePos);
			if (InputState.MouseButtonDown (MouseButton.Left))
			{
				if (_pressed == null && hit != null)
				{
					_pressed = hit.Item2;
					_highlighted = hit.Item2;
				}
				else
					_highlighted = _pressed == hit ? _pressed : null;
			}
			else if (_pressed != null)
			{
				if (_pressed == hit)
					ItemClicked (_pressed);
				_pressed = null;
				_highlighted = null;
			}
		}
	}
}