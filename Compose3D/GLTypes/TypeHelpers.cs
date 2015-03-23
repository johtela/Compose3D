﻿namespace Compose3D.GLTypes
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using OpenTK.Graphics.OpenGL;
    using System.Collections.Generic;
    using System.Text;
    using System.Globalization;

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

        public static GLAttribute GetGLAttribute (this MemberInfo mi)
        {
            if (mi == null)
                return null;
            var attrs = mi.GetCustomAttributes (typeof (GLAttribute), true);
            return attrs == null || attrs.Length == 0 ? null : attrs.Cast<GLAttribute> ().Single ();
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

        public static bool IsGLStruct (this Type type)
        {
            return type.IsDefined (typeof (GLStruct), true);
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
            return from field in type.GetFields (_bindingFlags)
                   where field.FieldType.GetGenericTypeDefinition () == typeof (Uniform<>)
                   select field;
        }

        private static void GetStructFields (Type type, Expression expression, ParameterExpression parameter,
            IList<GLStructField> fields, string prefix)
        {
            foreach (var field in type.GetGLFields ())
            {
                var fieldType = field.FieldType;
                var expr = Expression.Field (expression, field);
                if (fieldType.IsGLStruct ())
                    GetStructFields (fieldType, expr, parameter, fields, prefix + field.Name + ".");
                else
                    fields.Add (new GLStructField (prefix + field.Name, fieldType, 
                        Expression.Lambda<Func<object, object>> (
                        Expression.Convert(expr, typeof (object)), parameter).Compile ()));
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
            }
            return result;
        }
    }
}