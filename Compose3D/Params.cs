namespace Compose3D
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Params<T> : IEnumerable<Tuple<T, object>>
    {
        private List<Tuple<T, object>> _parameters;

        public Params ()
        {
            _parameters = new List<Tuple<T, object>> ();
        }

        public void Add (T parameter, object value)
        {
            _parameters.Add (Tuple.Create (parameter, value));
        }

        public object this[T parameter]
        {
            get
            {
                return _parameters.FindLast (p => p.Equals (parameter));
            }
        }
 
        public IEnumerator<Tuple<T, object>> GetEnumerator ()
        {
            return _parameters.GetEnumerator ();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return GetEnumerator ();
        }
    }
}
