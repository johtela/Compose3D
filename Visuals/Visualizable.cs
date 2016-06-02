namespace Visuals
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Drawing;
	using System.Drawing.Drawing2D;

	/// <summary>
	/// Interface that can be implemented by any object that
	/// can be visualized with the elements of the Visual library.
	/// </summary>
	public interface IVisualizable
	{
		Visual ToVisual ();
	}

	public class Visualizable : IVisualizable
	{
		private Func<Visual> _getVisual;

		public Visualizable (Func<Visual> getVisual)
		{
			_getVisual = getVisual;
		}

		public Visual ToVisual ()
		{
			return _getVisual ();
		}
	}
}
