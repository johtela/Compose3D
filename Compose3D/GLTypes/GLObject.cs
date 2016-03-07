namespace Compose3D.GLTypes
{
	using System;

	public abstract class GLObject
	{
		private UsingScope _scope;

		private class UsingScope : IDisposable
		{
			private GLObject _glObject;

			public UsingScope (GLObject glObject)
			{
				_glObject = glObject;
				_glObject._scope = this;
				_glObject.Use ();
			}

			public void Dispose ()
			{
				_glObject.Release ();
				_glObject._scope = null;
			}
		}

		public IDisposable Scope ()
		{
			if (_scope != null)
				throw new InvalidOperationException ("Scope already active for this GL object. "+ 
					"Only one scope can be in effect at any time.");
			return new UsingScope (this);
		}

		public abstract void Use ();
		public abstract void Release ();
	}
}