namespace ComposeFX.Compiler
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;
	using Extensions;

	public delegate TRes Macro<TRes> ();
	public delegate TRes Macro<T1, TRes> (T1 arg1);
	public delegate TRes Macro<T1, T2, TRes> (T1 arg1, T2 arg2);
	public delegate TRes Macro<T1, T2, T3, TRes> (T1 arg1, T2 arg2, T3 arg3);
	public delegate TRes Macro<T1, T2, T3, T4, TRes> (T1 arg1, T2 arg2, T3 arg3, T4 arg4);
	public delegate TRes Macro<T1, T2, T3, T4, T5, TRes> (T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
	public delegate TRes Macro<T1, T2, T3, T4, T5, T6, TRes> (T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);

	public class Macro
	{
		private static Dictionary<MemberInfo, Macro> _macros =
			new Dictionary<MemberInfo, Macro> ();
		private static int _lastUniqInd;
		internal readonly Ast.Macro AstMacro;
		internal readonly Ast.Program Program;
		internal readonly HashSet<Type> TypesDefined;

		internal Macro (Ast.Macro macro, Ast.Program program, HashSet<Type> typesDefined)
		{
			AstMacro = macro;
			Program = program;
			TypesDefined = typesDefined;
		}

		internal Macro (Ast.Macro macro)
			: this (macro, null, null) { }

		internal static void Add (MemberInfo member, Macro macro)
		{
			_macros.Add (member, macro);
		}

		internal static Macro Get (MemberInfo member)
		{
            return _macros.TryGetValue (member, out Macro result) ? result : null;
        }

		internal static bool IsDefined (MemberInfo member)
		{
			return _macros.ContainsKey (member);
		}

		public static Macro<TRes> Create<TRes> (Expression<Func<Macro<TRes>>> member,
			Ast.Macro macro)
		{
			Add ((member.Body as MemberExpression).Member, new Macro (macro));
			return new Macro<TRes> (() => default (TRes));
		}

		public static Macro<T1, TRes> Create<T1, TRes> (
			Expression<Func<Macro<T1, TRes>>> member, Ast.Macro macro)
		{
			Add ((member.Body as MemberExpression).Member, new Macro (macro));
			return new Macro<T1, TRes> ((a1) => default (TRes));
		}

		public static Macro<T1, T2, TRes> Create<T1, T2, TRes> (
			Expression<Func<Macro<T1, T2, TRes>>> member, Ast.Macro macro)
		{
			Add ((member.Body as MemberExpression).Member, new Macro (macro));
			return new Macro<T1, T2, TRes> ((a1, a2) => default (TRes));
		}

		public static Macro<T1, T2, T3, TRes> Create<T1, T2, T3, TRes> (
			Expression<Func<Macro<T1, T2, T3, TRes>>> member, Ast.Macro macro)
		{
			Add ((member.Body as MemberExpression).Member, new Macro (macro));
			return new Macro<T1, T2, T3, TRes> ((a1, a2, a3) => default (TRes));
		}

		public static Macro<T1, T2, T3, T4, TRes> Create<T1, T2, T3, T4, TRes> (
			Expression<Func<Macro<T1, T2, T3, T4, TRes>>> member, Ast.Macro macro)
		{
			Add ((member.Body as MemberExpression).Member, new Macro (macro));
			return new Macro<T1, T2, T3, T4, TRes> ((a1, a2, a3, a4) => default (TRes));
		}

		public static Macro<T1, T2, T3, T4, T5, TRes> Create<T1, T2, T3, T4, T5, TRes> (
			Expression<Func<Macro<T1, T2, T3, T4, T5, TRes>>> member, Ast.Macro macro)
		{
			Add ((member.Body as MemberExpression).Member, new Macro (macro));
			return new Macro<T1, T2, T3, T4, T5, TRes> ((a1, a2, a3, a4, a5) => default (TRes));
		}

		public static Macro<T1, T2, T3, T4, T5, T6, TRes> Create<T1, T2, T3, T4, T5, T6, TRes> (
			Expression<Func<Macro<T1, T2, T3, T4, T5, T6, TRes>>> member, Ast.Macro macro)
		{
			Add ((member.Body as MemberExpression).Member, new Macro (macro));
			return new Macro<T1, T2, T3, T4, T5, T6, TRes> ((a1, a2, a3, a4, a5, a6) => default (TRes));
		}

		public static bool IsMacroType (Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition ().In (
				typeof (Macro<>), typeof (Macro<,>), typeof (Macro<,,>), typeof (Macro<,,,>),
				typeof (Macro<,,,,>), typeof (Macro<,,,,,>), typeof (Macro<,,,,,,>));
		}

		public static Ast.MacroDefinition GetMacroDefinition (Type type)
		{
			if (!IsMacroType (type))
				throw new ArgumentException ("Given type is not a macro type.", nameof (type));
			var gtypes = type.GetGenericArguments ();
			var argLen = gtypes.Length - 1;
			var res = Ast.MRes (gtypes[argLen]);
			var pars = from t in gtypes.Take (argLen)
					   select IsMacroType (t) ?
							Ast.MDPar (GetMacroDefinition (t)) :
							Ast.MPar (t);
			return Ast.MDef (pars, res);
		}

		public static Ast.Variable GenUniqueVar (Type type, string name)
		{
			return Ast.Var (type, string.Format ("_{0}{1}", name, _lastUniqInd++));
		}

		public static Ast.Function InstantiateAllMacros (Ast.Function function)
		{
			while (true)
			{
				var instantiated = (Ast.Function)function.Transform (node =>
					node is Ast.MacroCall ? InstantiateMacro (node as Ast.MacroCall) : node);
				if (instantiated == function)
					return function;
				function = instantiated;
			}
		}

		private static Ast.Block InstantiateMacro (Ast.MacroCall call)
		{
			var macro = call.Target as Ast.Macro;
			if (macro == null)
				throw new InvalidOperationException ("Trying to instantiate macro definition.");
			if (call.Parameters.Any (mp => mp.GetType () == typeof (Ast.MacroDefinition)))
				throw new InvalidOperationException ("Uninstantiated macro parameter in macro call.");
			return (Ast.Block)macro.Implementation.Transform (node => ReplaceMacroParams (call, node));
		}

		private static Ast ReplaceMacroParams (Ast.MacroCall call, Ast node)
		{
			var macro = call.Target;
			if (node is Ast.MacroDefinition)
			{
				if (node is Ast.Macro)
				{
					var innerMacro = node as Ast.Macro;
					return Ast.Mac (innerMacro.Parameters, innerMacro.Result,
						(Ast.Block)innerMacro.Implementation.Transform (n => ReplaceMacroParams (call, n)));
				}
				var macroDef = node as Ast.MacroDefinition;
				var i = macro.GetMacroDefParamIndex (macroDef);
				if (i >= 0)
					return call.Parameters[i];
			}
			if (node is Ast.MacroParamRef)
			{
				var mp = node as Ast.MacroParamRef;
				var i = macro.GetParamRefIndex (mp);
				if (i >= 0)
					return call.Parameters[i];
			}
			if (node is Ast.MacroResultVar && node == macro.Result)
				return call.ResultVar;
			return node;
		}
	}
}