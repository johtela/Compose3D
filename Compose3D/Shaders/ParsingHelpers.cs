namespace Compose3D.Shaders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
	using GLTypes;
	
    public class Source
    {
        private IEnumerator<MethodCallExpression> _enumerator;
        private bool _atEnd;

        public Source (IEnumerable<MethodCallExpression> e)
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

        public MethodCallExpression Current
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

        public static Expression SkipUnary (this Expression expr, ExpressionType et) 
        {
            var res = expr.CastExpr<UnaryExpression> (et);
            return res == null ? expr : res.Operand;
        }

        public static IEnumerable<MethodCallExpression> Traverse (this Expression expr)
        {
			var mcexpr = expr.GetSelect ();
            if (mcexpr != null)
            {
                foreach (var sexpr in mcexpr.Arguments[0].Traverse ())
                    yield return sexpr;
                yield return mcexpr;
            }
        }

		public static MethodCallExpression GetSelect (this Expression expr)
		{
			var mcexpr = expr as MethodCallExpression;
			return mcexpr != null && mcexpr.Method.IsSelect () ? mcexpr : null;
		}

        public static LambdaExpression GetSelectLambda (this MethodCallExpression expr)
        {
			return expr.Arguments[1].ExpectLambda ();
        }

		public static ParameterExpression GetLambdaParameter (this Expression expr, int index)
		{
			return expr.ExpectLambda ().Parameters [index];
		}

        public static MethodCallExpression ExpectSelect (this Expression expr)
        {
            var result = expr.Expect<MethodCallExpression> (ExpressionType.Call);
            if (!result.Method.IsSelect ())
                throw new ParseException ("Expected a Select or SelectMany call");
            return result;
        }

        public static LambdaExpression ExpectLambda (this Expression expr)
        {
            return expr.Expect<LambdaExpression> (ExpressionType.Lambda);
        }

        public static bool IsSelect (this MethodInfo mi)
        {
			return (mi.DeclaringType == typeof (Shader) || mi.DeclaringType == typeof (Enumerable))
				&& (mi.Name == "Select" || mi.Name == "SelectMany");
        }

		public static bool IsSelectMany (this MethodInfo mi)
		{
			return mi.DeclaringType == typeof (Shader) && mi.Name == "SelectMany";
		}

		public static bool IsAggregate (this MethodInfo mi)
        {
			if (mi.DeclaringType != typeof (Enumerable) || mi.Name != "Aggregate")
                return false;
            if (mi.GetGenericArguments ().Length != 2)
                throw new ParseException ("The only supported overload of Aggregate is one containing two generic parameters.");
            return true;
        }

		public static bool IsRange (this MethodInfo mi)
		{
			return mi.DeclaringType == typeof (Enumerable) && mi.Name == "Range";
		}

		public static bool IsEvaluate (this MethodInfo mi)
		{
			return mi.DeclaringType == typeof (Shader) || mi.Name == "Evaluate";
		}
		       
		public static bool ParseLambda (this Source source, Func<LambdaExpression, NewExpression, bool> func)
        {
            var lambdaExpr = source.Current.GetSelectLambda ();
            if (lambdaExpr == null)
                return false;
            var newExpr = lambdaExpr.Body.CastExpr<NewExpression> (ExpressionType.New);
            if (newExpr == null)
                return false;
            return func (lambdaExpr, newExpr);
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

        public static Parser SkipOne ()
        {
            return ExactlyOne (source => true);
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
