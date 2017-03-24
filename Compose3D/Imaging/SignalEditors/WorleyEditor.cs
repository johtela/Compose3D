namespace Compose3D.Imaging.SignalEditors
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;
	using Imaging;
	using Maths;
	using Reactive;
	using UI;
	using Visuals;

	public enum ControlPointKind { Random, Halton23 }

	public class WorleyEditor : SignalEditor<Vec2, float>
	{
		public WorleyNoiseKind NoiseKind;
		public ControlPointKind ControlPoints;
		public DistanceKind DistanceKind;
		public int ControlPointCount;
		public int Seed;
		public float Jitter;
		public bool Periodic;

		private Func<int, IEnumerable<Vec2>>[] _cpGenerators = {
				Imaging.Signal.RandomControlPoints<Vec2>,
				_ => Imaging.Signal.HaltonControlPoints ()
			};

		private Func<Vec2, Vec2, float>[] _distFunctions = {
				Vec.DistanceTo<Vec2>,
				Vec.ManhattanDistanceTo<Vec2>
			};

		protected override Control CreateControl ()
		{
			var changed = Changed.Adapt<int, AnySignalEditor> (this);
			var changedf = Changed.Adapt<float, AnySignalEditor> (this);
			return FoldableContainer.WithLabel ("Worley Noise", true, HAlign.Left,
				Container.LabelAndControl ("Type: ",
					new Picker ((int)NoiseKind,
						React.By ((int i) => NoiseKind = (WorleyNoiseKind)i).And (changed),
						Enum.GetNames (typeof (WorleyNoiseKind))), true),
				Container.LabelAndControl ("Seed: ",
					new NumericEdit (Seed, true, 1,
						React.By ((float i) => Seed = (int)i).And (changedf)), true),
				Container.LabelAndControl ("CP Type: ",
					new Picker ((int)ControlPoints,
						React.By ((int i) => ControlPoints = (ControlPointKind)i).And (changed),
						Enum.GetNames (typeof (ControlPointKind))), true),
				Container.LabelAndControl ("CP Count: ",
					new NumericEdit (ControlPointCount, true, 1,
						React.By ((float i) => ControlPointCount = (int)i).And (changedf).Filter (i => i > 2)), true),
				Container.LabelAndControl ("Distance: ",
					new Picker ((int)DistanceKind,
						React.By ((int i) => DistanceKind = (DistanceKind)i).And (changed),
						Enum.GetNames (typeof (DistanceKind))), true),
				Container.LabelAndControl ("Jitter: ",
					new NumericEdit (Jitter, false, 0.1f,
						React.By ((float x) => Jitter = x).And (changedf)), true),
				Container.LabelAndControl ("Periodic: ",
					new Picker (Periodic ? 1 : 0,
						React.By ((int i) => Periodic = i != 0).And (changed),
						"No", "Yes"), true));
		}

		protected override void Load (XElement xelem)
		{
			NoiseKind = xelem.AttrEnum<WorleyNoiseKind> (nameof (NoiseKind));
			ControlPoints = xelem.AttrEnum<ControlPointKind> (nameof (ControlPoints));
			DistanceKind = xelem.AttrEnum<DistanceKind> (nameof (DistanceKind));
			ControlPointCount = xelem.AttrInt (nameof (ControlPointCount));
			Seed = xelem.AttrInt (nameof (Seed));
			Jitter = xelem.AttrFloat (nameof (Jitter));
			Periodic = xelem.AttrBool (nameof (Periodic));
		}

		protected override void Save (XElement xelem)
		{
			xelem.SetAttributeValue (nameof (NoiseKind), NoiseKind);
			xelem.SetAttributeValue (nameof (ControlPoints), ControlPoints);
			xelem.SetAttributeValue (nameof (DistanceKind), DistanceKind);
			xelem.SetAttributeValue (nameof (ControlPointCount), ControlPointCount);
			xelem.SetAttributeValue (nameof (Seed), Seed);
			xelem.SetAttributeValue (nameof (Jitter), Jitter);
			xelem.SetAttributeValue (nameof (Periodic), Periodic);
		}

		public override IEnumerable<AnySignalEditor> Inputs
		{
			get { return Enumerable.Empty<AnySignalEditor> (); }
		}

        private IEnumerable<Vec2> GetControlPoints ()
        {
            var cpoints = _cpGenerators[(int)ControlPoints] (Seed)
                .Take (ControlPointCount)
                .Jitter (Jitter);
            if (Periodic)
                cpoints = cpoints.ReplicateOnTorus ();
            return cpoints;
        }

        public override Signal<Vec2, float> Signal
		{
			get
			{
				return Imaging.Signal.WorleyNoise (NoiseKind, _distFunctions[(int)DistanceKind], 
                    GetControlPoints ());
			}
		}

        public WorleyArgs Args
        {
            get { return new WorleyArgs (DistanceKind, NoiseKind, GetControlPoints ().ToArray ()); }
        }
	}
}
