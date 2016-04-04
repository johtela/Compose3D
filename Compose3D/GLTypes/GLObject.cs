namespace Compose3D.GLTypes
{
	using System;
	using System.Collections.Generic;

	public abstract class GLObject
	{
		private UsingScope _scope;
		private static HashSet<Type> _activeScopes = new HashSet<Type> ();

		private class UsingScope : IDisposable
		{
			private GLObject _glObject;
			private int _refCount;

			public UsingScope (GLObject glObject)
			{
				_glObject = glObject;
				_glObject._scope = this;
				AddRef ();
				_glObject.Use ();
			}

			public void AddRef ()
			{
				_refCount++;
			}

			public void Dispose ()
			{
				if (--_refCount == 0)
				{
					_glObject.Release ();
					_glObject._scope = null;
					_activeScopes.Remove (_glObject.GetType ());
				}
			}
		}

		public IDisposable Scope ()
		{
			if (_scope != null)
			{
				_scope.AddRef ();
				return _scope;
			}
			if (_activeScopes.Contains (GetType ()))
				throw new InvalidOperationException ("An active scope already in effect for another object of this type.");
			_activeScopes.Add (GetType ());
			return new UsingScope (this);
		}

		public abstract void Use ();
		public abstract void Release ();
	}
}