namespace Compose3D.GLTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public static class TypeMapping
    {
        private static Type floatT = typeof (float);
        private static Type doubleT = typeof (double);
        private static Type mathT = typeof (Math);

        private static MethodInfo GetMethod (Type type, string name, params Type[] args)
        {
            return type.GetMethod (name, args);
        }

        private static Dictionary<Type, string> _types = new Dictionary<Type, string> ()
        {
             { floatT, "float" },
             { doubleT, "double "}
        };

        private static Dictionary<MethodInfo, string> _functions = new Dictionary<MethodInfo, string> ()
        {
            { GetMethod (mathT, "Min", floatT, floatT), "min ({0})" },
            { GetMethod (mathT, "Min", doubleT, doubleT), "min ({0})" },
            { GetMethod (mathT, "Max", floatT, floatT), "max ({0})" },
            { GetMethod (mathT, "Max", doubleT, doubleT), "max ({0})" }
        };
 
        public static string Type (Type type)
        {
            return _types[type];
        }

        public static string Function (MethodInfo method)
        {
            return _functions[method];
        }
    }
}
