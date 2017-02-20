namespace Compose3D.CLTypes
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using System.Reflection;
	using Compiler;
	using Extensions;

	public class CLTypeMapping : TypeMapping
    {
        private static Dictionary<Type, string> _types = new Dictionary<Type, string> ()
        {
            { boolT, "bool" },
            { floatT, "float" },
			{ doubleT, "double " },
			{ intT, "int" },
			{ uintT, "uint" }
        };

		private static Dictionary<MethodInfo, string> _functions = new Dictionary<MethodInfo, string> ()
        {
            { GetMethod (mathT, "Abs", floatT), "abs ({0})" },
            { GetMethod (mathT, "Abs", intT), "abs ({0})" },
            { GetMethod (mathT, "Sign", floatT), "sign ({0})" },
            { GetMethod (mathT, "Sign", doubleT), "sign ({0})" },
            { GetMethod (mathT, "Sign", intT), "sign ({0})" },
            { GetMethod (mathT, "Floor", doubleT), "floor ({0})" },
            { GetMethod (mathT, "Ceiling", doubleT), "ceil ({0})" },
            { GetMethod (mathT, "Truncate", doubleT), "trunc ({0})" },
            { GetMethod (mathT, "Min", floatT, floatT), "min ({0})" },
            { GetMethod (mathT, "Min", doubleT, doubleT), "min ({0})" },
            { GetMethod (mathT, "Min", intT, intT), "min ({0})" },
            { GetMethod (mathT, "Max", floatT, floatT), "max ({0})" },
            { GetMethod (mathT, "Max", doubleT, doubleT), "max ({0})" },
            { GetMethod (mathT, "Max", intT, intT), "max ({0})" },
            { GetMethod (floatT, "IsNaN", floatT), "isnan ({0})" },
            { GetMethod (doubleT, "IsNaN", doubleT), "isnan ({0})" },
            { GetMethod (mathT, "Sin", doubleT), "sin ({0})" },
            { GetMethod (mathT, "Cos", doubleT), "cos ({0})" },
            { GetMethod (mathT, "Tan", doubleT), "tan ({0})" },
            { GetMethod (mathT, "Asin", doubleT), "asin ({0})" },
            { GetMethod (mathT, "Acos", doubleT), "acos ({0})" },
            { GetMethod (mathT, "Atan", doubleT), "atan ({0})" },
            { GetMethod (mathT, "Atan2", doubleT, doubleT), "atan ({0})" },
            { GetMethod (mathT, "Pow", doubleT, doubleT), "pow ({0})" },
            { GetMethod (mathT, "Exp", doubleT), "exp ({0})" },
            { GetMethod (mathT, "Log", doubleT), "log ({0})" },
            { GetMethod (mathT, "Sqrt", doubleT), "sqrt ({0})" }
		};

        private static Dictionary<ExpressionType, string> _operators = new Dictionary<ExpressionType, string>
        {
            { ExpressionType.Add, "{0} + {1}" },
            { ExpressionType.Subtract, "{0} - {1}" },
            { ExpressionType.Multiply, "{0} * {1}" },
            { ExpressionType.Divide, "{0} / {1}" },
            { ExpressionType.Negate, "-{0}"},
            { ExpressionType.And, "{0} & {1}"},
            { ExpressionType.Or, "{0} | {1}"},
            { ExpressionType.LeftShift, "{0} << {1}"},
            { ExpressionType.RightShift, "{0} >> {1}"},
            { ExpressionType.OnesComplement, "~{0}"},
            { ExpressionType.Equal, "{0} == {1}" },
			{ ExpressionType.NotEqual, "{0} != {1}" },
            { ExpressionType.LessThan, "{0} < {1}" },
            { ExpressionType.LessThanOrEqual, "{0} <= {1}" },
            { ExpressionType.GreaterThan, "{0} > {1}" },
            { ExpressionType.GreaterThanOrEqual, "{0} >= {1}" },
            { ExpressionType.AndAlso, "{0} && {1}" },
            { ExpressionType.OrElse, "{0} || {1}" },
            { ExpressionType.Not, "!{0}" },
            { ExpressionType.ArrayIndex, "{0}[{1}]" },
			{ ExpressionType.ArrayLength, "{0}.length ()" },
			{ ExpressionType.PostDecrementAssign, "{0}--" },
			{ ExpressionType.PostIncrementAssign, "{0}++" },
			{ ExpressionType.AddAssign, "{0} += {1}" },
			{ ExpressionType.SubtractAssign, "{0} -= {1}" }
		};
 
        public override string Type (Type type)
        {
			if (type.IsArray)
				return (Type (type.GetElementType ())) + "*";
			string result;
			return type.GetCLSyntax () ??
				(_types.TryGetValue (type, out result) ? result : null);
		}

		public override string Function (MethodInfo method)
        {
			string result;
			return method.GetCLSyntax () ??
				(_functions.TryGetValue (method, out result) ? result : null);
		}

		public override string Operator (MethodInfo method, ExpressionType et)
        {
			string result;
			return method.GetCLSyntax () ??
				(_operators.TryGetValue (et, out result) ? result : null);
		}

		public override string Constructor (ConstructorInfo constructor)
		{
			return constructor.GetCLSyntax ();
		}

		public override string Indexer (MethodInfo method)
		{
			return null;
		}
	}
}
