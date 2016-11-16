namespace Compose3D.UI
{
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using OpenTK.Input;
	using Visuals;
	using Reactive;

	public class Picker : Control
	{
		public readonly Reaction<int> Selected;
		public readonly string[] Items;

		// Click regions
		private MouseRegions<int> _mouseRegions;

		// Control state
		private bool _active;
		private int _selected;

		public Picker (Reaction<int> selected, params string[] items) 
		{ 
			Items = items;
			Selected = selected;
			_mouseRegions = new MouseRegions<int> ();		
		}

		public Picker (Reaction<int> selected, IEnumerable<string> items)
			: this (selected, items.ToArray ()) {}

		public override Visual ToVisual (SizeF panelSize)
		{
			_mouseRegions.Clear ();
			return _active ? 
				Visual.VStack (HAlign.Left, Items.Select ((str, i) =>
				{
					var visual = Visual.Clickable (Visual.Label (str), _mouseRegions.Add (i));
					return i != _selected ?
							visual :
							Visual.Styled (Visual.Frame (
								Visual.Margin (visual, 2f),
								FrameKind.RoundRectangle, true), SelectedStyle);
				})) :
				Visual.Clickable (
					Visual.Frame (
						Visual.Margin (Visual.Label (Items [_selected]), 2f),
						FrameKind.Rectangle, false),
					_mouseRegions.Add (_selected));

		}

		public override void HandleInput (PointF relativeMousePos)
		{
			var hit = _mouseRegions.ItemUnderMouse (relativeMousePos);
			if (_active)
			{
				if (hit != null)
					_selected = hit.Item2;
				if (!InputState.MouseButtonDown (MouseButton.Left))
				{
					_active = false;
					Selected (_selected);
				}
			}
			else if (hit != null && InputState.MouseButtonPressed (MouseButton.Left))
				_active = true;
		}
	}
}