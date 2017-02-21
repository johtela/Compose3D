namespace Compose3D.Parallel
{
	using System.Collections.Generic;
	using CLTypes;
	using Maths;
	using Extensions;
	using Cloo;
	using System;

	public abstract class KernelArg
	{
		public abstract void PushToCLKernel (CLKernel clKernel, int index);
	}

	public class Value<T> : KernelArg
		where T : struct
	{
		private T _data;

		public Value (T data)
		{
			_data = data;
		}

		public override void PushToCLKernel (CLKernel clKernel, int index)
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
			_flags = flags;
		}

		public override void PushToCLKernel (CLKernel clKernel, int index)
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

	public class Image<T> : KernelArg
	{
		public override void PushToCLKernel (CLKernel clKernel, int index)
		{
			throw new NotImplementedException ();
		}
	}

	public class Assign
	{
		private Assign () {	}

		public Assign Buffer<T> (Buffer<T> buffer, int index, T value)
			where T : struct
		{
			return new Assign ();
		}

		public Assign Image<T, V> (Image<T> image, V coord, T value)
			where T : struct
			where V : struct, IVec<V, int>
		{
			return new Assign ();
		}
	}

	public class KernelResult : IEnumerable<Assign>
	{
		private List<Assign> _assignments;

		public KernelResult ()
		{
			_assignments = new List<Assign> ();
		}

		public void Add (Assign assignment)
		{
			_assignments.Add (assignment);
		}

		public IEnumerator<Assign> GetEnumerator ()
		{
			return _assignments.GetEnumerator ();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}

	public class BufferResult<TRes> : Params<int, TRes>
		where TRes : struct
	{ }

	public class ImageResult<TVec, TRes> : Params<TVec, TRes>
		where TVec: struct, IVec<TVec, int>
		where TRes : struct
	{ }
}
