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

		internal abstract class Expression : Ast
		{
		}

		internal class Literal : Expression
		{
			public readonly string Value;

			protected Literal (string value)
			{
				Value = value;
			}

			public override string ToString ()
			{
				return Value;
			}
		}

		internal class Variable : Expression
		{
			public readonly string Type;
			public readonly string Name;

			protected Variable (string type, string name)
			{
				Type = type;
				Name = name;
			}

			public override string ToString ()
			{
				return string.Format ("{0} {1}", Type, Name);
			}
		}

		internal class Field : Variable
		{
			public Field (string type, string name) : base (type, name) { }
		}

		internal class Argument : Variable
		{
			public Argument (string type, string name) : base (type, name) { }
		}

		internal class Constant : Variable
		{
			public Constant (string type, string name) : base (type, name) { }
		}

		internal class VariableRef : Expression
		{
			public readonly Variable Target;

			protected VariableRef (Variable target)
			{
				Target = target;
			}

			public override string ToString ()
			{
				return Target.Name;
			}
		}

		internal class FunctionArgument : Ast
		{
			public readonly string Name;

			public FunctionArgument (string name)
			{
				Name = name;
			}
		}

		internal class MacroParam : Ast
		{
			public readonly string Name;

			public MacroParam (string name)
			{
				Name = name;
			}

			public override string ToString ()
			{
				return Name;
			}
		}

		internal class MacroParamRef : Expression
		{
			public readonly MacroParam Target;

			public MacroParamRef (MacroParam target)
			{
				Target = target;
			}
		}

		internal class MacroDefinition : Ast
		{
			public readonly MacroParam Name;
			public readonly MacroDefinition[] MacroParameters;
			public readonly MacroParam[] Parameters;

			public MacroDefinition (MacroParam name, IEnumerable<MacroDefinition> macroParams, 
				IEnumerable<MacroParam> parameters)
			{
				Name = name;
				MacroParameters = macroParams.ToArray ();
				Parameters = parameters.ToArray ();
			}

			public override string ToString ()
			{
				return string.Format ("{0} ({1}, {2})", Name,
					MacroParameters.Select (v => v.ToString ()).SeparateWith (", "),
					Parameters.Select (v => v.ToString ()).SeparateWith (", "));
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var ret = (MacroParam)transform (Name);
				var mpars = MacroParameters.Select (p => (MacroDefinition)transform (p));
				var pars = Parameters.Select (p => (MacroParam)transform (p));
				return transform (ret == Name && mpars.SequenceEqual (MacroParameters) 
					&& pars.SequenceEqual (Parameters) ? this :
					new MacroDefinition (ret, mpars, pars));
			}

			public static bool AreCompatible (MacroDefinition def, MacroDefinition other)
			{
				return def.MacroParameters.Length == other.MacroParameters.Length &&
					def.Parameters.Length == other.Parameters.Length &&
					def.MacroParameters.Zip (other.MacroParameters, AreCompatible).All (Fun.Identity);
			}
		}

		internal class Macro : MacroDefinition
		{
			public readonly Block Implementation;

			internal Macro (MacroParam returnParam, IEnumerable<MacroDefinition> macroParams,
				IEnumerable<MacroParam> parameters, Block implementation)
				: base (returnParam, macroParams, parameters)
			{
				Implementation = implementation;
			}

			public override string ToString ()
			{
				return base.ToString () + "\n" + Implementation;
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var def = (MacroDefinition)base.Transform (transform);
				var impl = (Block)transform (Implementation);
				return transform (def == this && impl == Implementation ? this :
					new Macro (def.Name, def.MacroParameters, def.Parameters, Implementation));
			}
		}

		internal abstract class FunctionRef : Ast { }

		internal class NamedFunctionRef : FunctionRef
		{
			public readonly Function Target;

			internal NamedFunctionRef (Function target)
			{
				Target = target;
			}

			public override string ToString ()
			{
				return Target.Name;
			}
		}

		internal class FunctionArgumentRef : FunctionRef
		{
			public readonly FunctionArgument Target;

			protected FunctionArgumentRef (FunctionArgument target)
			{
				Target = target;
			}

			public override string ToString ()
			{
				return Target.Name;
			}
		}

		internal class FieldRef : Expression
		{
			public readonly Expression TargetExpr;
			public readonly Field TargetField;

			protected FieldRef (Expression expression, Field field)
			{
				TargetExpr = expression;
				TargetField = field;
			}

			public override string ToString ()
			{
				return string.Format ("{0}.{1}", TargetExpr, TargetField);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var expr = (Expression)transform (TargetExpr);
				return transform (expr == TargetExpr ? this :
					new FieldRef (expr, TargetField));
			}
		}

		internal class UnaryOperation : Expression
		{
			public readonly bool Prefix;
			public readonly string Operator;
			public readonly Expression Operand;

			protected UnaryOperation (bool prefix, string oper, Expression operand)
			{
				Prefix = prefix;
				Operator = oper;
				Operand = operand;
			}

			public override string ToString ()
			{
				return Prefix ? Operator + Operand : Operand + Operator;
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var argument = (Expression)transform (Operand);
				return transform (argument == Operand ?  this : 
					new UnaryOperation (Prefix, Operator, argument));
			}
		}

		internal class BinaryOperation : Expression
		{
			public readonly string Operator;
			public readonly Expression LeftOperand;
			public readonly Expression RightOperand;

			protected BinaryOperation (string oper, Expression left, Expression right)
			{
				Operator = oper;
				LeftOperand = left;
				RightOperand = right;
			}

			public override string ToString ()
			{
				return string.Format ("{0} {1} {2}", LeftOperand, Operator, RightOperand);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var left = (Expression)transform (LeftOperand);
				var right = (Expression)transform (RightOperand);
				return transform (left == LeftOperand && right == RightOperand ? this :
					new BinaryOperation (Operator, left, right));
			}
		}

		internal class FunctionCall : Expression
		{
			public readonly FunctionRef FuncRef;
			public readonly Expression[] Arguments;

			internal FunctionCall (FunctionRef funcref, IEnumerable<Expression> args)
			{
				FuncRef = funcref;
				Arguments = args.ToArray ();
				var namedfunc = funcref as NamedFunctionRef;
				if (namedfunc != null && namedfunc.Target.Arguments.Length != Arguments.Length)
					throw new ArgumentException ("Invalid number of arguments.", nameof (args));
			}

			public override string ToString ()
			{
				return string.Format ("{0} ({1})", FuncRef,
					Arguments.Select (e => e.ToString ()).SeparateWith (", "));
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var func = (FunctionRef)transform (FuncRef);
				var args = Arguments.Select (a => (Expression)transform (a));
				return transform (func == FuncRef && args.SequenceEqual (Arguments) ? this :
					new FunctionCall (func, args));
			}
		}

		internal class HigherOrderFunctionCall : FunctionCall
		{
			public readonly FunctionRef[] FunctionArguments;

			internal HigherOrderFunctionCall (NamedFunctionRef funcref, IEnumerable<Expression> args,
				IEnumerable<FunctionRef> funcargs) : base (funcref, args)
			{
				FunctionArguments = funcargs.ToArray ();
				if (funcref.Target.FunctionArguments.Length != FunctionArguments.Length)
					throw new ArgumentException ("Invalid number of function arguments.", nameof (funcargs));
			}

			public NamedFunctionRef NamedFuncRef
			{
				get { return FuncRef as NamedFunctionRef; }
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var func = (NamedFunctionRef)transform (FuncRef);
				var args = Arguments.Select (a => (Expression)transform (a));
				var funcargs = FunctionArguments.Select (a => (FunctionRef)transform (a));
				return transform (func == FuncRef && args.SequenceEqual (Arguments) && 
					funcargs.SequenceEqual (FunctionArguments) ? this :
					new HigherOrderFunctionCall (func, args, funcargs));
			}
		}

		internal class NewArray : Expression
		{
			public readonly string ItemType;
			public readonly int ItemCount;
			public readonly Expression[] Items;

			protected NewArray (string type, int count, IEnumerable<Expression> items)
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
				var items = Items.Select (i => (Expression)transform (i));
				return transform (items.SequenceEqual (Items) ? this :
					new NewArray (ItemType, ItemCount, items));
			}
		}

		internal class Conditional : Expression
		{
			public readonly Expression Condition;
			public readonly Expression IfTrue;
			public readonly Expression IfFalse;

			protected Conditional (Expression condition, Expression ifTrue, Expression ifFalse)
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
				var cond = (Expression)transform (Condition);
				var iftrue = (Expression)transform (IfTrue);
				var iffalse = (Expression)transform (IfFalse);
				return transform (cond == Condition && iftrue == IfTrue && iffalse == IfFalse ? this :
					new Conditional (cond, iftrue, iffalse));
			}
		}

		internal abstract class Statement : Ast
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

			protected void EmitIntended (StringBuilder sb, Statement statement)
			{
				if (statement is Block)
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

		internal class Assignment : Statement
		{
			public readonly VariableRef VarRef;
			public readonly Expression Value;

			protected Assignment (VariableRef varRef, Expression value)
			{
				VarRef = varRef;
				Value = value;
			}

			public override string ToString ()
			{
				return string.Format ("{0}{1} = {2};\n", Tabs (), VarRef, Value);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var vari = (VariableRef)transform (VarRef);
				var val = (Expression)transform (Value);
				return transform (vari == VarRef && val == Value ? this :
					new Assignment (vari, val));
			}
		}

		internal class DeclareVar : Statement
		{
			public readonly Variable Var;
			public readonly Expression Value;

			protected DeclareVar (Variable variable, Expression value)
			{
				Var = variable;
				Value = value;
			}

			public override string ToString ()
			{
				return Value != null ?
					string.Format ("{0}{1} = {2};\n", Tabs (), Var, Value) :
					string.Format ("{0}{1};\n", Tabs (), Var);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var vari = (Variable)transform (Var);
				var val = (Expression)transform (Value);
				return transform (vari == Var && val == Value ? this :
					new DeclareVar (vari, val));
			}
		}

		internal class Block : Statement
		{
			public readonly List<Statement> Statements;

			protected Block (IEnumerable<Statement> statements)
			{
				Statements = new List<Statement> (statements);
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
				var stats = Statements.Select (i => (Statement)transform (i));
				return transform (stats.SequenceEqual (Statements) ? this :
					new Block (stats));
			}
		}

		internal class If : Statement
		{
			public readonly Expression Condition;
			public readonly Statement IfTrue;
			public readonly Statement IfFalse;

			protected If (Expression condition, Statement ifTrue, Statement ifFalse)
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
				var cond = (Expression)transform (Condition);
				var iftrue = (Statement)transform (IfTrue);
				var iffalse = (Statement)transform (IfFalse);
				return transform (cond == Condition && iftrue == IfTrue && iffalse == IfFalse ? this :
					new If (cond, iftrue, iffalse));
			}
		}

		internal class For : Statement
		{
			public readonly Variable LoopVar;
			public readonly Expression InitialValue;
			public readonly Expression Condition;
			public readonly Expression Increment;
			public readonly Statement Body;

			protected For (Variable loopVar, Expression initialValue, Expression condition,
				Expression increment, Statement body)
			{
				LoopVar = loopVar;
				InitialValue = initialValue;
				Condition = condition;
				Increment = increment;
				Body = body;
			}

			public override string ToString ()
			{
				var result = new StringBuilder ();
				EmitLine (result, "for ({0} = {1}; {2}; {3} = {4})\n", LoopVar, InitialValue,
					Condition, LoopVar.Name, Increment);
				EmitIntended (result, Body);
				return result.ToString ();
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var loopvar = (Variable)transform (LoopVar);
				var initval = (Expression)transform (InitialValue);
				var cond = (Expression)transform (Condition);
				var incr = (Expression)transform (Increment);
				var body = (Statement)transform (Body);
				return transform (loopvar == LoopVar && initval == InitialValue &&
					cond == Condition && incr == Increment && body == Body ? this :
					new For (loopvar, initval, cond, incr, body));
			}
		}

		internal class Return : Statement
		{
			public readonly Expression Value;

			protected Return (Expression value)
			{
				Value = value;
			}

			public override string ToString ()
			{
				return string.Format ("{0}return {1};\n", Tabs (), Value);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var val = (Expression)transform (Value);
				return transform (val == Value ? this : new Return (val));
			}
		}

		internal class MacroCall : Statement
		{
			public readonly MacroDefinition Target;
			public readonly Variable ReturnVar;
			public readonly MacroDefinition[] MacroParameters;
			public readonly Expression[] Parameters;

			public MacroCall (MacroDefinition target, Variable returnVar, 
				IEnumerable<MacroDefinition> macroParams, IEnumerable<Expression> parameters)
			{
				Target = target;
				ReturnVar = returnVar;
				MacroParameters = macroParams.ToArray ();
				Parameters = parameters.ToArray ();
				if (MacroParameters.Length != Target.MacroParameters.Length)
					throw new ArgumentException ("Wrong number of macro parameters", nameof (macroParams));
				if (Parameters.Length != Target.Parameters.Length)
					throw new ArgumentException ("Wrong number of expression parameters", nameof (parameters));
				if (!MacroParameters.Zip (Target.MacroParameters, MacroDefinition.AreCompatible)
					.All (Fun.Identity))
					throw new ArgumentException ("Macro parameters are not compatible with the definition", 
						nameof (macroParams));
			}

			public override string ToString ()
			{
				return string.Format ("{0} = {1} ({2}, {3})", ReturnVar, Target.Name,
					MacroParameters.Select (mp => mp.ToString ()).SeparateWith (", "),
					Parameters.Select (p => p.ToString ()).SeparateWith (", "));
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var target = (MacroDefinition)transform (Target);
				var retVar = (Variable)transform (ReturnVar);
				var mpars = MacroParameters.Select (p => (MacroDefinition)transform (p));
				var pars = Parameters.Select (p => (Expression)transform (p));
				return transform (target == Target && retVar == ReturnVar &&
					mpars.SequenceEqual (MacroParameters)
					&& pars.SequenceEqual (Parameters) ? this :
					new MacroCall (target, retVar, mpars, pars));
			}
		}

		internal abstract class Global : Ast
		{
		}

		internal class Function : Global
		{
			public readonly string Name;
			public readonly string ReturnType;
			public readonly Argument[] Arguments;
			public readonly FunctionArgument[] FunctionArguments;
			public readonly Block Body;

			internal Function (string name, string returnType, IEnumerable<Argument> arguments,
				IEnumerable<FunctionArgument> functionArguments, Block body)
			{
				Name = name;
				ReturnType = returnType;
				Arguments = arguments.ToArray ();
				FunctionArguments = functionArguments.ToArray ();
				Body = body;
				if (IsHigherOrder && IsExternal)
					throw new ArgumentException ("Higher order external functions are not supported.");
			}

			public bool IsExternal
			{
				get { return Body == null; }
			}

			public bool IsHigherOrder
			{
				get { return FunctionArguments.Length > 0; }
			}

			public override string ToString ()
			{
				return string.Format ("{0} {1} ({2})\n{3}", ReturnType, Name,
					Arguments.Select (v => v.ToString ()).SeparateWith (", "), Body);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var args = Arguments.Select (a => (Argument)transform (a));
				var funcargs = FunctionArguments.Select (a => (FunctionArgument)transform (a));
				var body = (Block)transform (Body);
				return transform (args.SequenceEqual (Arguments) && body == Body ? this :
					new Function (Name, ReturnType, args, funcargs, body));
			}
		}

		internal class Struct : Global
		{
			public readonly string Name;
			public readonly List<Field> Fields;

			protected Struct (string name, IEnumerable<Field> fields)
			{
				Name = name;
				Fields = new List<Field> (fields);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var fields = Fields.Select (f => (Field)transform (f));
				return transform (fields.SequenceEqual (Fields) ? this :
					new Struct (Name, fields));
			}
		}

		internal class ConstantDecl : Global
		{
			public readonly Constant Const;
			public readonly Expression Value;

			protected ConstantDecl (Constant constant, Expression value)
			{
				Const = constant;
				Value = value;
			}

			public override string ToString ()
			{
				return string.Format ("const {0} = {1};\n", Const, Value);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var con = (Constant)transform (Const);
				var val = (Expression)transform (Value);
				return transform (con == Const && val == Value ? this :
					new ConstantDecl (con, val));
			}
		}

		internal class Program : Ast
		{
			public readonly List<Global> Globals;

			protected Program (IEnumerable<Global> globals)
			{
				Globals = new List<Global> (globals);
			}

			public IEnumerable<Function> DefinedFunctions
			{
				get { return Globals.OfType<Function> (); }
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
				var globs = Globals.Select (g => (Global)transform (g));
				return transform (globs.SequenceEqual (Globals) ? this :
					new Program (globs));
			}
		}
	}
}
