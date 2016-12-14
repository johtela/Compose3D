namespace Compose3D.GLTypes
{
	using System;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;
	using OpenTK.Graphics.OpenGL4;
	using System.Collections.Generic;
	using Extensions;
	using Compiler;

	/// <summary>
	/// Metadata about a field in a GLSL struct type. This class also helps accessing
	/// the field value in a generic way. The Getter delegate is dynamically compiled
	/// to return the corresponding field to speed up the access.
	/// </summary>
	public class GLStructField
    {
		/// <summary>
		/// Create metadata for a GLSL struct field.
		/// </summary>
        public GLStructField (string name, Type type, Func<object, object> getter)
        {
            Name = name;
            Type = type;
            Getter = getter;
        }

		/// <summary>
		/// The name of the field contains the full path. If the struct where the field 
		/// resides is contained inside another structure or array, then the name contains
		/// multiple parts separated by dots. This corresponds to the way how GLSL uniforms
		/// are accessed from the host application.
		/// </summary>
        public string Name { get; private set; }

		/// <summary>
		/// The type of the field.
		/// </summary>
        public Type Type { get; private set; }

		/// <summary>
		/// A getter to generically return the value of the field. This is handy when
		/// transferring data to GLSL uniforms.
		/// </summary>
        public Func<object, object> Getter { get; private set; }
    }

    public static class GLTypeHelpers
    {
        private const BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public;

		/// <summary>
		/// This global dictionary contains all the GLSL struct types defined, and their fields.
		/// The keys in the dictionary have the format [expr]@[type] where [expr] is the string
		/// that is used to access the field from the host code, and [type] is the fully qualified
		/// name of the C# type where the field resides. The type name is needed to distinguish
		/// fields with the same name defined in different classes.
		/// </summary>
        private static Dictionary<string, IList<GLStructField>> _structFields = 
			new Dictionary<string, IList<GLStructField>> ();

		/// <summary>
		/// Return an OpenGL related attribute from a member or null, if one is not found.
		/// </summary>
        public static GLAttribute GetGLAttribute (this MemberInfo mi)
        {
            return mi.GetAttribute<GLAttribute> ();
        }

		public static string GetGLSyntax (this MemberInfo mi)
		{
			var result = GetGLAttribute (mi);
			return result != null ? result.Syntax : null;
		}

		/// <summary>
		/// Check if a type is used in GLSL. This is done by checking if the type is
		/// annotated with any GL related attibute.
		/// </summary>
        public static bool IsGLType (this Type type)
        {
            return type.GetGLAttribute () != null;
        }

		/// <summary>
		/// Get the qualifiers of a varying input or output field.
		/// </summary>
		public static string GetQualifiers (this MemberInfo mi)
        {
            return mi.GetCustomAttributes (typeof (GLQualifierAttribute), true)
                .Cast<GLQualifierAttribute> ().Select (q => q.Qualifier).SeparateWith (" ");
        }

		/// <summary>
		/// Check whether a [Builtin] attribute is defined for a member. This attribute
		/// is used to distinguish variables that are built into GLSL and thus don't need
		/// to be outputted.
		/// </summary>
        public static bool IsBuiltin (this MemberInfo mi)
        {
            return mi.IsDefined (typeof (BuiltinAttribute), true);
        }

		/// <summary>
		/// Check whether a type contains the [GLStruct] attribute and thus is 
		/// outputted to GLSL.
		/// </summary>
		public static bool IsGLStruct (this Type type)
        {
            return type.IsDefined (typeof (GLStruct), true);
        }

		/// <summary>
		/// Get the name of the field in GLSL. The [GLField] attribute is used in 
		/// built-in types to map C# names to GLSL.
		/// </summary>
		public static string GetGLFieldName (this MemberInfo mi)
		{
			var attr = mi.GetAttribute<GLFieldAttribute> ();
			return attr == null ? mi.Name : attr.Name;
		}

		/// <summary>
		/// Enumerate all the fields of a struct type that are used in GLSL. This
		/// includes all the public instance fields and excludes private and
		/// static ones.
		/// </summary>
        public static IEnumerable<FieldInfo> GetGLFields (this Type type)
        {
            return type.GetFields (_bindingFlags);
        }

		/// <summary>
		/// Enumerate all the propertis of a type that are used in GLSL. This
		/// includes all the public instance properties and excludes private and
		/// static ones.
		/// </summary>
		public static IEnumerable<PropertyInfo> GetGLProperties (this Type type)
        {
            return type.GetProperties (_bindingFlags);
        }

		/// <summary>
		/// Get all the fields of a type that are outputted to GLSL as uniforms.
		/// This means all the fields that are of generic type <see cref="Uniform{T}"/>.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static IEnumerable<FieldInfo> GetUniforms (this Type type)
        {
			return from field in type.GetFields (_bindingFlags)
			       where field.FieldType.IsGenericType &&
			           field.FieldType.GetGenericTypeDefinition () == typeof (Uniform<>)
			       select field;
        }

		/// <summary>
		/// Subroutine that constructs <see cref="GLStructField"/> objects for each item
		/// in an array. In GLSL arrays have fixed length, so there will be as many struct
		/// fields added as there are items in the array. The accessor delegate is constructed
		/// for each item. Also, if the item of the array is a struct, then additional 
		/// GLStructFields will be created for each field.
		/// </summary>
		/// <param name="type">The array type for which the fields are created.</param>
		/// <param name="expression">The Linq expression which refers to the array.</param>
		/// <param name="parameter">The parameter expression of the dynamic delegate that is 
		/// constructed.</param>
		/// <param name="fields">The list of fields created so far. The created fields will
		/// be added at the end of this list.</param>
		/// <param name="prefix">The prefix string that is prepended to the field name.</param> 
		/// <param name="arrayLen">Length of the array.</param>
		private static void CreateArrayFields (Type type, Expression expression, 
			ParameterExpression parameter, IList<GLStructField> fields, string prefix, int arrayLen)
        {
            for (int i = 0; i < arrayLen; i++)
            {
                var elemType = type.GetElementType ();
                var arrayExpr = Expression.ArrayAccess (expression, Expression.Constant (i));
                if (elemType.IsGLStruct ())
                    CreateStructFields (elemType, arrayExpr, parameter, fields, 
                        string.Format ("{0}[{1}].", prefix, i));
                else
                    fields.Add (new GLStructField (string.Format ("{0}[{1}]", prefix, i),
                        elemType, Expression.Lambda<Func<object, object>> (
                        Expression.Convert(arrayExpr, typeof (object)), parameter).Compile ()));
            }
        }

		/// <summary>
		/// Subroutine that constructs <see cref="GLStructField"/> objects for the fields of 
		/// struct type. The accessor expression is constructed for each field in the struct. 
		/// If a field is an array, then accessors for each array item are created separately.
		/// </summary>
		/// <param name="type">The type of the struct.</param>
		/// <param name="expression">The Linq expression referring to the struct.</param>
		/// <param name="parameter">The parameter expression of the dynamic delegate that is 
		/// constructed.</param>
		/// <param name="fields">The list of fields created so far. The created fields will
		/// be added at the end of this list.</param>
		/// <param name="prefix">The prefix string that is prepended to the field name.</param> 
		private static void CreateStructFields (Type type, Expression expression, 
			ParameterExpression parameter, IList<GLStructField> fields, string prefix)
        {
            foreach (var field in type.GetGLFields ())
            {
                var fieldType = field.FieldType;
                var fieldExpr = Expression.Field (expression, field);
                if (fieldType.IsGLStruct ())
                    CreateStructFields (fieldType, fieldExpr, parameter, fields, 
						prefix + field.Name + ".");
                else if (fieldType.IsArray)
                    CreateArrayFields (fieldType, fieldExpr, parameter, fields, 
						prefix + field.Name, field.ExpectFixedArrayAttribute ().Length);
                else
                    fields.Add (new GLStructField (prefix + field.Name, fieldType,
                        Expression.Lambda<Func<object, object>> (
                        Expression.Convert (fieldExpr, typeof (object)), parameter).Compile ()));
            }
        }

		/// <summary>
		/// Construct the key for the field that is used in the global dictionary.
		/// </summary>
		/// <remarks>
		/// The keys in the dictionary have the format [expr]@[type] where [expr] is the string
		/// that is used to access the field from the host code, and [type] is the fully qualified
		/// name of the C# type where the field resides. The type name is needed to distinguish
		/// fields with the same name defined in different classes.
		/// </remarks>
		private static string GetKey (Type type, string prefix)
		{
			return prefix + "@" + type.FullName;
		}

		/// <summary>
		/// Enumerate the <see cref="GLStructField"/> objects associated with a type. If the type is
		/// not yet in the global dictionary it will be added there and the fields will be created.
		/// If it is already there then the cached fields are returned.
		/// Note: If the cached fields are returned the <paramref name="prefix"/> is ignored. It is
		/// used only the first time when the fields are created.
		/// </summary>
		public static IEnumerable<GLStructField> GetGLStructFields (this Type type, string prefix)
        {
            IList<GLStructField> result;
			var key = GetKey (type, prefix);

			if (!_structFields.TryGetValue (key, out result))
            {
                result = new List<GLStructField> ();
                var expression = Expression.Parameter (typeof (object), "obj");
                CreateStructFields (type, Expression.Convert (expression, type), expression, result, 
					prefix);
                _structFields.Add (key, result);
            }
            return result;
        }

		/// <summary>
		/// Enumerate the <see cref="GLStructField"/> objects associated to an array. Arrays in GLS 
		/// have fixed length, so that information must be also provided. In C# type metadata this 
		/// information does not exist. If the fields have been already created for the same GLSL 
		/// variable, then they are returned from a global dictionary.
		/// Note: If the cached fields are returned the <paramref name="prefix"/> is ignored. It is
		/// used only the first time when the fields are created.
		/// </summary>
		public static IEnumerable<GLStructField> GetGLArrayElements (this Type type, string prefix, 
			int arrayLen)
        {
            IList<GLStructField> result;
			var key = GetKey (type, prefix);
			 
			if (!_structFields.TryGetValue (key, out result))
            {
                result = new List<GLStructField> ();
                var expression = Expression.Parameter (typeof (object), "obj");
                CreateArrayFields (type, Expression.Convert (expression, type), expression, result, 
					prefix, arrayLen);
                _structFields.Add (key, result);
            }
            return result;
        }

		/// <summary>
		/// Maps the OpenGL primitive type to a keyword used in GLSL geometry
		/// shaders as input qualifier.
		/// </summary>
		public static string MapInputGSPrimitive (this PrimitiveType type)
		{
			switch (type)
			{
				case PrimitiveType.Points: return "points";
				case PrimitiveType.Lines:
				case PrimitiveType.LineLoop:
				case PrimitiveType.LineStrip: return "lines";
				case PrimitiveType.LinesAdjacency:
				case PrimitiveType.LineStripAdjacency: return "lines_adjacency";
				case PrimitiveType.Triangles:
				case PrimitiveType.TriangleStrip:
				case PrimitiveType.TriangleFan: return "triangles";
				case PrimitiveType.TrianglesAdjacency:
				case PrimitiveType.TriangleStripAdjacency: return "triangles_adjacency";
				default: throw new ArgumentException (
					"Unsupported geometry shader input primitive type: " + type);
			}
		}

		/// <summary>
		/// Maps the OpenGL primitive type to a keyword used in GLSL geometry
		/// shaders as output qualifier.
		/// </summary>
		public static string MapOutputGSPrimitive (this PrimitiveType type)
		{
			switch (type)
			{
				case PrimitiveType.Points: return "points";
				case PrimitiveType.LineStrip: return "line_strip";
				case PrimitiveType.TriangleStrip: return "triangle_strip";
				default: throw new ArgumentException (
					"Unsupported geometry shader output primitive type: " + type);
			}
		}
	}
}
