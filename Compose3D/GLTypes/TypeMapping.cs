namespace Compose3D.GLTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class TypeMapping
    {
        private static Dictionary<Type, string> _types = new Dictionary<Type, string> ()
        {
             { typeof (float), "float" },
             { typeof (double), "double "}
        };
 
        public static string Type (Type type)
        {
            return _types[type];
        }
    }
}
