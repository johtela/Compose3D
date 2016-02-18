namespace Compose3D.Reactive
{
	using System;
	using Extensions;

	public delegate void Reaction<T> (T input);

	public static class React
	{
		public static Reaction<T> By<T> (Action<T> action)
		{
			return new Reaction<T> (action);
		}

		public static Reaction<Tuple<T, U>> By<T, U> (Action<T, U> action)
		{
			return input => action (input.Item1, input.Item2);
		}

		public static Reaction<Tuple<T, U, V>> By<T, U, V> (Action<T, U, V> action)
		{
			return input => action (input.Item1, input.Item2, input.Item3);
		}

		public static Reaction<Tuple<T, U, V, W>> By<T, U, V, W> (Action<T, U, V, W> action)
		{
			return input => action (input.Item1, input.Item2, input.Item3, input.Item4);
		}

		public static Reaction<T> Map<T, U> (this Reaction<U> reaction, Func<T, U> func)
		{
			return input => reaction (func (input));
		}

		public static Reaction<T> Map<T> (this Reaction<T> reaction, Func<T, T> func)
		{
			return input => reaction (func (input));
		}

		public static Reaction<T> Filter<T> (this Reaction<T> reaction, Func<T, bool> predicate)
		{
			return input =>
			{
				if (predicate (input))
					reaction (input);
			};
		}

		public static Reaction<T> Once<T> (this Reaction<T> reaction)
		{
			bool occurred = false;
			return input =>
			{
				if (!occurred)
				{
					reaction (input);
					occurred = true;
				}			
			};
		}

		public static Reaction<T> Merge<T> (params Reaction<T>[] reactions)
		{
			return input =>
			{
				foreach (var r in reactions)
					r (input);
			};
		}

		public static Tuple<Reaction<T>, Reaction<U>> Any<T, U> (this Reaction<Tuple<T, U>> reaction)
		{
			return Tuple.Create<Reaction<T>, Reaction<U>> (
				input1 => reaction (Tuple.Create (input1, default (U))),
				input2 => reaction (Tuple.Create (default (T), input2)));
		}

		public static Tuple<Reaction<T>, Reaction<U>, Reaction<V>> Any<T, U, V> (
			this Reaction<Tuple<T, U, V>> reaction)
		{
			return Tuple.Create<Reaction<T>, Reaction<U>, Reaction<V>> (
				input1 => reaction (Tuple.Create (input1, default (U), default (V))),
				input2 => reaction (Tuple.Create (default (T), input2, default (V))),
				input3 => reaction (Tuple.Create (default (T), default (U), input3)));
		}

		public static Tuple<Reaction<T>, Reaction<U>, Reaction<V>, Reaction<W>> Any<T, U, V, W> (
			this Reaction<Tuple<T, U, V, W>> reaction)
		{
			return Tuple.Create<Reaction<T>, Reaction<U>, Reaction<V>, Reaction<W>> (
				input1 => reaction (Tuple.Create (input1, default (U), default (V), default (W))),
				input2 => reaction (Tuple.Create (default (T), input2, default (V), default (W))),
				input3 => reaction (Tuple.Create (default (T), default (U), input3, default (W))),
				input4 => reaction (Tuple.Create (default (T), default (U), default (V), input4)));
		}

		public static Tuple<Reaction<T>, Reaction<U>> All<T, U> (this Reaction<Tuple<T, U>> reaction)
		{
			var value = TupleExt.EmptyTuple<T, U> ();
			Action invoke = () =>
			{
				if (value.HasValues ())
				{
					reaction (value);
					value = TupleExt.EmptyTuple<T, U> ();
				}
			};
			return Tuple.Create<Reaction<T>, Reaction<U>> (
				input1 => { value = Tuple.Create (input1, value.Item2); invoke (); },
				input2 => { value = Tuple.Create (value.Item1, input2); invoke (); }
			);
		}

		public static Tuple<Reaction<T>, Reaction<U>, Reaction<V>> All<T, U, V> (this Reaction<Tuple<T, U, V>> reaction)
		{
			var value = TupleExt.EmptyTuple<T, U, V> ();
			Action invoke = () =>
			{
				if (value.HasValues ())
				{
					reaction (value);
					value = TupleExt.EmptyTuple<T, U, V> ();
				}
			};
			return Tuple.Create<Reaction<T>, Reaction<U>, Reaction<V>> (
				input1 => { value = Tuple.Create (input1, value.Item2, value.Item3); invoke (); },
				input2 => { value = Tuple.Create (value.Item1, input2, value.Item3); invoke (); },
				input3 => { value = Tuple.Create (value.Item1, value.Item2, input3); invoke (); }
			);
		}

		public static Tuple<Reaction<T>, Reaction<U>, Reaction<V>, Reaction<W>> All<T, U, V, W> (
			this Reaction<Tuple<T, U, V, W>> reaction)
		{
			var value = TupleExt.EmptyTuple<T, U, V, W> ();
			Action invoke = () =>
			{
				if (value.HasValues ())
				{
					reaction (value);
					value = TupleExt.EmptyTuple<T, U, V, W> ();
				}
			};
			return Tuple.Create<Reaction<T>, Reaction<U>, Reaction<V>, Reaction<W>> (
				input1 => { value = Tuple.Create (input1, value.Item2, value.Item3, value.Item4); invoke (); },
				input2 => { value = Tuple.Create (value.Item1, input2, value.Item3, value.Item4); invoke (); },
				input3 => { value = Tuple.Create (value.Item1, value.Item2, input3, value.Item4); invoke (); },
				input4 => { value = Tuple.Create (value.Item1, value.Item2, value.Item3, input4); invoke (); }
			);
		}

		public static Tuple<Reaction<T>, Reaction<U>> InSequence<T, U> (Reaction<T> first, Reaction<U> second, bool repeat)
		{
			int phase = 1;
			return Tuple.Create<Reaction<T>, Reaction<U>> (
				input1 =>
				{
					if (phase == 1)
						first (input1);
				},
				input2 =>
				{
					if (phase.In (1, 2))
					{
						phase = repeat ? 1 : 2;
						second (input2);
					}
				}
			);
		}

		public static Tuple<Reaction<T>, Reaction<U>, Reaction<V>> InSequence<T, U, V> (
			Reaction<T> first, Reaction<U> second, Reaction<V> third, bool repeat)
		{
			int phase = 1;
			return Tuple.Create<Reaction<T>, Reaction<U>, Reaction<V>> (
				input1 =>
				{
					if (phase == 1)
						first (input1);
				},
				input2 =>
				{
					if (phase.In (1, 2))
					{
						phase = 2;
						second (input2);
					}
				},
				input3 =>
				{
					if (phase.In (2, 3))
					{
						phase = repeat ? 1 : 3;
						third (input3);
					}
				}
			);
		}

		public static Tuple<Reaction<T>, Reaction<U>, Reaction<V>, Reaction<W>> InSequence<T, U, V, W> (
			Reaction<T> first, Reaction<U> second, Reaction<V> third, Reaction<W> fourth, bool repeat)
		{
			int phase = 1;
			return Tuple.Create<Reaction<T>, Reaction<U>, Reaction<V>, Reaction<W>> (
				input1 =>
				{
					if (phase == 1)
						first (input1);
				},
				input2 =>
				{
					if (phase.In (1, 2))
					{
						phase = 2;
						second (input2);
					}
				},
				input3 =>
				{
					if (phase.In (2, 3))
					{
						phase = 3;
						third (input3);
					}
				},
				input4 =>
				{
					if (phase.In (3, 4))
					{
						phase = repeat ? 1 : 4;
						fourth (input4);
					}
				}
			);
		}

		public static Reaction<U> Fold<T, U> (this Reaction<T> reaction, Func<T, U, T> func, T initial)
		{
			var current = initial;
			return input =>
			{
				current = func (current, input);
				reaction (current);
			};
		}

		public static EventHandler<T> ToEvent<T> (this Reaction<T> reaction) where T : EventArgs
		{
			return (sender, e) => reaction (e);
		}
	}
}

