namespace ComposeFX.Graphics
{
    using System;

    [AttributeUsage (AttributeTargets.Field)]
    public class BuiltinAttribute : Attribute { }

	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property)]
	public class OmitInGlslAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.Field | AttributeTargets.Property)]
    public class GLQualifierAttribute : Attribute
    {
        public string Qualifier;

        public GLQualifierAttribute (string qualifier)
        {
            Qualifier = qualifier;
        }
    }

	[AttributeUsage (AttributeTargets.Field)]
	public class GLFieldAttribute : Attribute
	{
		public string Name;

		public GLFieldAttribute (string name) 
		{ 
			Name = name;
		}
	}

    public class GLAttribute : Attribute
    {
        public string Syntax;

        public GLAttribute (string syntax)
        {
            Syntax = syntax;
        }
    }

    [AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct)]
    public class GLType : GLAttribute
    {
        public GLType (string syntax) : base (syntax) { }
    }

    [AttributeUsage (AttributeTargets.Constructor)]
    public class GLConstructor : GLAttribute
    {
        public GLConstructor (string syntax) : base (syntax) { }
    }

    [AttributeUsage (AttributeTargets.Method)]
    public class GLUnaryOperator : GLAttribute
    {
        public GLUnaryOperator (string syntax) : base (syntax) { }
    }

    [AttributeUsage (AttributeTargets.Method)]
    public class GLBinaryOperator : GLAttribute
    {
        public GLBinaryOperator (string syntax) : base (syntax) { }
    }

    [AttributeUsage (AttributeTargets.Method | AttributeTargets.Property)]
    public class GLFunction : GLAttribute
    {
        public GLFunction (string syntax) : base (syntax) { }
    }

    [AttributeUsage (AttributeTargets.Struct)]
    public class GLStruct : GLAttribute
    {
        public GLStruct () : base (null) { }
    }
}
