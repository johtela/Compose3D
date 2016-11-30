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

	public class KeyboardCommand
	{
		public readonly string Description;
		public readonly Reaction<Key> Pressed;
		public readonly Key Command;
		public readonly Key[] Modifiers;

		public KeyboardCommand (string description, Reaction<Key> pressed,
			Key command, params Key[] modifiers)
		{
			Description = description;
			Pressed = pressed;
			Command = command;
			Modifiers = modifiers;
		}
	}

	public class CommandContainer : Control
	{
		public readonly Control Inner;
		public readonly KeyboardCommand[] Commands;

		private int _countDown;
		private string _message;

		public CommandContainer (Control inner, params KeyboardCommand[] commands)
		{
			Inner = inner;
			Commands = commands;
		}

		private VisualStyle NotifierStyle (int alpha)
		{
			return new VisualStyle (Style,
				font: new Font ("Calibri", 10f, FontStyle.Bold),
				textBrush: new SolidBrush (Color.FromArgb (alpha, Color.White)));
		}

		public override void HandleInput (PointF relativeMousePos)
		{
			foreach (var command in Commands)
				if (InputState.KeyPressed (command.Command, false) &&
					command.Modifiers.All (InputState.KeyDown))
				{
					command.Pressed (command.Command);
					_message = command.Description;
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
					Visual.Styled (Visual.Label (_message), NotifierStyle (_countDown))) :
				inner;
		}
	}
}
