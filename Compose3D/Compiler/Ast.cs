namespace Compose3D.Compiler
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Extensions;

	public abstract class Ast
	{
		public virtual Ast Transform (Func<Ast, Ast> transform)
		{
			return transform (this);
		}

		internal abstract class _Expression : Ast
		{
		}

		internal class _Literal : _Expression
		{
			public readonly string Value;

			protected _Literal (string value)
			{
				Value = value;
			}

			public override string ToString ()
			{
				return Value;
			}
		}

		internal class _Variable : _Expression
		{
			public readonly string Type;
			public readonly string Name;

			protected _Variable (string type, string name)
			{
				Type = type;
				Name = name;
			}

			public override string ToString ()
			{
				return string.Format ("{0} {1}", Type, Name);
			}
		}

		internal class _Field : _Variable
		{
			public _Field (string type, string name) : base (type, name) { }
		}

		internal class _Argument : _Variable
		{
			public _Argument (string type, string name) : base (type, name) { }
		}

		internal class _FunctionArgument : _Argument
		{
			public _FunctionArgument (string name) : base ("function", name) { }
		}

		internal class _Constant : _Variable
		{
			public _Constant (string type, string name) : base (type, name) { }
		}

		internal class _VariableRef : _Expression
		{
			public readonly _Variable Variable;

			protected _VariableRef (_Variable variable)
			{
				Variable = variable;
			}

			public override string ToString ()
			{
				return Variable.Name;
			}
		}

		internal class _FunctionRef : _Expression
		{
			public readonly _Function Function;

			protected _FunctionRef (_Function function)
			{
				Function = function;
			}

			public override string ToString ()
			{
				return Function.Name;
			}
		}

		internal class _FieldRef : _Expression
		{
			public readonly _Expression Expression;
			public readonly _Field Field;

			protected _FieldRef (_Expression expression, _Field field)
			{
				Expression = expression;
				Field = field;
			}

			public override string ToString ()
			{
				return string.Format ("{0}.{1}", Expression, Field);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var expr = (_Expression)transform (Expression);
				return transform (expr == Expression ? this :
					new _FieldRef (expr, Field));
			}
		}

		internal class _UnaryOperation : _Expression
		{
			public readonly bool Prefix;
			public readonly string Operator;
			public readonly _Expression Argument;

			protected _UnaryOperation (bool prefix, string oper, _Expression argument)
			{
				Prefix = prefix;
				Operator = oper;
				Argument = argument;
			}

			public override string ToString ()
			{
				return Prefix ? Operator + Argument : Argument + Operator;
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var argument = (_Expression)transform (Argument);
				return transform (argument == Argument ?  this : 
					new _UnaryOperation (Prefix, Operator, argument));
			}
		}

		internal class _BinaryOperation : _Expression
		{
			public readonly string Operator;
			public readonly _Expression LeftArgument;
			public readonly _Expression RightArgument;

			protected _BinaryOperation (string oper, _Expression left, _Expression right)
			{
				Operator = oper;
				LeftArgument = left;
				RightArgument = right;
			}

			public override string ToString ()
			{
				return string.Format ("{0} {1} {2}", LeftArgument, Operator, RightArgument);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var left = (_Expression)transform (LeftArgument);
				var right = (_Expression)transform (RightArgument);
				return transform (left == LeftArgument && right == RightArgument ? this :
					new _BinaryOperation (Operator, left, right));
			}
		}

		internal class _Call : _Expression
		{
			public readonly _FunctionRef Function;
			public readonly _Expression[] Arguments;

			protected _Call (_FunctionRef function, IEnumerable<_Expression> arguments)
			{
				Function = function;
				Arguments = arguments.ToArray ();
				if (function.Function.Arguments.Length != Arguments.Length)
					throw new ArgumentException ("Invalid number of arguments.", nameof (arguments));
				for (int i = 0; i < Arguments.Length; i++)
					if (function.Function.Arguments[i] is _FunctionArgument && !(Arguments[i] is _FunctionRef))
						throw new ArgumentException ("Invalid function argument for higher order function.\n" +
							"Must be a function reference.");
			}

			public override string ToString ()
			{
				return string.Format ("{0} ({1})", Function,
					Arguments.Select (e => e.ToString ()).SeparateWith (", "));
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var func = (_FunctionRef)transform (Function);
				var args = Arguments.Select (a => (_Expression)transform (a));
				return transform (func == Function && args.SequenceEqual (Arguments) ? this :
					new _Call (func, args));
			}
		}

		internal class _NewArray : _Expression
		{
			public readonly string ItemType;
			public readonly int ItemCount;
			public readonly _Expression[] Items;

			protected _NewArray (string type, int count, IEnumerable<_Expression> items)
			{
				if (items.Count () != count)
					throw new ArgumentException ("Invalid number arguments", nameof (items));
				ItemType = type;
				ItemCount = count;
				Items = items.ToArray ();
			}

			public override string ToString ()
			{
				return string.Format ("{0}[{1}] ( {2} )", ItemType, ItemCount,
					Items.Select (e => e.ToString ()).SeparateWith (", "));
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var items = Items.Select (i => (_Expression)transform (i));
				return transform (items.SequenceEqual (Items) ? this :
					new _NewArray (ItemType, ItemCount, items));
			}
		}

		internal class _Conditional : _Expression
		{
			public readonly _Expression Condition;
			public readonly _Expression IfTrue;
			public readonly _Expression IfFalse;

			protected _Conditional (_Expression condition, _Expression ifTrue, _Expression ifFalse)
			{
				Condition = condition;
				IfTrue = ifTrue;
				IfFalse = ifFalse;
			}

			public override string ToString ()
			{
				return string.Format ("{0} ? {1} : {2}", Condition, IfTrue, IfFalse);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var cond = (_Expression)transform (Condition);
				var iftrue = (_Expression)transform (IfTrue);
				var iffalse = (_Expression)transform (IfFalse);
				return transform (cond == Condition && iftrue == IfTrue && iffalse == IfFalse ? this :
					new _Conditional (cond, iftrue, iffalse));
			}
		}

		internal abstract class _Statement : Ast
		{
			[ThreadStatic]
			protected static int _tabLevel;

			protected string Tabs ()
			{
				var sb = new StringBuilder ();
				for (int i = 0; i < _tabLevel; i++)
					sb.Append ("    ");
				return sb.ToString ();
			}

			protected void BeginScope (StringBuilder sb)
			{
				sb.AppendFormat ("{0}{{\n", Tabs ());
				_tabLevel++;
			}

			protected void EndScope (StringBuilder sb)
			{
				_tabLevel--;
				sb.AppendFormat ("{0}}}\n", Tabs ());
			}

			protected void EmitIntended (StringBuilder sb, _Statement statement)
			{
				if (statement is _Block)
					sb.Append (statement);
				else
				{
					_tabLevel++;
					sb.Append (statement);
					_tabLevel--;
				}
			}

			protected void EmitLine (StringBuilder sb, string formatString, params object[] parameters)
			{
				sb.AppendFormat (Tabs () + formatString + "\n", parameters);
			}
		}

		internal class _Assignment : _Statement
		{
			public readonly _VariableRef Variable;
			public readonly _Expression Value;

			protected _Assignment (_VariableRef variable, _Expression value)
			{
				Variable = variable;
				Value = value;
			}

			public override string ToString ()
			{
				return string.Format ("{0}{1} = {2};\n", Tabs (), Variable, Value);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var vari = (_VariableRef)transform (Variable);
				var val = (_Expression)transform (Value);
				return transform (vari == Variable && val == Value ? this :
					new _Assignment (vari, val));
			}
		}

		internal class _DeclareVar : _Statement
		{
			public readonly _Variable Variable;
			public readonly _Expression Value;

			protected _DeclareVar (_Variable variable, _Expression value)
			{
				Variable = variable;
				Value = value;
			}

			public override string ToString ()
			{
				return Value != null ?
					string.Format ("{0}{1} = {2};\n", Tabs (), Variable, Value) :
					string.Format ("{0}{1};\n", Tabs (), Variable);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var vari = (_Variable)transform (Variable);
				var val = (_Expression)transform (Value);
				return transform (vari == Variable && val == Value ? this :
					new _DeclareVar (vari, val));
			}
		}

		internal class _Block : _Statement
		{
			public readonly List<_Statement> Statements;

			protected _Block (IEnumerable<_Statement> statements)
			{
				Statements = new List<_Statement> (statements);
			}

			public override string ToString ()
			{
				var result = new StringBuilder ();
				BeginScope (result);
				foreach (var statement in Statements)
					result.Append (statement);
				EndScope (result);
				return result.ToString ();
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var stats = Statements.Select (i => (_Statement)transform (i));
				return transform (stats.SequenceEqual (Statements) ? this :
					new _Block (stats));
			}
		}

		internal class _If : _Statement
		{
			public readonly _Expression Condition;
			public readonly _Statement IfTrue;
			public readonly _Statement IfFalse;

			protected _If (_Expression condition, _Statement ifTrue, _Statement ifFalse)
			{
				Condition = condition;
				IfTrue = ifTrue;
				IfFalse = ifFalse;
			}

			public override string ToString ()
			{
				var result = new StringBuilder ();
				EmitLine (result, "if ({0})\n", Condition);
				EmitIntended (result, IfTrue);
				if (IfFalse != null)
				{
					EmitLine (result, "else");
					EmitIntended (result, IfFalse);
				}
				return result.ToString ();
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var cond = (_Expression)transform (Condition);
				var iftrue = (_Statement)transform (IfTrue);
				var iffalse = (_Statement)transform (IfFalse);
				return transform (cond == Condition && iftrue == IfTrue && iffalse == IfFalse ? this :
					new _If (cond, iftrue, iffalse));
			}
		}

		internal class _For : _Statement
		{
			public readonly _Variable LoopVariable;
			public readonly _Expression InitialValue;
			public readonly _Expression Condition;
			public readonly _Expression Increment;
			public readonly _Statement Body;

			protected _For (_Variable loopVariable, _Expression initialValue, _Expression condition,
				_Expression increment, _Statement body)
			{
				LoopVariable = loopVariable;
				InitialValue = initialValue;
				Condition = condition;
				Increment = increment;
				Body = body;
			}

			public override string ToString ()
			{
				var result = new StringBuilder ();
				EmitLine (result, "for ({0} = {1}; {2}; {3} = {4})\n", LoopVariable, InitialValue,
					Condition, LoopVariable.Name, Increment);
				EmitIntended (result, Body);
				return result.ToString ();
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var loopvar = (_Variable)transform (LoopVariable);
				var initval = (_Expression)transform (InitialValue);
				var cond = (_Expression)transform (Condition);
				var incr = (_Expression)transform (Increment);
				var body = (_Statement)transform (Body);
				return transform (loopvar == LoopVariable && initval == InitialValue &&
					cond == Condition && incr == Increment && body == Body ? this :
					new _For (loopvar, initval, cond, incr, body));
			}
		}

		internal class _Return : _Statement
		{
			public readonly _Expression Value;

			protected _Return (_Expression value)
			{
				Value = value;
			}

			public override string ToString ()
			{
				return string.Format ("{0}return {1};\n", Tabs (), Value);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var val = (_Expression)transform (Value);
				return transform (val == Value ? this : new _Return (val));
			}
		}

		internal abstract class _Global : Ast
		{
		}

		internal class _Function : _Global
		{
			public readonly string Name;
			public readonly string ReturnType;
			public readonly _Argument[] Arguments;
			public readonly _Block Body;

			protected _Function (string name, string returnType, IEnumerable<_Argument> arguments,
				_Block body)
			{
				Name = name;
				ReturnType = returnType;
				Arguments = arguments.ToArray ();
				Body = body;
			}

			public bool IsHigherOrder
			{
				get { return Arguments.Any (a => a is _FunctionArgument); }
			}

			public override string ToString ()
			{
				return string.Format ("{0} {1} ({2})\n{3}", ReturnType, Name,
					Arguments.Select (v => v.ToString ()).SeparateWith (", "), Body);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var args = Arguments.Select (a => (_Argument)transform (a));
				var body = (_Block)transform (Body);
				return transform (args.SequenceEqual (Arguments) && body == Body ? this :
					new _Function (Name, ReturnType, args, body));
			}
		}

		internal class _Struct : _Global
		{
			public readonly string Name;
			public readonly List<_Field> Fields;

			protected _Struct (string name, IEnumerable<_Field> fields)
			{
				Name = name;
				Fields = new List<_Field> (fields);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var fields = Fields.Select (f => (_Field)transform (f));
				return transform (fields.SequenceEqual (Fields) ? this :
					new _Struct (Name, fields));
			}
		}

		internal class _ConstantDecl : _Global
		{
			public readonly _Constant Constant;
			public readonly _Expression Value;

			protected _ConstantDecl (_Constant constant, _Expression value)
			{
				Constant = constant;
				Value = value;
			}

			public override string ToString ()
			{
				return string.Format ("const {0} = {1};\n", Constant, Value);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var con = (_Constant)transform (Constant);
				var val = (_Expression)transform (Value);
				return transform (con == Constant && val == Value ? this :
					new _ConstantDecl (con, val));
			}
		}

		internal class _Program : Ast
		{
			public readonly List<_Global> Globals;

			protected _Program (IEnumerable<_Global> globals)
			{
				Globals = new List<_Global> (globals);
			}

			public override string ToString ()
			{
				var result = new StringBuilder ();
				foreach (var glob in Globals)
				{
					result.Append (glob);
					result.AppendLine ();
				}
				return result.ToString ();
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var globs = Globals.Select (g => (_Global)transform (g));
				return transform (globs.SequenceEqual (Globals) ? this :
					new _Program (globs));
			}
		}
	}
}
