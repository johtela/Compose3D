	namespace Compose3D.Imaging.SignalEditors
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Drawing;
	using System.Xml.Linq;
	using Extensions;
	using Imaging;
	using Visuals;
	using Reactive;
	using Textures;
	using UI;

	public abstract class AnySignalEditor
	{
		private Connected _control;
		internal int _level;
		internal uint[] _buffer;
		internal Texture _texture;

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

		protected Control InputSignalControl (string name, AnySignalEditor input)
		{
			return new Connector (Container.Frame (Label.Static (name)), input.Control,
				VisualDirection.Horizontal, HAlign.Left, VAlign.Center, ConnectorKind.Curved,
				new VisualStyle (pen: new Pen (Color.AliceBlue, 2f)));
		}

		protected string MethodSignature (string instance, string method, params object[] args)
		{
			return string.Format ("{0}.{1} ({2})", instance, method,
				args.Select (CodeGen.ToCode).SeparateWith (", "));
		}

		internal string InitializationCode ()
		{
			return string.Format ("var {0} = {1};\n", Name, ToCode ());
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
		protected abstract string ToCode ();

		protected virtual void Load (XElement xelem) { }
		protected virtual void Save (XElement xelem) { }

		public abstract IEnumerable<AnySignalEditor> Inputs { get; }
		public Reaction<AnySignalEditor> Changed { get; internal set; }
	}
}
