namespace Compose3D.Compiler.Ast
{
	using System;
	using System.Linq;
	using Extensions;
	using System.Text;

	public abstract class Ast
	{
		internal abstract class _Expression : Ast
		{
		}

		internal class _Constant : _Expression
		{
			public readonly string Value;

			private _Constant (string value)
			{
				Value = value;
			}

			public override string ToString ()
			{
				return Value;
			}
		}

		internal class _VariableRef : _Expression
		{
			public readonly string VariableName;

			public _VariableRef (string name)
			{
				VariableName = name;
			}

			public override string ToString ()
			{
				return VariableName;
			}
		}

		internal class _FieldRef : _Expression
		{
			public readonly _Expression Expression;
			public readonly string FieldName;

			public _FieldRef (_Expression expression, string name)
			{
				Expression = expression;
				FieldName = name;
			}

			public override string ToString ()
			{
				return string.Format ("{0}.{1}", Expression, FieldName);
			}
		}

		internal class _UnaryOparation : _Expression
		{
			public readonly bool Prefix;
			public readonly string Operator;
			public readonly _Expression Argument;

			private _UnaryOparation (bool prefix, string oper, _Expression argument)
			{
				Prefix = prefix;
				Operator = oper;
				Argument = argument;
			}

			public override string ToString ()
			{
				return Prefix ? Operator + Argument : Argument + Operator;
			}
		}

		internal class _BinaryOparation : _Expression
		{
			public readonly string Operator;
			public readonly _Expression LeftArgument;
			public readonly _Expression RightArgument;

			private _BinaryOparation (string oper, _Expression left, _Expression right)
			{
				Operator = oper;
				LeftArgument = left;
				RightArgument = right;
			}

			public override string ToString ()
			{
				return string.Format ("{0} {1} {2}", LeftArgument, Operator, RightArgument);
			}
		}

		internal class _Call : _Expression
		{
			public readonly string FunctionName;
			public readonly _Expression[] Parameters;

			public _Call (string functionName, params _Expression[] parameters)
			{
				FunctionName = functionName;
				Parameters = parameters;
			}

			public override string ToString ()
			{
				return string.Format ("{0} ({1})", FunctionName,
					Parameters.Select (e => e.ToString ()).SeparateWith (", "));
			}
		}

		internal class _NewArray : _Expression
		{
			public readonly string ItemType;
			public readonly int ItemCount;
			public readonly _Expression[] Items;

			public _NewArray (string type, int count, params _Expression[] items)
			{
				ItemType = type;
				ItemCount = count;
				Items = items;
			}

			public override string ToString ()
			{
				return string.Format ("{0}[{1}] ( {2} )", ItemType, ItemCount,
					Items.Select (e => e.ToString ()).SeparateWith (", "));
			}
		}

		internal class _Conditional : _Expression
		{
			public readonly _Expression Condition;
			public readonly _Expression IfTrue;
			public readonly _Expression IfFalse;

			public _Conditional (_Expression condition, _Expression ifTrue, _Expression ifFalse)
			{
				Condition = condition;
				IfTrue = ifTrue;
				IfFalse = ifFalse;
			}

			public override string ToString ()
			{
				return string.Format ("{0} ? {1} : {2}", Condition, IfTrue, IfFalse);
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
				sb.AppendFormat ("{0}}}\n", Tabs ());
				_tabLevel--;
			}

			protected void Scoped (StringBuilder sb, _Statement statement)
			{
				BeginScope (sb);
				sb.Append (statement);
				EndScope (sb);
			}

			protected void EmitLine (StringBuilder sb, string formatString, params object[] parameters)
			{
				sb.AppendFormat (Tabs () + formatString + "\n", parameters);
			}
		}

		internal class _Assignment : _Statement
		{
			public readonly string Variable;
			public readonly _Expression Value;

			public _Assignment (string variable, _Expression value)
			{
				Variable = variable;
				Value = value;
			}

			public override string ToString ()
			{
				return string.Format ("{0}{1} = {2};\n", Tabs (), Variable, Value);
			}
		}

		internal class _DeclareVar : _Statement
		{
			public readonly string Type;
			public readonly string Variable;
			public readonly _Expression Value;

			public _DeclareVar (string type, string variable, _Expression value)
			{
				Type = type;
				Variable = variable;
				Value = value;
			}

			public override string ToString ()
			{
				return Value != null ?
					string.Format ("{0}{1} {2} = {3};\n", Tabs (), Type, Variable, Value) :
					string.Format ("{0}{1} {2};\n", Tabs (), Type, Variable);
			}
		}

		internal class _If : _Statement
		{
			public readonly _Expression Condition;
			public readonly _Statement IfTrue;
			public readonly _Statement IfFalse;

			public _If (_Expression condition, _Statement ifTrue, _Statement ifFalse)
			{
				Condition = condition;
				IfTrue = ifTrue;
				IfFalse = ifFalse;
			}

			public override string ToString ()
			{
				var result = new StringBuilder ();
				EmitLine (result, "if ({0})", Condition);
				Scoped (result, IfTrue);
				if (IfFalse != null)
				{
					EmitLine (result, "else");
					Scoped (result, IfFalse);
				}
				return result.ToString ();
			}
		}

		internal class _For : _Statement
		{
			public readonly string LoopVariable;
			public readonly _Expression InitialValue;
			public readonly _Expression Condition;
			public readonly _Expression Increment;
			public readonly _Statement Body;

			public _For (string loopVariable, _Expression initialValue, _Expression condition,
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
				EmitLine (result, "for ({0} = {1}; {2}; {0} = {3})", LoopVariable, InitialValue,
					Condition, Increment);
				Scoped (result, Body);
				return result.ToString ();
			}
		}

		internal class _Return : _Statement
		{
			public readonly _Expression Value;

			public _Return (_Expression value)
			{
				Value = value;
			}

			public override string ToString ()
			{
				return string.Format ("{0}return {1};", Tabs (), Value);
			}
		}
	}
}
