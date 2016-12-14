namespace Extensions
{
	using System;
	using System.Linq;
	using System.Reflection;

	public static class TypeExt
	{
		public static T GetAttribute<T> (this MemberInfo mi) where T : Attribute
		{
			if (mi == null)
				return null;
			var attrs = mi.GetCustomAttributes (typeof (T), true);
			return attrs == null || attrs.Length == 0 ? null : attrs.Cast<T> ().Single ();
		}
	}
}
