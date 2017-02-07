namespace Compose3D.Parallel
{
	using CLTypes;
	using Extensions;

	public abstract class KernelArg { }

	public class Value<T> : KernelArg
	{
		private T _value = default (T);

		[CLUnaryOperator ("{0}")]
		public static T operator ! (Value<T> value)
		{
			return value._value;
		}
	}

	public class Buffer<T> : KernelArg
	{
		private T[] _value = null;

		[CLUnaryOperator ("{0}")]
		public static T[] operator ! (Buffer<T> value)
		{
			return value._value;
		}
	}

	public class Image<T> : KernelArg { }

	public class KernelResult<T> : Params<int, T> { }
}
