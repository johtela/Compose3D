﻿namespace Compose3D.Imaging.SignalEditors
{
	using System.Collections.Generic;
	using System.Xml.Linq;
	using Extensions;
	using CLTypes;
	using Imaging;
	using Visuals;
	using Reactive;
	using Maths;
	using UI;
	using Textures;

	internal class WarpEditor : SignalEditor<float>
	{
		public float Scale;
		public Vec2 Dv;
		public SignalEditor<float> Source;
		public SignalEditor<float> Warp;

		public WarpEditor (Texture texture)
			: base (texture) { }

		protected override Control CreateControl ()
		{
			var changed = Changed.Adapt<float, AnySignalEditor> (this);
			return FoldableContainer.WithLabel ("Warp", true, HAlign.Left,
				InputSignalControl ("Source", Source),
				InputSignalControl ("Warp", Warp),
				Container.LabelAndControl ("Scale: ",
					new NumericEdit (Scale, false, 0.1f,
						React.By ((float s) => Scale = s).And (changed)), true));
		}

		protected override void Load (XElement xelem)
		{
			Scale = xelem.AttrFloat (nameof (Scale));
		}

		protected override void Save (XElement xelem)
		{
			xelem.SetAttributeValue (nameof (Scale), Scale);
		}

		public override IEnumerable<AnySignalEditor> Inputs
		{
			get { return EnumerableExt.Enumerate (Source, Warp); }
		}

		public override Signal<Vec2, float> Signal
		{
			get { return Source.Signal.Warp (Warp.Signal.Cache ().Scale (Scale), Dv); }
		}

		public Value<float> Args
		{ 
			get { return KernelArg.Value (Scale); }
		}
	}
}
