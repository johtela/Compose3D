namespace Compose3D.Parallel
{
	using System;
	using Extensions;
	using Compiler;
	using CLTypes;

	public delegate T Kernel<T> ();

	public abstract class KernelArg { }
	public class Value<T> : KernelArg { }
	public class Buffer<T> : KernelArg { }
	public class Image<T> : KernelArg { }

	public class KernelResult<T> : Params<int, T> { }

	public static class Kernel
	{
		[LiftMethod]
		public static Kernel<T> ToKernel<T> (this T value)
		{
			return () => value;
		}

		public static Kernel<U> Bind<T, U> (this Kernel<T> kernel, Func<T, Kernel<U>> func)
		{
			return () => func (kernel ()) ();
		}

		public static T Evaluate<T> (this Kernel<T> kernel)
		{
			return kernel ();
		}

		[LiftMethod]
		public static Kernel<T> Argument<T> ()
			where T : struct
		{
			return () => default (T);
		}

		[LiftMethod]
		public static Kernel<T[]> Buffer<T> ()
			where T : struct
		{
			return () => new T[0];
		}

		[LiftMethod]
		public static Kernel<T> Constants<T> (T constants)
		{
			return () => constants;
		}

		[CLFunction ("get_global_id ({0})")]
		public static int GetGlobalId (int dimension)
		{
			return 0;
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
			return () =>
			{
				var res = kernel ();
				return predicate (res) ? res : default (T);
			};
		}
	}
}