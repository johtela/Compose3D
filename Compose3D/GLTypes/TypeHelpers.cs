namespace Compose3D.GLTypes
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using OpenTK.Graphics.OpenGL;
    using System.Collections.Generic;
    using System.Text;
    using System.Globalization;
	using Extensions;

    public class GLStructField
    {
        public GLStructField (string name, Type type, Func<object, object> getter)
        {
            Name = name;
            Type = type;
            Getter = getter;
        }

        public string Name { get; private set; }
        public Type Type { get; private set; }
        public Func<object, object> Getter { get; private set; }
    }

    public static class TypeHelpers
    {
        private const BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static Dictionary<Type, IList<GLStructField>> _structFields = new Dictionary<Type, IList<GLStructField>> ();

        private static T GetAttribute<T> (MemberInfo mi) where T : Attribute
        {
            if (mi == null)
                return null;
            var attrs = mi.GetCustomAttributes (typeof (T), true);
            return attrs == null || attrs.Length == 0 ? null : attrs.Cast<T> ().Single ();
        }

        public static GLAttribute GetGLAttribute (this MemberInfo mi)
        {
            return GetAttribute<GLAttribute> (mi);
        }

        public static GLArrayAttribute ExpectGLArrayAttribute (this FieldInfo fi)
        {
            var res = GetAttribute<GLArrayAttribute> (fi);
            if (res == null)
                throw new ArgumentException ("Missing GLArray attribute for array.");
            return res;
        }

        public static bool IsGLType (this Type type)
        {
            return type.GetGLAttribute () != null;
        }

        public static string GetQualifiers (this MemberInfo mi)
        {
            return mi.GetCustomAttributes (typeof (GLQualifierAttribute), true)
                .Cast<GLQualifierAttribute> ().Select (q => q.Qualifier).SeparateWith (" ");
        }

        public static bool IsBuiltin (this MemberInfo mi)
        {
            return mi.IsDefined (typeof (BuiltinAttribute), true);
        }

		public static bool IsLiftMethod (this MethodInfo mi)
		{
			return mi.IsDefined (typeof (LiftMethodAttribute), true);
		}

        public static bool IsGLStruct (this Type type)
        {
            return type.IsDefined (typeof (GLStruct), true);
        }

		public static string GetGLFieldName (this MemberInfo mi)
		{
			var attr = GetAttribute<GLFieldAttribute> (mi);
			return attr == null ? mi.Name : attr.Name;
		}

        public static IEnumerable<FieldInfo> GetGLFields (this Type type)
        {
            return type.GetFields (_bindingFlags);
        }

        public static IEnumerable<PropertyInfo> GetGLProperties (this Type type)
        {
            return type.GetProperties (_bindingFlags);
        }

        public static IEnumerable<FieldInfo> GetUniforms (this Type type)
        {
			foreach (var field in type.GetFields (_bindingFlags))
			{
				var fieldType = field.FieldType;
				if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition () == typeof (Uniform<>))
					yield return field;
				else
					foreach (var subField in GetUniforms (fieldType))
						yield return subField;
			}
            //return from field in type.GetFields (_bindingFlags)
            //       where field.FieldType.GetGenericTypeDefinition () == typeof (Uniform<>)
            //       select field;
        }

        private static void GetArrayFields (Type type, Expression expression, ParameterExpression parameter,
            IList<GLStructField> fields, string prefix, int arrayLen)
        {
            for (int i = 0; i < arrayLen; i++)
            {
                var elemType = type.GetElementType ();
                var arrayExpr = Expression.ArrayAccess (expression, Expression.Constant (i));
                if (elemType.IsGLStruct ())
                    GetStructFields (elemType, arrayExpr, parameter, fields, 
                        string.Format ("{0}[{1}].", prefix, i));
                else
                    fields.Add (new GLStructField (string.Format ("{0}[{1}]", prefix, i),
                        elemType, Expression.Lambda<Func<object, object>> (
                        Expression.Convert(arrayExpr, typeof (object)), parameter).Compile ()));
            }
        }

        private static void GetStructFields (Type type, Expression expression, ParameterExpression parameter,
            IList<GLStructField> fields, string prefix)
        {
            foreach (var field in type.GetGLFields ())
            {
                var fieldType = field.FieldType;
                var fieldExpr = Expression.Field (expression, field);
                if (fieldType.IsGLStruct ())
                    GetStructFields (fieldType, fieldExpr, parameter, fields, prefix + field.Name + ".");
                else if (fieldType.IsArray)
                    GetArrayFields (fieldType, fieldExpr, parameter, fields, prefix + field.Name, 
                        field.ExpectGLArrayAttribute ().Length);
                else
                    fields.Add (new GLStructField (prefix + field.Name, fieldType,
                        Expression.Lambda<Func<object, object>> (
                        Expression.Convert (fieldExpr, typeof (object)), parameter).Compile ()));
            }
        }

        public static IEnumerable<GLStructField> GetGLStructFields (this Type type, string prefix)
        {
            IList<GLStructField> result;
            if (!_structFields.TryGetValue (type, out result))
            {
                result = new List<GLStructField> ();
                var expression = Expression.Parameter (typeof (object), "obj");
                GetStructFields (type, Expression.Convert (expression, type), expression, result, prefix);
                _structFields.Add (type, result);
            }
            return result;
        }

        public static IEnumerable<GLStructField> GetGLArrayElements (this Type type, string prefix, int arrayLen)
        {
            IList<GLStructField> result;
            if (!_structFields.TryGetValue (type, out result))
            {
                result = new List<GLStructField> ();
                var expression = Expression.Parameter (typeof (object), "obj");
                GetArrayFields (type, Expression.Convert (expression, type), expression, result, prefix, arrayLen);
                _structFields.Add (type, result);
            }
            return result;
        }
    }
}
