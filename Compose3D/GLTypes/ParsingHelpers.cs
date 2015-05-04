namespace Compose3D.GLTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class Source
    {
        private IEnumerator<Expression> _enumerator;
        private bool _atEnd;

        public Source (IEnumerable<Expression> e)
        {
            _enumerator = e.GetEnumerator ();
            _atEnd = !_enumerator.MoveNext ();
        }

        private void CheckNotAtEnd ()
        {
            if (_atEnd)
                throw new InvalidOperationException ("Source is exhausted.");
        }

        public bool AtEnd
        {
            get { return _atEnd; }
        }

        public Expression Current
        {
            get 
            {
                CheckNotAtEnd ();
                return _enumerator.Current;
            }
        }

        public void Consume ()
        {
            CheckNotAtEnd ();
            _atEnd = !_enumerator.MoveNext ();
        }
    }

    public delegate bool Parser (Source source);

    public class ParseException : Exception
    {
        public ParseException (string msg) : base (msg) { }
    }

    public static class Parse
    {
        public static T CastExpr<T> (this Expression expr, ExpressionType type) where T : Expression
        {
            var res = expr as T;
            return res != null && res.NodeType == type ? res : null;
        }

        public static T Expect<T> (this Expression expr, ExpressionType type) where T : Expression
        {
            var res = CastExpr<T> (expr, type);
            if (res == null)
                throw new ParseException (
                    string.Format ("Unexpected expression '{0}'.\n" + 
                    "Expected '{1}' of type '{2}', encountered '{3}' of type '{4}'",
                    expr, type, typeof (T), expr.NodeType, expr.GetType ()));
            return res;
        }

        public static IEnumerable<Expression> Traverse (this Expression expr)
        {
            var mcexpr = expr as MethodCallExpression;
            if (mcexpr == null)
                yield return expr;
            else
            {
                if (!mcexpr.Method.IsSelect ())
                    throw new ParseException ("Expected a Select or SelectMany call");
                foreach (var sexpr in mcexpr.Arguments[0].Traverse ())
                    yield return sexpr;
                yield return mcexpr.Arguments[1].Expect<UnaryExpression> (ExpressionType.Quote)
                    .Operand.Expect<LambdaExpression> (ExpressionType.Lambda).Body;
            }
        }

        public static bool IsSelect (this MethodInfo mi)
        {
            return mi.DeclaringType == typeof (Queryable) && (mi.Name == "Select" || mi.Name == "SelectMany");
        }

        public static bool IsAggregate (this MethodInfo mi)
        {
            return mi.DeclaringType == typeof (Queryable) && (mi.Name == "Aggregate");
        }

        public static Parser ExactlyOne (this Parser parser)
        {
            return source =>
            {
                var res = !source.AtEnd && parser (source);
                if (res)
                    source.Consume ();
                return res;
            };
        }

        public static Parser ZeroOrMore (this Parser parser)
        {
            return source =>
            {
                while (!source.AtEnd && parser (source)) 
                    source.Consume ();
                return true;
            };
        }

        public static Parser Then (this Parser parser, Parser next)
        {
            return source =>
            {
                if (!parser (source))
                    return false;
                return next (source);
            };
        }

        public static Parser OneOrMore (this Parser parser)
        {
            return parser.ExactlyOne ().Then (parser.ZeroOrMore ());
        }

        public static Parser IfFail (this Parser parser, Exception ex)
        {
            return source =>
            {
                if (!parser (source))
                    throw ex;
                return true;
            };
        }

        public static Parser IfSucceed (this Parser parser, Action action)
        {
            return source =>
            {
                var res = parser (source);
                if (res) action ();
                return res;
            };
        }

        public static void Execute (this Parser parser, Source source)
        {
            if (!parser (source))
                throw new ParseException ("Parse failed.");
            if (!source.AtEnd)
                throw new ParseException ("Unexpexted expressions after the end.");
        }
    }
}
