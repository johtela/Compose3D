namespace ComposeFX.Graphics.Reactive
{
	using System;
	using System.Collections.Generic;
	using OpenTK;
    using ComposeFX.Reactive;

	public class DelayedReactionUpdater
	{
		internal class PendingAction
		{
			public double Elapsed;
			public double Delay;
			public Action Action;
		}

		internal Dictionary<object, PendingAction> _pending;

		public DelayedReactionUpdater (GameWindow window)
		{
			_pending = new Dictionary<object, PendingAction> ();
			window.UpdateFrame += Update;
		}

		private void Update (object sender, FrameEventArgs args)
		{
			var time = args.Time;
			var deleted = new List<object> ();
			foreach (var pair in _pending)
			{
				pair.Value.Elapsed += time;
				if (pair.Value.Elapsed > pair.Value.Delay)
				{
					pair.Value.Action ();
					deleted.Add (pair.Key);
				}
			}
			foreach (var item in deleted)
				_pending.Remove (item);
		}
	}

	public static class Delayed
	{
		public static Reaction<T> Delay<T> (this Reaction<T> reaction, DelayedReactionUpdater updater, 
			double delay)
		{
			return React.By<T> (x =>
			{
				DelayedReactionUpdater.PendingAction pa;
				if (updater._pending.TryGetValue (reaction, out pa))
					pa.Elapsed = 0;
				else
					updater._pending.Add (reaction, new DelayedReactionUpdater.PendingAction ()
					{
						Delay = delay,
						Action = () => reaction (x)
					});
			});
		}
	}
}