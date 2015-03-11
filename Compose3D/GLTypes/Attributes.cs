namespace Compose3D.GLTypes
{
    using System;

    [AttributeUsage (AttributeTargets.Field)]
    public class SmoothAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.Field)]
    public class BuiltinAttribute : Attribute { }

    public class GLAttribute : Attribute
    {
        public readonly string Syntax;

        public GLAttribute (string syntax)
        {
            Syntax = syntax;
        }
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
}
