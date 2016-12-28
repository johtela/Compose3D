namespace Compose3D.Compiler
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	internal class Function : IEquatable<Function>
	{
		public readonly string Name;
		public readonly string Declarations;
		public readonly string Code;
		public readonly int[] FuncParams;
		public readonly List<Invocation> Invocations;

		internal int instanceCount;

		public Function (string name, string decls, string code, List<Invocation> invocations,
			params int[] funcParams)
		{
			Name = name;
			Declarations = decls;
			Code = code;
			FuncParams = funcParams;
			Invocations = invocations;
		}

		public override bool Equals (object obj)
		{
			var other = obj as Function;
			return other != null && Equals (other);
		}

		public override int GetHashCode ()
		{
			return Name.GetHashCode ();
		}

		#region IEquatable implementation

		public bool Equals (Function other)
		{
			return Name.Equals (other.Name);
		}

		#endregion
	}
}