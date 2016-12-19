namespace Compose3D.CLTypes
{
	using Cloo;

	public abstract class CLArgument<T>
	{
		protected ComputeKernel _clKernel;
		protected int _index;

		public CLArgument (ComputeKernel kernel, int index)
		{
			_clKernel = kernel;
			_index = index;
		}
	}

	public class CLInput<T> : CLArgument<T>
		where T : struct
	{
		private T _value;

		public CLInput (ComputeKernel kernel, int index)
			: base (kernel, index) { }

		public static CLInput<T> operator & (CLInput<T> input, T value)
		{
			input._value = value;
			input._clKernel.SetValueArgument<T> (input._index, value);
			return input;
		}

		[CLUnaryOperator ("{0}")]
		public static T operator ! (CLInput<T> input)
		{
			return input._value;
		}
	}

	public class CLBuffer<T> : CLArgument<ComputeBuffer<T>>
		where T : struct
	{
		private ComputeBuffer<T> _clBuffer;

		public CLBuffer (ComputeKernel kernel, int index)
			: base (kernel, index) { }

		public static CLBuffer<T> operator & (CLBuffer<T> buffer, ComputeBuffer<T> value)
		{
			buffer._clBuffer = value;
			buffer._clKernel.SetMemoryArgument (buffer._index, value);
			return buffer;
		}

		[CLUnaryOperator ("{0}")]
		public static T[] operator ! (CLBuffer<T> buffer)
		{
			return new T[buffer._clBuffer.Count];
		}
	}
}