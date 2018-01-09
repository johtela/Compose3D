/*
# Generic Parameter List

Collection initializers allow easy creation of collection classes. The secret 
sauce that makes a class support collection initializers work is to make a
class implement IEnumerable and define the `Add` method in it. Then you can
initialize the collection in the same way as dictionaries, for example.

The `Params` class is designed to be a base class for any list of parameters. 
The parameter name and value types are generic, and there are practically no
restrictions about what can be stored in the `Params` class. But as the keys
and values are stored in a list, the class should not be used if efficient 
retrieval based on keys is required. In that case dictionary is a better option.
*/
namespace Extensions
{
	using System;
	using System.Collections.Generic;

	public class Params<T, U> : IEnumerable<Tuple<T, U>>
	{
		/*
		## Internal Storage

		The parameters are stored in a list of tuples. The list is created in 
		the default constructor.
		*/
		private List<Tuple<T, U>> _parameters;

		public Params ()
		{
			_parameters = new List<Tuple<T, U>> ();
		}

		/*
		## Adding New Parameters

		The add method is trivial. The method isn't called explicitly, instead
		the compiler generates code that calls it when collection initializer
		is used.
		*/
		public void Add (T parameter, U value)
		{
			_parameters.Add (Tuple.Create (parameter, value));
		}
		/*
		## Retrieving a Parameter Value

		The indexer property can be used to retrieve a paremeter value, if key
		is given. The implementation linearly searches for a matching parameter,
		so its time complexity is _O(n)_.
		*/
		public U this[T parameter]
		{
			get
			{
				return _parameters.FindLast (p => p.Equals (parameter)).Item2;
			}
		}
		/*
		## IEnumerable Implementation

		The implementation for IEnumerable is also trivial, it just delegates 
		everything to the List class.
		*/
		public IEnumerator<Tuple<T, U>> GetEnumerator ()
		{
			return _parameters.GetEnumerator ();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}
}