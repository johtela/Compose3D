/*
# Function Extensions

It is possible to create extension methods for delagates, but they cannot be
used in conjunction with lamda expressions, which makes them less convenient. 
So, the static methods defined in the `Fun`class below, are not actually extension 
methods. Anyhow, they are helper methods that either take or return generic `Func` 
and `Action` delegates.
*/
namespace Extensions
{
	using System;

	public static class Fun
	{

		/*
		## Identity function

		The identity function is the simplest possible function that one can
		define. As a lambda expression it looks like this:
		```
		a => a
		```
		Since the identity function is needed in so many places, it makes sense
		to define it explicitly as a static function. Then we don't need to define 
		a new lambda function and allocate a closure every time. So, it is a bit 
		more efficient as well.
		*/
		public static T Identity<T> (T arg)
		{
			return arg;
		}
		/*
		## Memoization

		Memoization is a technique where the result of a function is stored for
		later use. When the same function is called again, the cached result is
		returned instead of evaluating the function again.

		We will define here just the simplest case when the function to be called
		has no parameters. Therefore, we need to store only a single value instead
		of a map from parameters to results. 
		*/
		public static Func<T> Memoize<T> (Func<T> func) where T : class
		{
			T store = null;
			return () =>
			{
				if (store == null)
					store = func ();
				return store;
			};
		}
		/*
		## Calling an Action in an Expression

		Sometimes you need to invoke a method that does not return anything
		(a.k.a an `Action`) in an expression. This can be desirable, for 
		example, to achieve some side effect. For these	cases, we will the 
		following function which will call the action and then return the
		second parameter as-is.
		*/
		public static T ToExpression<T> (Action action, T result)
		{
			action ();
			return result;
		}
	}
}
