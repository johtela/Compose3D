namespace Compose3D.UI
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using OpenTK.Input;
	using Visuals;

	public class ListView : Control
	{
		public List<IVisualizable> Items { get; private set; }

		public ListView ()
		{
			Items = new List<IVisualizable> ();
		}

		public ListView (IEnumerable<IVisualizable> items) : this ()
		{
			Items.AddRange (items);
		}

		public ListView (params IVisualizable[] items) 
			: this ((IEnumerable<IVisualizable>)items)
		{ }

		public override Visual ToVisual ()
		{
			return Visual.VStack (HAlign.Left, Items.Select (i => i.ToVisual ()));
		}

		public override void HandleInput (MouseDevice mouse, KeyboardDevice keyboard, 
			PointF relativeMousePos)
		{
		}
	}
}