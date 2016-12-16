namespace Compose3D.Parallel
{
	using System;
	using Compiler;
	using CLTypes;

	public class KernelState
	{
		[CLFunction ("get_global_id ()")]
		public int GetGlobalId () { return 0; }
	}

	public delegate T Kernel<T> (KernelState state);

	public static class Kernel
	{
		[LiftMethod]
		public static Kernel<T> ToKernel<T> (this T value)
		{
			return state => value;
		}

		public static Kernel<U> Bind<T, U> (this Kernel<T> kernel, Func<T, Kernel<U>> func)
		{
			return state => func (kernel (state)) (state);
		}

		public static T Execute<T> (this Kernel<T> kernel, KernelState state)
		{
			return kernel (state);
		}

		public static T Evaluate<T> (this Kernel<T> kernel)
		{
			return kernel (new KernelState ());
		}

		[LiftMethod]
		public static Kernel<T[]> Buffer<T> ()
		{
			return state => new T[0];
		}

		[LiftMethod]
		public static Kernel<T> Parameter<T> ()
		{
			return state => default (T);
		}

		[LiftMethod]
		public static Kernel<T> Constants<T> (T constants)
		{
			return state => constants;
		}

		[LiftMethod]
		public static Kernel<KernelState> State ()
		{
			return state => state;
		}

		public static Kernel<U> Select<T, U> (this Kernel<T> kernel, Func<T, U> select)
		{
			return kernel.Bind (a => select (a).ToKernel ());
		}

		public static Kernel<V> SelectMany<T, U, V> (this Kernel<T> kernel,
			Func<T, Kernel<U>> project, Func<T, U, V> select)
		{
			return kernel.Bind (a => project (a).Bind (b => select (a, b).ToKernel ()));
		}

		public static Kernel<T> Where<T> (this Kernel<T> kernel, Func<T, bool> predicate)
		{
			return state =>
			{
				var res = kernel (state);
				return predicate (res) ? res : default (T);
			};
		}
	}
}