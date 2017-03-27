namespace Compose3D.Imaging.SignalEditors
{
	using System.Collections.Generic;
	using System.Linq;
	using Maths;
	using Visuals;
	using Textures;
	using UI;

	internal class DummyEditor<T> : SignalEditor<T>
		where T : struct
	{
		public Signal<Vec2, T> Source;

		public DummyEditor (Texture texture)
			: base (texture) { }

		protected override Control CreateControl ()
		{
			return new Connected (
				Container.Frame (Label.Static (Name)),
				HAlign.Right, VAlign.Center);
		}

		public override IEnumerable<AnySignalEditor> Inputs
		{
			get { return Enumerable.Empty<AnySignalEditor> (); }
		}

		public override Signal<Vec2, T> Signal
		{
			get { return Source; }
		}
	}
}
