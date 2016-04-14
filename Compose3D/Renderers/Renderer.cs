namespace Compose3D.Renderers
{
	using System;
	using System.Linq;

	public struct Renderer<T>
	{
		private Action<T> _action;

		public Renderer (Action<T> action)
		{
			_action = action;
		}

		public static Renderer<T> operator > (Renderer<T> first, Renderer<T> second)
		{
			return new Renderer<T> (a =>
			{
				first._action (a);
				second._action (a);
			});
		}

		public static Renderer<T> operator < (Renderer<T> second, Renderer<T> first)
		{
			return new Renderer<T> (a =>
			{
				first._action (a);
				second._action (a);
			});
		}

		public static Renderer<T> operator | (Renderer<T> first, Func<T, bool> condition)
		{
			return new Renderer<T> (a =>
			{
				if (condition (a)) first._action (a);
			});
		}
	}

	public static class Renderer
	{ 
		public static Renderer<T> Do<T> (Action<T> action)
		{
			return new Renderer<T> (action);
		}

		public static void Test ()
		{
			var foo = Renderer.Do<int> (a => Console.WriteLine ()) >
				Renderer.Do<int> (b => Console.WriteLine ()) | (i => i < 2);
		}
	}
}
