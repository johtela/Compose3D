﻿namespace Compose3D.Compiler
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

		public abstract class Expression : Ast
		{
		}

		public class Literal : Expression
		{
			public readonly string Value;

			internal Literal (string value)
			{
				Value = value;
			}

			public override string ToString ()
			{
				return Value;
			}
		}

		public class Variable : Expression
		{
			public readonly string Type;
			public readonly string Name;
			public readonly int ArrayLength;

			internal Variable (string type, string name, int arraylen)
			{
				Type = type;
				Name = name;
				ArrayLength = arraylen;
			}

			public override string ToString ()
			{
				var arrayDecl = ArrayLength > 0 ?
					string.Format ("[{0}]", ArrayLength) : "";
				return string.Format ("{0} {1}{2}", Type, Name, arrayDecl);
			}
		}
		 
		public class Field : Variable
		{
			internal Field (string type, string name, int arraylen) 
				: base (type, name, arraylen) { }
		}

		public class Argument : Variable
		{
			internal Argument (string type, string name, int arraylen) 
				: base (type, name, arraylen) { }
		}

		public class Constant : Variable
		{
			public readonly Expression Value;

			internal Constant (string type, string name, int arraylen, Expression value)
				: base (type, name, arraylen)
			{
				var na = value as NewArray;
				if (arraylen > 0 && na == null)
					throw new ArgumentException (
						"Constant array must be initialized with the NewArray expression.");
				if (na != null && na.Items.Length != arraylen)
					throw new ArgumentException (
						"Wrong number of items in array initializer", nameof (value));
				Value = value;
			}

			public override string ToString ()
			{
				var decl = base.ToString ();
				return string.Format ("const {0} = {1}", decl, Value);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var val = (Expression)Value.Transform (transform);
				return transform (val == Value ? this :
					new Constant (Type, Name, ArrayLength, val));
			}
		}

		public class VariableRef : Expression
		{
			public readonly Variable Target;

			internal VariableRef (Variable target)
			{
				Target = target;
			}

			public override string ToString ()
			{
				return Target.Name;
			}
		}

		public class MacroParam : Ast
		{
			public readonly string Type;

			internal MacroParam (string type)
			{
				Type = type;
			}

			public override string ToString ()
			{
				return Type;
			}

			public bool IsMacroDefinition
			{
				get { return this is MacroDefParam; }
			}
		}

		public class MacroDefParam : MacroParam
		{
			public readonly MacroDefinition Definition;

			public MacroDefParam (MacroDefinition definition)
				: base (definition.ToString ())
			{
				Definition = definition;
			}

			public override string ToString ()
			{
				return Definition.ToString ();
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var def = (MacroDefinition)Definition.Transform (transform);
				return transform (def == Definition ? this : new MacroDefParam (def));
			}
		}

		public class MacroParamRef : Expression
		{
			public readonly MacroParam Target;

			internal MacroParamRef (MacroParam target)
			{
				if (target is MacroDefParam)
					throw new ArgumentException ("Cannot use macro definition as expression.");
				Target = target;
			}
		}

		public class MacroResultVar : Variable
		{
			public MacroResultVar (string type)
				: base (type, "macro", 0)
			{ }
		}

		public class MacroDefinition : Ast
		{
			public readonly MacroParam[] Parameters;
			public readonly MacroResultVar Result;

			internal MacroDefinition (IEnumerable<MacroParam> parameters, 
				MacroResultVar result)
			{
				Parameters = parameters.ToArray ();
				Result = result;
			}

			public override string ToString ()
			{
				return string.Format ("{0} ({1})", Result, 
					Parameters.Select (v => v.ToString ()).SeparateWith (", "));
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var pars = Parameters.Select (p => (MacroParam)p.Transform (transform));
				var res = (MacroResultVar)Result.Transform (transform);
				return transform (pars.SequenceEqual (Parameters) && res == Result ? this :
					new MacroDefinition (pars, res));
			}

			public int GetMacroDefParamIndex (MacroDefinition def)
			{
				return Parameters.FirstIndex (p =>
					p.IsMacroDefinition && (p as Ast.MacroDefParam).Definition == def);
			}

			public int GetParamRefIndex (MacroParamRef pref)
			{
				return Parameters.IndexOf (pref.Target);
			}

			public static bool AreCompatible (MacroDefinition def, MacroDefinition other)
			{
				return def.ToString () == other.ToString ();
			}
		}

		public class Macro : MacroDefinition
		{
			public readonly Block Implementation;

			internal Macro (IEnumerable<MacroParam> parameters, MacroResultVar result, 
				Block implementation) : base (parameters, result)
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
				var impl = (Block)Implementation.Transform (transform);
				return transform (def == this && impl == Implementation ? this :
					new Macro (def.Parameters, def.Result, Implementation));
			}
		}

		public class FieldRef : Expression
		{
			public readonly Expression TargetExpr;
			public readonly Field TargetField;

			internal FieldRef (Expression expression, Field field)
			{
				TargetExpr = expression;
				TargetField = field;
			}

			public override string ToString ()
			{
				return string.Format ("{0}.{1}", TargetExpr, TargetField.Name);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var expr = (Expression)TargetExpr.Transform (transform);
				return transform (expr == TargetExpr ? this :
					new FieldRef (expr, TargetField));
			}
		}

		public class UnaryOperation : Expression
		{
			public readonly string Operator;
			public readonly Expression Operand;

			internal UnaryOperation (string oper, Expression operand)
			{
				Operator = oper;
				Operand = operand;
			}

			public override string ToString ()
			{
				return string.Format (Operator, Operand);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var argument = (Expression)Operand.Transform (transform);
				return transform (argument == Operand ?  this : 
					new UnaryOperation (Operator, argument));
			}
		}

		public class BinaryOperation : Expression
		{
			public readonly string Operator;
			public readonly Expression LeftOperand;
			public readonly Expression RightOperand;

			internal BinaryOperation (string oper, Expression left, Expression right)
			{
				Operator = oper;
				LeftOperand = left;
				RightOperand = right;
			}

			public override string ToString ()
			{
				return string.Format ("(" + Operator + ")", LeftOperand, RightOperand);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var left = (Expression)LeftOperand.Transform (transform);
				var right = (Expression)RightOperand.Transform (transform);
				return transform (left == LeftOperand && right == RightOperand ? this :
					new BinaryOperation (Operator, left, right));
			}
		}

		public class FunctionCall : Expression
		{
			public readonly Function FuncRef;
			public readonly Expression[] Arguments;

			internal FunctionCall (Function funcref, IEnumerable<Expression> args)
			{
				FuncRef = funcref;
				Arguments = args.ToArray ();
				if (FuncRef.Arguments.Length != Arguments.Length)
					throw new ArgumentException ("Invalid number of arguments.", nameof (args));
			}

			public override string ToString ()
			{
				return string.Format ("{0} ({1})", FuncRef.Name,
					Arguments.Select (e => e.ToString ()).SeparateWith (", "));
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var func = (Function)FuncRef.Transform (transform);
				var args = Arguments.Select (a => (Expression)a.Transform (transform));
				return transform (func == FuncRef && args.SequenceEqual (Arguments) ? this :
					new FunctionCall (func, args));
			}
		}

		public class ExternalFunctionCall : Expression
		{
			public readonly string FormatStr;
			public readonly Expression[] Arguments;

			internal ExternalFunctionCall (string formatStr, IEnumerable<Expression> args)
			{
				FormatStr = formatStr;
				Arguments = args.ToArray ();
			}

			public override string ToString ()
			{
				return string.Format (FormatStr, 
					Arguments.Select (e => e.ToString ()).SeparateWith (", "));
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var args = Arguments.Select (a => (Expression)a.Transform (transform));
				return transform (args.SequenceEqual (Arguments) ? this :
					new ExternalFunctionCall (FormatStr, args));
			}
		}

		public class NewArray : Expression
		{
			public readonly string ItemType;
			public readonly int ItemCount;
			public readonly Expression[] Items;

			internal NewArray (string type, int count, IEnumerable<Expression> items)
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
					Items.Select (e => e.ToString ()).SeparateWith (",\n\t"));
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var items = Items.Select (i => (Expression)i.Transform (transform));
				return transform (items.SequenceEqual (Items) ? this :
					new NewArray (ItemType, ItemCount, items));
			}
		}

		public class Conditional : Expression
		{
			public readonly Expression Condition;
			public readonly Expression IfTrue;
			public readonly Expression IfFalse;

			internal Conditional (Expression condition, Expression ifTrue, Expression ifFalse)
			{
				Condition = condition;
				IfTrue = ifTrue;
				IfFalse = ifFalse;
			}

			public override string ToString ()
			{
				return string.Format ("({0} ? {1} : {2})", Condition, IfTrue, IfFalse);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var cond = (Expression)Condition.Transform (transform);
				var iftrue = (Expression)IfTrue.Transform (transform);
				var iffalse = (Expression)IfFalse.Transform (transform);
				return transform (cond == Condition && iftrue == IfTrue && iffalse == IfFalse ? this :
					new Conditional (cond, iftrue, iffalse));
			}
		}

		public abstract class Statement : Ast
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

		public class Assignment : Statement
		{
			public readonly VariableRef VarRef;
			public readonly Expression Value;

			internal Assignment (VariableRef varRef, Expression value)
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
				var vari = (VariableRef)VarRef.Transform (transform);
				var val = (Expression)Value.Transform (transform);
				return transform (vari == VarRef && val == Value ? this :
					new Assignment (vari, val));
			}
		}

		public class DeclareVar : Statement
		{
			public readonly Variable Definition;
			public readonly Expression Value;

			internal DeclareVar (Variable definition, Expression value)
			{
				Definition = definition;
				Value = value;
			}

			public override string ToString ()
			{
				return Value != null ?
					string.Format ("{0}{1} = {2};\n", Tabs (), Definition, Value) :
					string.Format ("{0}{1};\n", Tabs (), Definition);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var vari = (Variable)Definition.Transform (transform);
				var val = Value != null ? (Expression)Value.Transform (transform) : null;
				return transform (vari == Definition && val == Value ? this :
					new DeclareVar (vari, val));
			}
		}

		public class Block : Statement
		{
			public readonly List<Statement> Statements;

			internal Block (IEnumerable<Statement> statements)
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
				var stats = Statements.Select (s => (Statement)s.Transform (transform));
				return transform (stats.SequenceEqual (Statements) ? this :
					new Block (stats));
			}
		}

		public class IfElse : Statement
		{
			public readonly Expression Condition;
			public readonly Statement IfTrue;
			public readonly Statement IfFalse;

			internal IfElse (Expression condition, Statement ifTrue, Statement ifFalse)
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
				var cond = (Expression)Condition.Transform (transform);
				var iftrue = (Statement)IfTrue.Transform (transform);
				var iffalse = (Statement)IfFalse.Transform (transform);
				return transform (cond == Condition && iftrue == IfTrue && iffalse == IfFalse ? this :
					new IfElse (cond, iftrue, iffalse));
			}
		}

		public class ForLoop : Statement
		{
			public readonly Variable LoopVar;
			public readonly Expression InitialValue;
			public readonly Expression Condition;
			public readonly Expression Increment;
			public readonly Statement Body;

			internal ForLoop (Variable loopVar, Expression initialValue, Expression condition,
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
				EmitLine (result, "for ({0} = {1}; {2}; {3})\n", LoopVar, InitialValue,
					Condition, Increment);
				EmitIntended (result, Body);
				return result.ToString ();
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var loopvar = (Variable)LoopVar.Transform (transform);
				var initval = (Expression)InitialValue.Transform (transform);
				var cond = (Expression)Condition.Transform (transform);
				var incr = (Expression)Increment.Transform (transform);
				var body = (Statement)Body.Transform (transform);
				return transform (loopvar == LoopVar && initval == InitialValue &&
					cond == Condition && incr == Increment && body == Body ? this :
					new ForLoop (loopvar, initval, cond, incr, body));
			}
		}

		public class CallStatement : Statement
		{
			public readonly ExternalFunctionCall ExternalCall;

			public CallStatement (ExternalFunctionCall call)
			{
				ExternalCall = call;
			}

			public override string ToString ()
			{
				return string.Format ("{0}{1};\n", Tabs(), ExternalCall);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var call = (ExternalFunctionCall)ExternalCall.Transform (transform);
				return transform (call == ExternalCall ? this :
					new CallStatement (call));
			}
		}

		public class Return : Statement
		{
			public readonly Expression Value;

			internal Return (Expression value)
			{
				Value = value;
			}

			public override string ToString ()
			{
				return string.Format ("{0}return {1};\n", Tabs (), Value);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var val = (Expression)Value.Transform (transform);
				return transform (val == Value ? this : new Return (val));
			}
		}

		public class MacroCall : Statement
		{
			public readonly MacroDefinition Target;
			public readonly Variable ResultVar;
			public readonly Ast[] Parameters;

			internal MacroCall (MacroDefinition target, Variable resultVar, 
				IEnumerable<Ast> parameters)
			{
				Target = target;
				ResultVar = resultVar;
				if (resultVar.Type != target.Result.Type)
					throw new ArgumentException ("Incompatible result variable type", nameof (resultVar));
				if (Parameters.Length != Target.Parameters.Length)
					throw new ArgumentException ("Wrong number of parameters", nameof (parameters));
				Parameters = parameters.ToArray ();
				for (int i = 0; i < Parameters.Length; i++)
				{
					var par = Target.Parameters[i];
					if (par is MacroDefParam && !(Parameters[i] is MacroDefinition))
						throw new ArgumentException ("Macro parameter expected in position " + i);
					if (!(par is MacroDefParam) && !(Parameters[i] is Expression))
						throw new ArgumentException ("Expression expected in position " + i);
				}
			}

			public override string ToString ()
			{
				return string.Format ("{0} = {1} ({2})", ResultVar, Target.Result.Name,
					Parameters.Select (p => p.ToString ()).SeparateWith (", "));
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var target = (MacroDefinition)Target.Transform (transform);
				var retVar = (Variable)ResultVar.Transform (transform);
				var pars = Parameters.Select (p => p.Transform (transform));
				return transform (target == Target && retVar == ResultVar &&
					pars.SequenceEqual (Parameters) ? this : new MacroCall (target, retVar, pars));
			}
		}

		public abstract class Global : Ast
		{
			public readonly string Name;

			internal Global (string name)
			{
				Name = name;
			}
		}

		public class Structure : Global
		{
			public readonly List<Field> Fields;

			internal Structure (string name, IEnumerable<Field> fields)
				: base (name)
			{
				Fields = new List<Field> (fields);
			}

			public Field FindField (string name)
			{
				return Fields.Find (f => f.Name == name);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var fields = Fields.Select (f => (Field)f.Transform (transform));
				return transform (fields.SequenceEqual (Fields) ? this :
					new Structure (Name, fields));
			}
		}

		public class Declaration : Global
		{
			public readonly string Text;

			public Declaration (string text)
				: base (text)
			{
				Text = text;
			}

			public override string ToString ()
			{
				return Text;
			}
		}

		public class Function : Ast
		{
			public readonly string Name;
			public readonly string ReturnType;
			public readonly Argument[] Arguments;
			public readonly Block Body;

			internal Function (string name, string returnType, IEnumerable<Argument> arguments,
				Block body)
			{
				Name = name;
				ReturnType = returnType;
				Arguments = arguments.ToArray ();
				Body = body;
			}

			public override string ToString ()
			{
				return string.Format ("{0} {1} ({2})\n{3}", ReturnType, Name,
					Arguments.Select (v => v.ToString ()).SeparateWith (", "), Body);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var args = Arguments.Select (a => (Argument)a.Transform (transform));
				var body = (Block)Body.Transform (transform);
				return transform (args.SequenceEqual (Arguments) && body == Body ? this :
					new Function (Name, ReturnType, args, body));
			}
		}

		public class Program : Ast
		{
			public readonly List<Global> Globals;

			public readonly List<Function> Functions;

			internal Program (IEnumerable<Global> globals, IEnumerable<Function> functions)
			{
				Globals = new List<Global> (globals);
				Functions = new List<Function> (functions);
			}

			public IEnumerable<Structure> DefinedStructs
			{
				get { return Globals.OfType<Structure> (); }
			}

			public Structure FindStruct (string name)
			{
				return DefinedStructs.First (s => s.Name == name);
			}

			public override string ToString ()
			{
				var result = new StringBuilder ();
				foreach (var glob in Globals)
				{
					result.Append (glob);
					result.AppendLine ();
				}
				result.AppendLine ();
				foreach (var func in Functions)
				{
					result.Append (func);
					result.AppendLine ();
				}
				return result.ToString ();
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var globs = Globals.Select (g => (Global)g.Transform (transform));
				var funcs = Functions.Select (f => (Function)f.Transform (transform));
				return transform (globs.SequenceEqual (Globals) && funcs.SequenceEqual (Functions) ? 
					this : new Program (globs, funcs));
			}
		}

		public static Literal Lit (string value)
		{
			return new Literal (value);
		}

		public static Variable Var (string type, string name)
		{
			return new Variable (type, name, 0);
		}

		public static Variable Var (string type, string name, int arraylen)
		{
			return new Variable (type, name, arraylen);
		}

		public static Field Fld (string type, string name)
		{
			return new Field (type, name, 0);
		}

		public static Field Fld (string type, string name, int arraylen)
		{
			return new Field (type, name, arraylen);
		}

		public static Field Fld (string name)
		{
			return new Field (null, name, 0);
		}

		public static Argument Arg (string type, string name)
		{
			return new Argument (type, name, 0);
		}

		public static Argument Arg (string type, string name, int arraylen)
		{
			return new Argument (type, name, arraylen);
		}

		public static Constant Const (string type, string name, Expression value)
		{
			return new Constant (type, name, 0, value);
		}

		public static Constant Const (string type, string name, int arraylen, Expression value)
		{
			return new Constant (type, name, arraylen, value);
		}

		public static VariableRef VRef (Variable variable)
		{
			return new VariableRef (variable);
		}

		public static MacroParam MPar (string type)
		{
			return new MacroParam (type);
		}

		public static MacroParam MDPar (MacroDefinition definition)
		{
			return new MacroDefParam (definition);
		}

		public static MacroResultVar MRes (string type)
		{
			return new MacroResultVar (type);
		}

		public static MacroParamRef MPRef (MacroParam mpar)
		{
			return new MacroParamRef (mpar);
		}

		public static MacroDefinition MDef (IEnumerable<MacroParam> mpars, 
			MacroResultVar result)
		{
			return new MacroDefinition (mpars, result);
		}

		public static Macro Mac (IEnumerable<MacroParam> mpars, MacroResultVar result, 
			Block impl)
		{
			return new Macro (mpars, result, impl);
		}

		public static FieldRef FRef (Expression expr, Field field)
		{
			return new FieldRef (expr, field);
		}

		public static UnaryOperation Op (string oper, Expression operand)
		{
			return new UnaryOperation (oper, operand);
		}

		public static BinaryOperation Op (string oper, Expression left, Expression right)
		{
			return new BinaryOperation (oper, left, right);
		}

		public static FunctionCall Call (Function function, IEnumerable<Expression> args)
		{
			return new FunctionCall (function, args);
		}

		public static FunctionCall Call (Function function, params Expression[] args)
		{
			return new FunctionCall (function, args);
		}

		public static ExternalFunctionCall Call (string formatStr, IEnumerable<Expression> args)
		{
			return new ExternalFunctionCall (formatStr, args);
		}

		public static ExternalFunctionCall Call (string formatStr, params Expression[] args)
		{
			return new ExternalFunctionCall (formatStr, args);
		}

		public static NewArray Arr (string type, int count, IEnumerable<Expression> items)
		{
			return new NewArray (type, count, items);
		}

		public static Conditional Cond (Expression condition, Expression ifTrue, Expression ifFalse)
		{
			return new Conditional (condition, ifTrue, ifFalse);
		}

		public static Assignment Ass (VariableRef varRef, Expression value)
		{
			return new Assignment (varRef, value);
		}

		public static DeclareVar DeclVar (Variable definition, Expression value)
		{
			return new DeclareVar (definition, value);
		}

		public static Block Blk (IEnumerable<Statement> statements)
		{
			return new Block (statements);
		}

		public static Block Blk ()
		{
			return new Block (Enumerable.Empty<Statement> ());
		}

		public static IfElse If (Expression condition, Statement ifTrue, Statement ifFalse)
		{
			return new IfElse (condition, ifTrue, ifFalse);
		}

		public static IfElse If (Expression condition, Statement ifTrue)
		{
			return new IfElse (condition, ifTrue, null);
		}

		public static ForLoop For (Variable loopVar, Expression initialValue, Expression condition,
			Expression increment, Statement body)
		{
			return new ForLoop (loopVar, initialValue, condition, increment, body);
		}

		public static CallStatement CallS (ExternalFunctionCall call)
		{
			return new CallStatement (call);
		}

		public static Return Ret (Expression value)
		{
			return new Return (value);
		}

		public static Return Ret ()
		{
			return new Return (Lit (""));
		}

		public static MacroCall MCall (MacroDefinition target, Variable returnVar,
			IEnumerable<Ast> parameters)
		{
			return new MacroCall (target, returnVar, parameters);
		}

		public static Function Fun (string name, string returnType, IEnumerable<Argument> arguments,
			Block body)
		{
			return new Function (name, returnType, arguments, body);
		}

		public static Declaration Decl (string text)
		{
			return new Declaration (text);
		}

		public static Program Prog (IEnumerable<Global> globals, IEnumerable<Function> functions)
		{
			return new Program (globals, functions);
		}

		public static Program Prog ()
		{
			return new Program (Enumerable.Empty<Global> (), Enumerable.Empty<Function> ());
		}
	}
}
