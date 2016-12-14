namespace Compose3D.Shaders
{
	using System;
	using System.Linq.Expressions;

	internal class Constant : IEquatable<Constant>
	{
		public readonly Type Type;
		public readonly string Name;
		public readonly Expression Value;

		public Constant (Type type, string name, Expression value)
		{
			Type = type;
			Name = name;
			Value = value;
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

		public bool Equals (Constant other)
		{
			return Name.Equals (other.Name);
		}

		#endregion
	}
}