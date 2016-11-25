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

	public class KeyboardCommand : Control
	{
		public readonly Control Inner;
		public readonly Reaction<Key> Pressed;
		public Key Command;
		public Key[] Modifiers;

		public KeyboardCommand (Control inner, Reaction<Key> pressed, Key command, 
			params Key[] modifiers)
		{
			Inner = inner;
			Pressed = pressed;
			Command = command;
			Modifiers = modifiers;
		}

		public override void HandleInput (PointF relativeMousePos)
		{
			if (InputState.KeyPressed (Command, false) && Modifiers.All (InputState.KeyDown))
				Pressed (Command);
			else
				Inner.HandleInput (relativeMousePos);
		}

		public override Visual ToVisual (SizeF panelSize)
		{
			return Inner.ToVisual (panelSize);
		}
	}
}
