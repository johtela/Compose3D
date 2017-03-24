namespace Compose3D.Imaging.SignalEditors
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;
	using Extensions;
	using Imaging;
	using Visuals;
	using Reactive;
	using Maths;
	using UI;

	public class SpectralControlEditor<V> : SignalEditor<V, float>
		where V : struct, IVec<V, float>
	{
		public SignalEditor<V, float> Source;
		public int FirstBand;
		public int LastBand;
		public List<float> BandWeights;

		private Container _bandContainer;

		private Tuple<Control, Reaction<Control>> BandSlider (int band)
		{
			return new Tuple<Control, Reaction<Control>> (
				new Slider (VisualDirection.Vertical, 16f, 100f, 0f, 1f, BandWeights[band],
					React.By ((float x) => BandWeights[band] = x)
					.And (Changed.Adapt<float, AnySignalEditor> (this))),
				null);
		}

		private void ChangeFirstBand (float fb)
		{
			var value = (int)fb;
			if (value >= 0 && value <= LastBand)
			{
				if (value < FirstBand)
					for (int i = value; i < FirstBand; i++)
						_bandContainer.Controls.Insert (0, BandSlider (i));
				else
					for (int i = FirstBand; i < value; i++)
						_bandContainer.Controls.RemoveAt (0);
				FirstBand = value;
			}
		}

		private void ChangeLastBand (float lb)
		{
			var value = (int)lb;
			if (value < 16 && value >= FirstBand)
			{
				if (value > LastBand)
					for (int i = LastBand + 1; i <= value; i++)
						_bandContainer.Controls.Add (BandSlider (i));
				else
					for (int i = value; i < LastBand; i++)
						_bandContainer.Controls.RemoveAt (_bandContainer.Controls.Count - 1);
				LastBand = value;
			}
		}

		private IEnumerable<float> ActiveBandWeights ()
		{
			return BandWeights.Skip (FirstBand).Take (LastBand - FirstBand + 1);
		}

		protected override Control CreateControl ()
		{
			var changed = Changed.Adapt<float, AnySignalEditor> (this);
			var fbEdit = Container.LabelAndControl ("First Band: ",
				new NumericEdit (FirstBand, true, 1f, React.By<float> (ChangeFirstBand).And (changed)), true);
			var lbEdit = Container.LabelAndControl ("Last Band: ",
				new NumericEdit (LastBand, true, 1f, React.By<float> (ChangeLastBand).And (changed)), true);
			var sliders = Enumerable.Range (FirstBand, LastBand - FirstBand + 1)
				.Select (BandSlider).ToArray ();
			_bandContainer = Container.Horizontal (true, false, sliders);
			return FoldableContainer.WithLabel ("Spectral Control", true, HAlign.Left,
				InputSignalControl ("Source", Source),
				fbEdit, lbEdit, _bandContainer);
		}

		protected override void Load (XElement xelem)
		{
			FirstBand = xelem.AttrInt (nameof (FirstBand));
			LastBand = xelem.AttrInt (nameof (LastBand));
			BandWeights = new List<float> (
				from sp in xelem.Element (nameof (BandWeights)).Descendants ("Weight")
				select sp.AttrFloat ("Value"));
		}

		protected override void Save (XElement xelem)
		{
			xelem.SetAttributeValue (nameof (FirstBand), FirstBand);
			xelem.SetAttributeValue (nameof (LastBand), LastBand);
			xelem.Add (new XElement (nameof (BandWeights),
				from weight in BandWeights
				select new XElement ("Weight",
					new XAttribute ("Value", weight))));
		}

		public override IEnumerable<AnySignalEditor> Inputs
		{
			get { return EnumerableExt.Enumerate (Source); }
		}

		public override Signal<V, float> Signal
		{
			get
			{
				return Source.Signal.SpectralControl (FirstBand, LastBand,
					ActiveBandWeights ().ToArray ());
			}
		}

        public SpectralControlArgs Args
        {
            get { return new SpectralControlArgs (FirstBand, LastBand, BandWeights.ToArray ()); }
        }
	}
}
