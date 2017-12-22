namespace ComposeFX.Compute
{
    using System;

	[AttributeUsage (AttributeTargets.Field)]
	public class CLFieldAttribute : Attribute
	{
		public string Name;

		public CLFieldAttribute (string name)
		{
			Name = name;
		}
	}

	public class CLAttribute : Attribute
    {
        public string Syntax;

        public CLAttribute (string syntax)
        {
            Syntax = syntax;
        }
    }

    [AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct)]
    public class CLType : CLAttribute
    {
        public CLType (string syntax) : base (syntax) { }
    }

    [AttributeUsage (AttributeTargets.Constructor)]
    public class CLConstructor : CLAttribute
    {
        public CLConstructor (string syntax) : base (syntax) { }
    }

    [AttributeUsage (AttributeTargets.Method)]
    public class CLUnaryOperator : CLAttribute
    {
        public CLUnaryOperator (string syntax) : base (syntax) { }
    }

    [AttributeUsage (AttributeTargets.Method)]
    public class CLBinaryOperator : CLAttribute
    {
        public CLBinaryOperator (string syntax) : base (syntax) { }
    }

    [AttributeUsage (AttributeTargets.Method | AttributeTargets.Property)]
    public class CLFunction : CLAttribute
    {
        public CLFunction (string syntax) : base (syntax) { }
    }

    [AttributeUsage (AttributeTargets.Struct)]
    public class CLStruct : CLAttribute
    {
        public CLStruct () : base (null) { }
    }

	[AttributeUsage (AttributeTargets.Struct)]
	public class CLUnion : CLAttribute
	{
		public CLUnion () : base (null) { }
	}
}