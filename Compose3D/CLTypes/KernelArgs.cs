namespace Compose3D.CLTypes
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Maths;
	using Cloo;

	public abstract class KernelArg
	{
		public abstract int PushToCLKernel (CLKernel clKernel, int index);

		public virtual void ReadResult (CLCommandQueue queue) { }

		public static Value<T> Value<T> (T data)
			where T : struct
		{
			return new Value<T> (data); 
		}

		public static Buffer<T> Buffer<T> (T[] data, ComputeMemoryFlags flags)
			where T : struct
		{
			return new Buffer<T> (data, flags);
		}

		public static Buffer<T> ReadBuffer<T> (T[] data)
			where T : struct
		{
			return new Buffer<T> (data, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer);
		}

		public static Buffer<T> WriteBuffer<T> (T[] data)
			where T : struct
		{
			return new Buffer<T> (data, ComputeMemoryFlags.WriteOnly);
		}
	}

	public class Value<T> : KernelArg
		where T : struct
	{
		private T _data;

		public Value (T data)
		{
			_data = data;
		}

		public override int PushToCLKernel (CLKernel clKernel, int index)
		{
			clKernel._comKernel.SetValueArgument (index, _data);
			return index + 1;
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

		public override int PushToCLKernel (CLKernel clKernel, int index)
		{
			if (_comBuffer == null)
				_comBuffer = (_flags & ComputeMemoryFlags.WriteOnly) != 0 ?
					new ComputeBuffer<T> (clKernel._comKernel.Context, _flags, _data.Length) :
					new ComputeBuffer<T> (clKernel._comKernel.Context, _flags, _data);
			clKernel._comKernel.SetMemoryArgument (index, _comBuffer);
			return index + 1;
		}

		public override void ReadResult (CLCommandQueue queue)
		{
			if ((_flags & ComputeMemoryFlags.ReadOnly) == 0)
				queue._comQueue.ReadFromBuffer (_comBuffer, ref _data, false, null);
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
		public override int PushToCLKernel (CLKernel clKernel, int index)
		{
			throw new NotImplementedException ();
		}
	}

	public abstract class ArgGroup : KernelArg
	{
		private static Dictionary<Type, Tuple<string, Type>[]> _memberDefinitions =
			new Dictionary<Type, Tuple<string, Type>[]> ();
		private KernelArg[] _members;

		public static IEnumerable<Tuple<string, Type>> MemberDefinitions (Type type, string parName)
		{
			Tuple<string, Type>[] result;
			if (!_memberDefinitions.TryGetValue (type, out result))
			{
				result = GetMemberDefinitions (type, parName).ToArray ();
				_memberDefinitions.Add (type, result);
			}
			return result;
		}

		private static IEnumerable<Tuple<string, Type>> GetMemberDefinitions (Type type, string baseName)
		{
			var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
			foreach (var fld in type.GetFields (bindingFlags).Where (
				f => f.FieldType.IsSubclassOf (typeof (KernelArg))))
				if (fld.FieldType.IsSubclassOf (typeof (ArgGroup)))
					foreach (var subArg in GetMemberDefinitions (fld.FieldType, baseName + fld.Name + "_"))
						yield return subArg;
				else
					yield return Tuple.Create (baseName + fld.Name, fld.FieldType);
			foreach (var prop in type.GetProperties (bindingFlags).Where (
				p => p.PropertyType.IsSubclassOf (typeof (KernelArg))))
				if (prop.PropertyType.IsSubclassOf (typeof (ArgGroup)))
					foreach (var subArg in GetMemberDefinitions (prop.PropertyType, baseName + prop.Name + "_"))
						yield return subArg;
				else
					yield return Tuple.Create (baseName + prop.Name, prop.PropertyType);
		}

		public KernelArg[] Members
		{
			get
			{
				if (_members == null)
				{
					var type = GetType ();
					var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
					var fields =
						from fld in type.GetFields (bindingFlags)
						where fld.FieldType.IsSubclassOf (typeof (KernelArg))
						select fld.GetValue (this) as KernelArg;
					var properties =
						from prop in type.GetProperties (bindingFlags)
						where prop.PropertyType.IsSubclassOf (typeof (KernelArg))
						select prop.GetValue (this) as KernelArg;
					_members = fields.Concat (properties).ToArray ();
				}
				return _members;
			}
		}

		public override int PushToCLKernel (CLKernel clKernel, int index)
		{
			foreach (var arg in Members)
				index = arg.PushToCLKernel (clKernel, index);
			return index;
		}

		public override void ReadResult (CLCommandQueue queue)
		{
			foreach (var arg in Members)
				arg.ReadResult (queue);
		}
	}

	public class Assign
	{
		private Assign () {	}

		public static Assign Buffer<T> (Buffer<T> buffer, int index, T value)
			where T : struct
		{
			return new Assign ();
		}

		public static Assign Image<T, V> (Image<T> image, V coord, T value)
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
}
