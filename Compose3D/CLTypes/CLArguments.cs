namespace Compose3D.CLTypes
{
	using System;
	using System.Collections.Generic;
	using Cloo;

	public class CLArguments
	{
		private class Arg
		{
			public readonly string Name;
			public readonly Type Type;

			public Arg (string name, Type type)
			{
				Name = name;
				Type = type;
			}
		}

		private List<Arg> _arguments;
		internal CLProgram _program;

		public CLArguments ()
		{
			_arguments = new List<Arg> ();
		}

		internal void Add (string name, Type type)
		{
			_arguments.Add (new Arg (name, type));
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

		private void CheckArgType<T> (int index) where T : struct
		{
			var arg = _arguments[index];
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
			CheckArgType<T> (index);
			_program._comKernel.SetValueArgument (index, value);
		}

		public void Set<T> (int index, ComputeBuffer<T> buffer)
			where T : struct
		{
			CheckArgType<T> (index);
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