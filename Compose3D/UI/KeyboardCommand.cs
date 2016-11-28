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
		public readonly string CommandDesc;
		public readonly Reaction<Key> Pressed;
		public Key Command;
		public Key[] Modifiers;

		private int _countDown;

		public KeyboardCommand (Control inner, string commandDesc, Reaction<Key> pressed, 
			Key command, params Key[] modifiers)
		{
			Inner = inner;
			CommandDesc = commandDesc;
			Pressed = pressed;
			Command = command;
			Modifiers = modifiers;
		}

		private VisualStyle NotifierStyle (int alpha)
		{
			return new VisualStyle (Style,
				font: new Font (FontFamily.GenericSansSerif, 11f, FontStyle.Bold),
				textBrush: new SolidBrush (Color.FromArgb (alpha, Color.White)));
		}

		public override void HandleInput (PointF relativeMousePos)
		{
			if (InputState.KeyPressed (Command, false) && Modifiers.All (InputState.KeyDown))
			{
				Pressed (Command);
				_countDown = 256;
			}
			else
				Inner.HandleInput (relativeMousePos);
		}

		public override Visual ToVisual (SizeF panelSize)
		{
			var inner = Inner.ToVisual (panelSize);
			_countDown = Math.Max (_countDown - 2, 0);
			return _countDown > 0 ?
				Visual.VStack (HAlign.Left,
					inner,
					Visual.Styled (Visual.Label (CommandDesc), NotifierStyle (_countDown))) :
				inner;
		}
	}
}
