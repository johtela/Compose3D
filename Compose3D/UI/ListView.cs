﻿namespace Compose3D.UI
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

		public override Visual ToVisual ()
		{
			_mouseRegions.Clear ();
			var selectedStyle = new VisualStyle (VisualStyle.Default,
				textBrush: Brushes.White,
				brush: Brushes.DarkGray);
			return Visual.VStack (HAlign.Left,
				Items.Select (i =>
				{
					var visual = Visual.Clickable (i.ToVisual (), _mouseRegions.Add (i));
					return i != _highlighted ?
						visual :
						Visual.Styled (Visual.Frame (visual, FrameKind.RoundRectangle, true), selectedStyle);
				}
			));
		}

		public override void HandleInput (MouseDevice mouse, KeyboardDevice keyboard, 
			PointF relativeMousePos)
		{
			var item = _mouseRegions.ItemUnderMouse (relativeMousePos);
			if (mouse[MouseButton.Left])
			{
				if (_pressed == null)
				{
					_pressed = item;
					_highlighted = item;
				}
				else
					_highlighted = _pressed == item ? _pressed : null;
			}
			else if (_pressed != null)
			{
				if (_pressed == item)
					ItemClicked (_pressed);
				_pressed = null;
				_highlighted = null;
			}
		}
	}
}