namespace Compose3D.GLTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    public static class TypeMapping
    {
        private static Type floatT = typeof (float);
        private static Type doubleT = typeof (double);
		private static Type intT = typeof (int);
        private static Type mathT = typeof (Math);

        private static MethodInfo GetMethod (Type type, string name, params Type[] args)
        {
            return type.GetMethod (name, args);
        }

        private static Dictionary<Type, string> _types = new Dictionary<Type, string> ()
        {
            { floatT, "float" },
			{ doubleT, "double " },
			{ intT, "int" }
        };

        private static Dictionary<MethodInfo, string> _functions = new Dictionary<MethodInfo, string> ()
        {
            { GetMethod (mathT, "Min", floatT, floatT), "min ({0})" },
            { GetMethod (mathT, "Min", doubleT, doubleT), "min ({0})" },
            { GetMethod (mathT, "Max", floatT, floatT), "max ({0})" },
            { GetMethod (mathT, "Max", doubleT, doubleT), "max ({0})" }
        };

        private static Dictionary<ExpressionType, string> _operators = new Dictionary<ExpressionType, string>
        {
            { ExpressionType.Add, "{0} + {1}"},
            { ExpressionType.Subtract, "{0} - {1}"},
            { ExpressionType.Multiply, "{0} * {1}"},
            { ExpressionType.Divide, "{0} / {1}"},
            { ExpressionType.Negate, "-{0}"},
            { ExpressionType.Equal, "{0} == {1}"},
            { ExpressionType.LessThan, "{0} < {1}"},
            { ExpressionType.LessThanOrEqual, "{0} <= {1}"},
            { ExpressionType.GreaterThan, "{0} > {1}"},
            { ExpressionType.GreaterThanOrEqual, "{0} >= {1}"}
        };
 
        public static string Type (Type type)
        {
			string result;
			if (!_types.TryGetValue (type, out result))
				throw new GLError ("No mapping defined for type: " + type);
			return result;
        }

        public static string Function (MethodInfo method)
        {
			string result;
			if (!_functions.TryGetValue (method, out result))
				throw new GLError ("No mapping defined for method: " + method);
			return result;
        }

        public static string Operators (ExpressionType et)
        {
			string result;
			if (!_operators.TryGetValue (et, out result))
				throw new GLError ("No mapping defined for operator: " + et);
			return result;
        }
    }
}
