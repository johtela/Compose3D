namespace Compose3D.Parallel
{
	using CLTypes;
	using Extensions;
	using Cloo;
	using System;

	public abstract class KernelArg
	{
		public abstract void Initialize (CLKernel clKernel, int index);
	}

	public class Value<T> : KernelArg
		where T : struct
	{
		private T _data;

		public Value (T data)
		{
			_data = data;
		}

		public override void Initialize (CLKernel clKernel, int index)
		{
			clKernel._comKernel.SetValueArgument (index, _data);
		}

		[CLUnaryOperator ("{0}")]
		public static T operator ! (Value<T> value)
		{
			return value._data;
		}

		public static Value<T> operator & (Value<T> value, T data)
		{
			value._data = data;
			return value;
		}
	}

	public class Buffer<T> : KernelArg
		where T : struct
	{
		private ComputeMemoryFlags _flags;
		internal T[] _data;
		internal ComputeBuffer<T> _comBuffer;

		public Buffer (T[] data, ComputeMemoryFlags flags)
		{
			_data = data;
		}

		public override void Initialize (CLKernel clKernel, int index)
		{
			if (_comBuffer == null)
				_comBuffer = (_flags | ComputeMemoryFlags.WriteOnly) != 0 ?
					new ComputeBuffer<T> (clKernel._comKernel.Context, _flags, _data.Length) :
					new ComputeBuffer<T> (clKernel._comKernel.Context, _flags, _data);
			clKernel._comKernel.SetMemoryArgument (index, _comBuffer);
		}

		[CLUnaryOperator ("{0}")]
		public static T[] operator ! (Buffer<T> buffer)
		{
			return buffer._data;
		}

		public static Buffer<T> operator & (Buffer<T> buffer, T[] data)
		{
			buffer._data = data;
			return buffer;
		}
	}

	//public class Image<T> : KernelArg { }

	public class KernelResult<T> : Params<int, T> 
		where T : struct { }
}
