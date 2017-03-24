	namespace Compose3D.Imaging.SignalEditors
{
    using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Drawing;
	using System.Xml.Linq;
	using Visuals;
    using Maths;
	using Reactive;
	using UI;

	public abstract class AnySignalEditor
	{
		private Connected _control;
		internal int _level;
        internal bool _updated;

        protected AnySignalEditor ()
        {
            RenderToBuffer = (editor, size) => editor.MapSignal ()
                .MapInput (Signal.BitmapCoordToUnitRange (size, 1f))
                .SampleToBuffer (Buffer, size);
        }

        private Signal<Vec2, uint> MapSignal ()
        {
            return
                this is SignalEditor<Vec2, Vec3> ?
                    ((SignalEditor<Vec2, Vec3>)this).Signal
                    .Vec3ToUintColor () :
                this is SignalEditor<Vec2, float> ?
                    ((SignalEditor<Vec2, float>)this).Signal
                    .Scale (0.5f)
                    .Offset (0.5f)
                    .FloatToUintGrayscale () :
                null;
        }

        protected Control InputSignalControl (string name, AnySignalEditor input)
		{
			return new Connector (Container.Frame (Label.Static (name)), input.Control,
				VisualDirection.Horizontal, HAlign.Left, VAlign.Center, ConnectorKind.Curved,
				new VisualStyle (pen: new Pen (Color.AliceBlue, 2f)));
		}

        internal void Render (Vec2i size)
        {
            if (_updated)
                return;
            var length = size.Producti ();
            if (Buffer == null || Buffer.Length != length)
                Buffer = new uint[length];
            RenderToBuffer (this, size);
            _updated = true;
        }

        private string XElementName ()
		{
			var result = GetType ().Name;
			var i = result.IndexOf ('`');
			if (i > 0)
				result = result.Substring (0, i);
			return result;
		}

		internal XElement SaveToXml ()
		{
			var result = new XElement (XElementName (),
				new XAttribute ("Name", Name));
			Save (result);
			return result;
		}

		internal void LoadFromXml (XElement xml)
		{
			Load (xml.Elements (XElementName ()).Single (xe => xe.Attribute ("Name").Value == Name));
		}

		protected abstract Control CreateControl ();

		protected virtual void Load (XElement xelem) { }
		protected virtual void Save (XElement xelem) { }

        public string Name { get; internal set; }

        public Connected Control
        {
            get
            {
                if (_control == null)
                    _control = new Connected (CreateControl (), HAlign.Right, VAlign.Center);
                return _control;
            }
        }

        public uint[] Buffer { get; private set; }

        public abstract IEnumerable<AnySignalEditor> Inputs { get; }

        public Reaction<AnySignalEditor> Changed { get; internal set; }

        public Action<AnySignalEditor, Vec2i> RenderToBuffer { get; set; }
	}
}
