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

			internal Variable (string type, string name)
			{
				Type = type;
				Name = name;
			}

			public override string ToString ()
			{
				return string.Format ("{0} {1}", Type, Name);
			}
		}

		public class Field : Variable
		{
			internal Field (string type, string name) : base (type, name) { }
		}

		public class Argument : Variable
		{
			internal Argument (string type, string name) : base (type, name) { }
		}

		public class Constant : Variable
		{
			internal Constant (string type, string name) : base (type, name) { }
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
			public readonly string Name;

			internal MacroParam (string name)
			{
				Name = name;
			}

			public override string ToString ()
			{
				return Name;
			}
		}

		public class MacroParamRef : Expression
		{
			public readonly MacroParam Target;

			internal MacroParamRef (MacroParam target)
			{
				Target = target;
			}
		}

		public class MacroDefinition : Ast
		{
			public readonly MacroParam Name;
			public readonly MacroDefinition[] MacroParameters;
			public readonly MacroParam[] Parameters;

			internal MacroDefinition (MacroParam name, IEnumerable<MacroDefinition> macroParams, 
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
					def.MacroParameters.Zip (other.MacroParameters, AreCompatible).All (Extensions.Fun.Identity);
			}
		}

		public class Macro : MacroDefinition
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
				var expr = (Expression)transform (TargetExpr);
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
				var argument = (Expression)transform (Operand);
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
				var left = (Expression)transform (LeftOperand);
				var right = (Expression)transform (RightOperand);
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
				return string.Format ("{0} ({1})", FuncRef,
					Arguments.Select (e => e.ToString ()).SeparateWith (", "));
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var func = (Function)transform (FuncRef);
				var args = Arguments.Select (a => (Expression)transform (a));
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
				var args = Arguments.Select (a => (Expression)transform (a));
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
					Items.Select (e => e.ToString ()).SeparateWith (", "));
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var items = Items.Select (i => (Expression)transform (i));
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
				var cond = (Expression)transform (Condition);
				var iftrue = (Expression)transform (IfTrue);
				var iffalse = (Expression)transform (IfFalse);
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
				var vari = (VariableRef)transform (VarRef);
				var val = (Expression)transform (Value);
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
				var vari = (Variable)transform (Definition);
				var val = (Expression)transform (Value);
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
				var stats = Statements.Select (i => (Statement)transform (i));
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
				var cond = (Expression)transform (Condition);
				var iftrue = (Statement)transform (IfTrue);
				var iffalse = (Statement)transform (IfFalse);
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
					new ForLoop (loopvar, initval, cond, incr, body));
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
				var val = (Expression)transform (Value);
				return transform (val == Value ? this : new Return (val));
			}
		}

		public class MacroCall : Statement
		{
			public readonly MacroDefinition Target;
			public readonly Variable ReturnVar;
			public readonly MacroDefinition[] MacroParameters;
			public readonly Expression[] Parameters;

			internal MacroCall (MacroDefinition target, Variable returnVar, 
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
					.All (Extensions.Fun.Identity))
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

		public abstract class Global : Ast
		{
		}

		public class Function : Global
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
				var args = Arguments.Select (a => (Argument)transform (a));
				var body = (Block)transform (Body);
				return transform (args.SequenceEqual (Arguments) && body == Body ? this :
					new Function (Name, ReturnType, args, body));
			}
		}

		public class Structure : Global
		{
			public readonly string Name;
			public readonly List<Field> Fields;

			internal Structure (string name, IEnumerable<Field> fields)
			{
				Name = name;
				Fields = new List<Field> (fields);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var fields = Fields.Select (f => (Field)transform (f));
				return transform (fields.SequenceEqual (Fields) ? this :
					new Structure (Name, fields));
			}
		}

		public class ConstantDecl : Global
		{
			public readonly Constant Definition;
			public readonly Expression Value;

			internal ConstantDecl (Constant definition, Expression value)
			{
				Definition = definition;
				Value = value;
			}

			public override string ToString ()
			{
				return string.Format ("const {0} = {1};\n", Definition, Value);
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var con = (Constant)transform (Definition);
				var val = (Expression)transform (Value);
				return transform (con == Definition && val == Value ? this :
					new ConstantDecl (con, val));
			}
		}

		public class Program : Ast
		{
			public readonly List<Global> Globals;

			internal Program (IEnumerable<Global> globals)
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

		public static Literal Lit (string value)
		{
			return new Literal (value);
		}

		public static Variable Var (string type, string name)
		{
			return new Variable (type, name);
		}

		public static Field Fld (string type, string name)
		{
			return new Field (type, name);
		}

		public static Field Fld (string name)
		{
			return new Field (null, name);
		}

		public static Argument Arg (string type, string name)
		{
			return new Argument (type, name);
		}

		public static Constant Const (string type, string name)
		{
			return new Constant (type, name);
		}

		public static VariableRef VRef (Variable variable)
		{
			return new VariableRef (variable);
		}

		public static MacroParam MPar (string name)
		{
			return new MacroParam (name);
		}

		public static MacroParamRef MPRef (MacroParam mpar)
		{
			return new MacroParamRef (mpar);
		}

		public static MacroDefinition MDef (MacroParam name, 
			IEnumerable<MacroDefinition> mdefpars, IEnumerable<MacroParam> mpars)
		{
			return new MacroDefinition (name, mdefpars, mpars);
		}

		public static Macro Mac (MacroParam name,
			IEnumerable<MacroDefinition> mdefpars, IEnumerable<MacroParam> mpars,
			Block impl)
		{
			return new Macro (name, mdefpars, mpars, impl);
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

		public static ExternalFunctionCall Call (string formatStr, IEnumerable<Expression> args)
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

		public static DeclareVar Decl (Variable definition, Expression value)
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

		public static Return Ret (Expression value)
		{
			return new Return (value);
		}

		public static Return Ret ()
		{
			return new Return (Lit (""));
		}

		public static MacroCall MCall (MacroDefinition target, Variable returnVar,
			IEnumerable<MacroDefinition> macroParams, IEnumerable<Expression> parameters)
		{
			return new MacroCall (target, returnVar, macroParams, parameters);
		}

		public static Function Fun (string name, string returnType, IEnumerable<Argument> arguments,
			Block body)
		{
			return new Function (name, returnType, arguments, body);
		}

		public static Structure Struct (string name, IEnumerable<Field> fields)
		{
			return new Structure (name, fields);
		}

		public static ConstantDecl DeclCon (Constant definition, Expression value)
		{
			return new ConstantDecl (definition, value);
		}

		public static Program Prog (IEnumerable<Global> globals)
		{
			return new Program (globals);
		}

		public static Program Prog ()
		{
			return new Program (Enumerable.Empty<Global> ());
		}
	}
}
