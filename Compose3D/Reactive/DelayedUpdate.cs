namespace Compose3D.Reactive
{
	using System;

	public class DelayedUpdate<T>
	{
		public readonly Action<T> Update;
		public readonly T Value;

		private double _elapsed;
		private double _delay;

		public DelayedUpdate (T value, Action<T> update, double delay)
		{
			Value = value;
			Update = update;
			_delay = delay;
		}

		public Reaction<double> Run ()
		{
			return React.By ((double t) =>
			{
				_elapsed += t;
				if (_elapsed > _delay)
				{
					Update (Value);
					_elapsed = double.NegativeInfinity;
				}
			});
		}

		public void Changed ()
		{
			_elapsed = 0;
		}
	}

	public static class Delayed
	{
		public static DelayedUpdate<T> Update<T> (T value, Action<T> update, double delay)
		{
			return new DelayedUpdate<T> (value, update, delay);
		}
	}
}