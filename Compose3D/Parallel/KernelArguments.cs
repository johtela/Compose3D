namespace Compose3D.Parallel
{
    using System;
    using System.Collections;
    using System.Collections.Generic; 
    using Cloo;
    using CLTypes;

    public enum KernelArgumentKind { Value, Buffer }
    public enum KernelArgumentAccess { Read, Write }

    public class KernelArgument
    {
        public readonly string Name;
        public readonly Type Type;
        public readonly KernelArgumentKind Kind;
        public readonly KernelArgumentAccess Access;

        public KernelArgument (string name, Type type, KernelArgumentKind kind,
            KernelArgumentAccess access)
        {
            Name = name;
            Type = type;
            Kind = kind;
            Access = access;
        }
    }

    public class KernelArguments : IEnumerable<KernelArgument>
	{
		private List<KernelArgument> _arguments;
		internal CLProgram _program;

		public KernelArguments ()
		{
			_arguments = new List<KernelArgument> ();
		}

		internal void Add (string name, Type type, KernelArgumentKind kind,
            KernelArgumentAccess access)
		{
			_arguments.Add (new KernelArgument (name, type, kind, access));
		}

		public int Count
		{
			get { return _arguments.Count; }
		}

        public KernelArgument this [int index]
        {
            get { return _arguments[index]; }
        }

		private void CheckArgType<T> (int index, KernelArgumentKind kind) where T : struct
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
			CheckArgType<T> (index, KernelArgumentKind.Value);
			_program._comKernel.SetValueArgument (index, value);
		}

		public void Set<T> (int index, ComputeBuffer<T> buffer)
			where T : struct
		{
			CheckArgType<T> (index, KernelArgumentKind.Buffer);
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

        public IEnumerator<KernelArgument> GetEnumerator ()
        {
            return _arguments.GetEnumerator ();
        }

        IEnumerator IEnumerable.GetEnumerator ()
        {
            return GetEnumerator ();
        }
    }
}