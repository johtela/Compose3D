namespace Extensions
{
    using System;
    using System.Collections.Generic;

    public class Params<T, U> : IEnumerable<Tuple<T, U>>
    {
        private List<Tuple<T, U>> _parameters;

        public Params ()
        {
            _parameters = new List<Tuple<T, U>> ();
        }

        public void Add (T parameter, U value)
        {
            _parameters.Add (Tuple.Create (parameter, value));
        }

        public U this[T parameter]
        {
            get
            {
                return _parameters.FindLast (p => p.Equals (parameter)).Item2;
            }
        }
 
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
