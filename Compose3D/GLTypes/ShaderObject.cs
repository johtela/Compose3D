namespace Compose3D.GLTypes
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using OpenTK.Graphics.OpenGL;
    using System.Collections.Generic;

    public enum ShaderObjectKind { Input, Output, Uniform }

    public class ShaderObject<T> : IQueryable<T>
    {
        private class ShaderProvider : IQueryProvider
        {
            public IQueryable<U> CreateQuery<U> (Expression expression)
            {
                return new ShaderObject<U> (ShaderObjectKind.Output, expression);
            }

            public IQueryable CreateQuery (Expression expression)
            {
                throw new NotImplementedException ();
            }

            public U Execute<U> (Expression expression)
            {
                throw new NotImplementedException ();
            }

            public object Execute (Expression expression)
            {
                throw new NotImplementedException ();
            }
        }

        private static ShaderProvider _builder = new ShaderProvider ();
        private ShaderObjectKind _kind;
        private Expression _expr;

        public ShaderObject (ShaderObjectKind kind)
        {
            _kind = kind;
            var ci = GetType ().GetConstructor (new Type[] { typeof (ShaderObjectKind) });
            _expr = Expression.New (ci, Expression.Constant(_kind));
        }

        internal ShaderObject (ShaderObjectKind kind, Expression expr)
        {
            _kind = kind;
            _expr = expr;
        }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator ()
        {
            throw new NotImplementedException ();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            throw new NotImplementedException ();
        }

        public Type ElementType
        {
            get { return typeof (T); }
        }

        public Expression Expression
        {
            get { return _expr; }
        }

        public IQueryProvider Provider
        {
            get { return _builder; }
        }
    }
}
