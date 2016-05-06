namespace Compose3D.GLTypes
{
	using System;
	using System.Collections.Generic;

	public abstract class GLObject
	{
		private class UsingScope : IDisposable
		{
			private GLObject _glObject;

			public UsingScope (GLObject glObject)
			{
				_glObject = glObject;
				_glObject.Use ();
			}

			public void Dispose ()
			{
				_glObject.Release ();
			}
		}

		public IDisposable Scope ()
		{
			return new UsingScope (this);
		}

		public abstract void Use ();
		public abstract void Release ();
	}
}