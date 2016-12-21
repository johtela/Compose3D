namespace Compose3D.CLTypes
{
	using System;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;
	using OpenTK.Graphics.OpenGL4;
	using System.Collections.Generic;
	using Extensions;

    public static class CLTypeHelpers
    {
        private const BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public;

		/// <summary>
		/// Return an OpenCL related attribute from a member or null, if one is not found.
		/// </summary>
        public static CLAttribute GetCLAttribute (this MemberInfo mi)
        {
            return mi.GetAttribute<CLAttribute> ();
        }

		public static string GetCLSyntax (this MemberInfo mi)
		{
			var attr = GetCLAttribute (mi);
			return attr != null ? attr.Syntax : null;
		}

		/// <summary>
		/// Check if a type is used in OpenCL C. This is done by checking if the type is
		/// annotated with any CL related attibute.
		/// </summary>
		public static bool IsCLType (this Type type)
        {
            return type.GetCLAttribute () != null;
        }

		/// <summary>
		/// Check whether a type contains the [CLStruct] attribute and thus is 
		/// outputted to GLSL.
		/// </summary>
		public static bool IsCLStruct (this Type type)
        {
            return type.IsDefined (typeof (CLStruct), true);
        }

		/// <summary>
		/// Enumerate all the fields of a struct type that are used in GLSL. This
		/// includes all the public instance fields and excludes private and
		/// static ones.
		/// </summary>
        public static IEnumerable<FieldInfo> GetCLFields (this Type type)
        {
            return type.GetFields (_bindingFlags);
        }

		/// <summary>
		/// Enumerate all the propertis of a type that are used in GLSL. This
		/// includes all the public instance properties and excludes private and
		/// static ones.
		/// </summary>
		public static IEnumerable<PropertyInfo> GetCLProperties (this Type type)
        {
            return type.GetProperties (_bindingFlags);
        }

		/// <summary>
		/// Get the name of the field in GLSL. The [GLField] attribute is used in 
		/// built-in types to map C# names to GLSL.
		/// </summary>
		public static string GetCLFieldName (this MemberInfo mi)
		{
			var attr = mi.GetAttribute<CLFieldAttribute> ();
			return attr == null ? mi.Name : attr.Name;
		}
	}
}
