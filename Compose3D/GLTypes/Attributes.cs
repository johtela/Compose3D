﻿namespace Compose3D.GLTypes
{
    using System;
    using System.Linq;

    [AttributeUsage (AttributeTargets.Field)]
    public class BuiltinAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.Field)]
    public class LocalAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.Field)]
    public class GLQualifierAttribute : Attribute
    {
        public readonly string Qualifier;

        public GLQualifierAttribute (string qualifier)
        {
            Qualifier = qualifier;
        }
    }

    public class GLAttribute : Attribute
    {
        public readonly string Syntax;

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
}
