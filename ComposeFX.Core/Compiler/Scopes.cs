namespace ComposeFX.Compiler
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;

	public class Scope 
	{
		public readonly Scope Parent;
		public readonly Ast.Block Block;
		public readonly Dictionary<string, Ast.Variable> LocalVars;

		protected Scope (Scope parent, Ast.Block block)
		{
			Parent = parent;
			Block = block;
			LocalVars = new Dictionary<string, Ast.Variable> ();
		}

		public void CodeOut (Ast.Statement statement)
		{
			Block.Statements.Add (statement);
		}

		public void AddLocal (string name, Ast.Variable variable)
		{
			LocalVars.Add (name, variable);
		}

		public Ast.Variable DeclareLocal (Type type, string name, Ast.Expression value)
		{
			return DeclareLocal (Ast.Var (type, name), value);
		}

		public Ast.Variable DeclareLocal (Ast.Variable local, Ast.Expression value)
		{
			CodeOut (Ast.DeclVar (local, value));
			LocalVars.Add (local.Name, local);
			return local;
		}

		public Ast.Variable FindLocalVar (string name)
		{
            if (LocalVars.TryGetValue (name, out Ast.Variable result))
                return result;
            else if (Parent != null)
                return Parent.FindLocalVar (name);
            return null;
		}

		public Ast.MacroParam FindMacroParam (ParameterExpression parameter)
		{
			var mscope = this;
			while (mscope != null)
			{
				if (mscope is MacroScope ms && 
                    ms.MacroParams.TryGetValue (parameter, out Ast.MacroParam result))
                    return result;
				mscope = mscope.Parent;
			}
			return null;
		}

		public static Scope Begin (Scope parent, Ast.Block block)
		{
			return new Scope (parent, block);
		}

		public Scope End ()
		{
			return Parent;
		}
	}

	public class MacroScope : Scope
	{
		public readonly Dictionary<ParameterExpression, Ast.MacroParam> MacroParams;

		protected MacroScope (Scope parent, Ast.Block block, 
			IEnumerable<KeyValuePair<ParameterExpression, Ast.MacroParam>> macroParams)
			: base (parent, block)
		{
			MacroParams = new Dictionary<ParameterExpression, Ast.MacroParam> ();
			foreach (var kv in macroParams)
				MacroParams.Add (kv.Key, kv.Value);
		}

		public static Scope Begin (Scope parent, Ast.Block block,
			IEnumerable<KeyValuePair<ParameterExpression, Ast.MacroParam>> macroParams)
		{
			return new MacroScope (parent, block, macroParams);
		}
	}
}
