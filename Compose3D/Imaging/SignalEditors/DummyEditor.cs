namespace Compose3D.Imaging.SignalEditors
{
	using System.Collections.Generic;
	using System.Linq;
	using Visuals;
	using UI;

	internal class DummyEditor<T, U> : SignalEditor<T, U>
	{
		public Signal<T, U> Source;

		protected override Control CreateControl ()
		{
			return new Connected (
				Container.Frame (Label.Static (Name)),
				HAlign.Right, VAlign.Center);
		}

		protected override string ToCode ()
		{
			return Name;
		}

		public override IEnumerable<AnySignalEditor> Inputs
		{
			get { return Enumerable.Empty<AnySignalEditor> (); }
		}

		public override Signal<T, U> Signal
		{
			get { return Source; }
		}
	}
}
