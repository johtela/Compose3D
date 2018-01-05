/*
# Extensions for Objects

The most general set of extension methods in this library is defined
here. The methods in this class can be used basically with any .NET
type.
*/
namespace Extensions
{
	using System.Linq;

	public static class ObjectExt
	{
		/*
		## Value in Set

		Testing if a value is in a specified set is usually done by a long and 
		repetitive `if` statement of the form:
			
			if (<variable> == <value1> || 
				<variable> == <value2> || 
				<variable> == <value3> || 
				...) 

		By using the extension method below the test simplifies to:

			if (<variable>.In (<value1>, <value2>, <value3>, ...))		
		*/
		public static bool In<T> (this T obj, params T[] alternatives)
		{
			return alternatives.Contains (obj);
		}
		/*
		## Check for Default Value

		To generically set whether a value or reference is set to default 
		or vice versa, one can use the following methods.
		*/
		public static bool IsDefault<T> (this T obj)
		{
			return obj.Equals (default (T));
		}

		public static bool NotDefault<T> (this T obj)
		{
			return !obj.Equals (default (T));
		}
	}
}
