namespace Compose3D.Imaging.SignalEditors
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Drawing;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using OpenTK.Graphics.OpenGL4;
	using Visuals;
	using CLTypes;
    using Maths;
	using Reactive;
	using Textures;
	using UI;

	public abstract class AnySignalEditor
	{
		private static HashSet<CLKernel> _kernels = new HashSet<CLKernel> ();
		protected static CLContext _context;
		protected static CLCommandQueue _queue;
		protected static CLProgram _program;

		private Connected _control;
		internal int _level;

		public AnySignalEditor (CLKernel kernel, Texture texture)
		{
			if (kernel != null && !_kernels.Contains (kernel))
				_kernels.Add (kernel);
			Texture = texture ?? new Texture (TextureTarget.Texture2D);
		}

        protected Control InputSignalControl (string name, AnySignalEditor input)
		{
			return new Connector (Container.Frame (Label.Static (name)), input.Control,
				VisualDirection.Horizontal, HAlign.Left, VAlign.Center, ConnectorKind.Curved,
				new VisualStyle (pen: new Pen (Color.AliceBlue, 2f)));
		}

		private void SetupCLProgram ()
		{
			if (_context != null || _kernels.Count == 0)
				return;
			_context = CLContext.CreateContextForDevices (CLContext.Gpus.First ());
			_queue = new CLCommandQueue (_context);
			_program = new CLProgram (_context, _kernels.ToArray ());
		}

		internal void Render (Vec2i size)
        {
			SetupCLProgram ();
            var length = size.Producti ();
			AllocateBuffer (length);
            RenderToBuffer (size).ContinueWith (_ => UpdateTexture (size), 
				TaskScheduler.FromCurrentSynchronizationContext ());
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

		protected abstract void AllocateBuffer (int length);
		protected abstract Task RenderToBuffer (Vec2i size);
		protected abstract void UpdateTexture (Vec2i size);
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

        public abstract IEnumerable<AnySignalEditor> Inputs { get; }

		public Texture Texture { get; private set; }

        public Reaction<AnySignalEditor> Changed { get; internal set; }
	}
}
