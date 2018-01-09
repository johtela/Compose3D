/*
# Reflection Extensions

The `ReflectionExt` class provides few extensions useful with reflection 
objects.
*/
namespace Extensions
{
	using System;
	using System.Linq;
	using System.Reflection;

	public static class ReflectionExt
	{
		/*
		## Get Member Attribute

		The following method should really be inside .NET framework in the 
		first place. It returns an attribute of specified type given the
		reflection info for a member. If an attribute with specified type
		is not found, null is returned.
		*/
		public static T GetAttribute<T> (this MemberInfo mi) where T : Attribute
		{
			if (mi == null)
				return null;
			var attrs = mi.GetCustomAttributes (typeof (T), true);
			return attrs == null || attrs.Length == 0 ? null : attrs.Cast<T> ().Single ();
		}
		/*
		## Is an Object Instance of a Generic Type 

		The method below checks if the object int the first argument position
		is an instance of generic type whose reflection info is given in the
		second argument.
		*/
		public static bool IsInstanceOfGenericType (this object obj, Type type)
		{
			return obj.GetType ().GetGenericTypeDefinition () == type;
		}
	}
}
