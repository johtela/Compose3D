namespace Compose3D.Compiler
{
	using System;
	using System.Linq.Expressions;
	using System.Reflection;
	using Extensions;

	public abstract class TypeMapping
    {
		protected static Type boolT = typeof (bool);
		protected static Type floatT = typeof (float);
		protected static Type doubleT = typeof (double);
		protected static Type intT = typeof (int);
		protected static Type mathT = typeof (Math);

        protected static MethodInfo GetMethod (Type type, string name, params Type[] args)
        {
			var res = type.GetMethod (name, args);
			if (res == null)
				throw new ArgumentException (string.Format ("Method {0}.{1}({2}) not found.", type, name,
					args.SeparateWith (", ")));
			return res;
        }

		public bool IsSupportedType (Type type)
		{
			return Type (type) != null;
		}

		public abstract string Type (Type type);

		public abstract string Function (MethodInfo method);

		public abstract string Operator (MethodInfo method, ExpressionType et);

		public abstract string Constructor (ConstructorInfo constructor);
    }
}
