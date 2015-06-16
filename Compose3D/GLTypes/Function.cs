namespace Compose3D.GLTypes
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Text;

	internal class Function : IEquatable<Function>
	{
		public readonly MemberInfo Member;
		public readonly string Code;
		public readonly HashSet<Function> Dependencies;

		public Function (MemberInfo member, string code, HashSet<Function> dependencies)
		{
			Member = member;
			Code = code;
			Dependencies = dependencies;
		}

		public void Output (StringBuilder sb, HashSet<Function> outputted)
		{
			foreach (var fun in Dependencies)
				if (!outputted.Contains (fun))
					fun.Output (sb, outputted);
			outputted.Add (this);
			sb.AppendLine (Code);
		}

		public override bool Equals (object obj)
		{
			var other = obj as Function;
			return other != null && Equals (other);
		}

		public override int GetHashCode ()
		{
			return Member.GetHashCode ();
		}

		#region IEquatable implementation

		public bool Equals (Function other)
		{
			return Member.Equals (other.Member);
		}

		#endregion
	}
}