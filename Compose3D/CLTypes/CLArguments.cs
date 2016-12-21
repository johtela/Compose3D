namespace Compose3D.CLTypes
{
	using System;
	using System.Collections.Generic;
	using Cloo;

	public enum CLArgumentKind { Value, Buffer }

	public class CLArguments
	{
		private class Arg
		{
			public readonly string Name;
			public readonly Type Type;
			public readonly CLArgumentKind Kind;

			public Arg (string name, Type type, CLArgumentKind kind)
			{
				Name = name;
				Type = type;
				Kind = kind;
			}
		}

		private List<Arg> _arguments;
		internal CLProgram _program;

		public CLArguments ()
		{
			_arguments = new List<Arg> ();
		}

		internal void Add (string name, Type type, CLArgumentKind kind)
		{
			_arguments.Add (new Arg (name, type, kind));
		}

		public int Count
		{
			get { return _arguments.Count; }
		}

		public string ArgumentName (int index)
		{
			return _arguments[index].Name;
		}

		public Type ArgumentType (int index)
		{
			return _arguments[index].Type;
		}

		public CLArgumentKind ArgumentKind (int index)
		{
			return _arguments[index].Kind;
		}

		private void CheckArgType<T> (int index, CLArgumentKind kind) where T : struct
		{
			var arg = _arguments[index];
			if (arg.Kind != kind)
				throw new ArgumentException (
					string.Format ("Wrong kind of argument for index {0}. Expected {1} got {2}.",
						index, arg.Kind, kind));
			if (arg.Type != typeof (T))
				throw new ArgumentException (
					string.Format ("Wrong type {0} for argument index {1}. Expected {2}.",
						typeof (T), index, arg.Type));
		}

		public int IndexOfArgument (string name)
		{
			var i = _arguments.FindIndex (arg => arg.Name == name);
			if (i < 0)
				throw new ArgumentException (string.Format ("Argument '{0}' not found.", name));
			return i;
		}

		public void Set<T> (int index, T value)
			where T : struct
		{
			CheckArgType<T> (index, CLArgumentKind.Value);
			_program._comKernel.SetValueArgument (index, value);
		}

		public void Set<T> (int index, ComputeBuffer<T> buffer)
			where T : struct
		{
			CheckArgType<T> (index, CLArgumentKind.Buffer);
			_program._comKernel.SetMemoryArgument (index, buffer);
		}

		public void Set<T> (CLProgram clKernel, string name, T value)
			where T : struct
		{
			Set (IndexOfArgument (name), value);
		}

		public void Set<T> (CLProgram clKernel, string name, ComputeBuffer<T> buffer)
			where T : struct
		{
			Set (IndexOfArgument (name), buffer);
		}
	}
}