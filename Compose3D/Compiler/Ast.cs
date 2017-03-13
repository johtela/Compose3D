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

		public abstract string Output (LinqParser parser);

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

			public override string Output (LinqParser parser)
			{
				return Value;
			}
		}

		public class Variable : Ast
		{
			public readonly Type Type;
			public readonly string Name;
			public readonly int ArrayLength;

			internal Variable (Type type, string name, int arraylen)
			{
				Type = type;
				Name = name;
				ArrayLength = arraylen;
			}

			public override string Output (LinqParser parser)
			{
				var arrayDecl = ArrayLength > 0 ?
					string.Format ("[{0}]", ArrayLength) : "";
				return string.Format ("{0} {1}{2}", parser.MapType (Type), Name, arrayDecl);
			}
		}
		 
		public class Field : Variable
		{
			internal Field (Type type, string name, int arraylen) 
				: base (type, name, arraylen) { }
		}

		public class Argument : Variable
		{
			internal Argument (Type type, string name, int arraylen) 
				: base (type, name, arraylen) { }
		}

		public class Constant : Variable
		{
			public readonly Expression Value;

			internal Constant (Type type, string name, int arraylen, Expression value)
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

			public override string Output (LinqParser parser)
			{
				var decl = base.Output (parser);
				return string.Format ("{0} = {1}", decl, Value.Output (parser));
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

			public override string Output (LinqParser parser)
			{
				return Target.Name;
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var target = (Variable)Target.Transform (transform);
				return transform (target == Target ? this : new VariableRef (target));
			}
		}

		public class ArrayRef : VariableRef
		{
			public readonly Expression Index;

			internal ArrayRef (Variable target, Expression index)
				: base (target)
			{
				Index = index;
			}

			public override string Output (LinqParser parser)
			{
				return string.Format ("{0}[{1}]", Target.Name, Index.Output (parser));
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var vref = (VariableRef)base.Transform (transform);
				var ind = (Expression)Index.Transform (transform);
				return transform (vref == this && ind == Index ? 
					this : new ArrayRef (vref.Target, ind));
			}
		}

		public class MacroParam : Ast
		{
			public readonly Type Type;

			internal MacroParam (Type type)
			{
				Type = type;
			}

			public override string Output (LinqParser parser)
			{
				return parser.MapType (Type);
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
				: base (null)
			{
				Definition = definition;
			}

			public override string Output (LinqParser parser)
			{
				return Definition.Output (parser);
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

			public override string Output (LinqParser parser)
			{
				return Target.Output (parser);
			}
		}

		public class MacroResultVar : Variable
		{
			public MacroResultVar (Type type)
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

			public override string Output (LinqParser parser)
			{
				return string.Format ("{0} ({1})", Result, 
					Parameters.Select (v => v.Output (parser)).SeparateWith (", "));
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

			public override string Output (LinqParser parser)
			{
				return base.Output (parser) + "\n" + Implementation.Output (parser);
			}

			//public override Ast Transform (Func<Ast, Ast> transform)
			//{
			//	var def = (MacroDefinition)base.Transform (transform);
			//	var impl = (Block)Implementation.Transform (transform);
			//	return def == this && impl == Implementation ? this :
			//		new Macro (def.Parameters, def.Result, impl);
			//}
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

			public override string Output (LinqParser parser)
			{
				return string.Format ("{0}.{1}", TargetExpr.Output (parser), TargetField.Name);
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

			public override string Output (LinqParser parser)
			{
				return string.Format (Operator, Operand.Output (parser));
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

			public override string Output (LinqParser parser)
			{
				return string.Format ("(" + Operator + ")", 
					LeftOperand.Output (parser), RightOperand.Output (parser));
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

			public override string Output (LinqParser parser)
			{
				return string.Format ("{0} ({1})", FuncRef.Name,
					Arguments.Select (e => e.Output (parser)).SeparateWith (", "));
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

			public override string Output (LinqParser parser)
			{
				return string.Format (FormatStr, 
					Arguments.Select (e => e.Output (parser)).SeparateWith (", "));
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var args = Arguments.Select (a => (Expression)a.Transform (transform));
				return transform (args.SequenceEqual (Arguments) ? this :
					new ExternalFunctionCall (FormatStr, args));
			}
		}

		public abstract class NewArray : Expression
		{
			public readonly Type ItemType;
			public readonly int ItemCount;
			public readonly Expression[] Items;

			internal NewArray (Type type, int count, IEnumerable<Expression> items)
			{
				if (items.Count () != count)
					throw new ArgumentException ("Invalid number arguments", nameof (items));
				ItemType = type;
				ItemCount = count;
				Items = items.ToArray ();
			}
		}

		public abstract class InitStruct : Expression
		{
			public readonly Type StructType;
			public readonly Tuple<VariableRef, Expression>[] InitFields;

			internal InitStruct (Type structType, IEnumerable<Tuple<VariableRef, Expression>> initFields)
			{
				StructType = structType;
				InitFields = initFields.ToArray ();
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

			public override string Output (LinqParser parser)
			{
				return string.Format ("({0} ? {1} : {2})", Condition.Output (parser), 
					IfTrue.Output (parser), IfFalse.Output (parser));
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

			protected void EmitIntended (StringBuilder sb, Statement statement, LinqParser parser)
			{
				if (statement is Block)
					sb.Append (statement.Output (parser));
				else
				{
					_tabLevel++;
					sb.Append (statement.Output (parser));
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

			public override string Output (LinqParser parser)
			{
				return string.Format ("{0}{1} = {2};\n", Tabs (), VarRef.Output (parser), 
					Value.Output (parser));
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

			public override string Output (LinqParser parser)
			{
				var def = Definition.Output (parser);
				return Value != null ?
					string.Format ("{0}{1} = {2};\n", Tabs (), def, Value.Output (parser)) :
					string.Format ("{0}{1};\n", Tabs (), def);
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

			public override string Output (LinqParser parser)
			{
				var result = new StringBuilder ();
				BeginScope (result);
				foreach (var statement in Statements)
					result.Append (statement.Output (parser));
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

			public override string Output (LinqParser parser)
			{
				var result = new StringBuilder ();
				EmitLine (result, "if ({0})\n", Condition.Output (parser));
				EmitIntended (result, IfTrue, parser);
				if (IfFalse != null)
				{
					EmitLine (result, "else");
					EmitIntended (result, IfFalse, parser);
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

			public override string Output (LinqParser parser)
			{
				var result = new StringBuilder ();
				EmitLine (result, "for ({0} = {1}; {2}; {3})", LoopVar.Output (parser), 
					InitialValue.Output (parser), Condition.Output (parser), Increment.Output (parser));
				EmitIntended (result, Body, parser);
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

		public class WhileLoop : Statement
		{
			public readonly Expression Condition;
			public readonly Statement Body;

			internal WhileLoop (Expression condition, Statement body)
			{
				Condition = condition;
				Body = body;
			}

			public override string Output (LinqParser parser)
			{
				var result = new StringBuilder ();
				EmitLine (result, "while ({0})", Condition.Output (parser));
				EmitIntended (result, Body, parser);
				return result.ToString ();
			}

			public override Ast Transform (Func<Ast, Ast> transform)
			{
				var cond = (Expression)Condition.Transform (transform);
				var body = (Statement)Body.Transform (transform);
				return transform (cond == Condition && body == Body ? 
					this : new WhileLoop (cond, body));
			}
		}

		public class CallStatement : Statement
		{
			public readonly ExternalFunctionCall ExternalCall;

			public CallStatement (ExternalFunctionCall call)
			{
				ExternalCall = call;
			}

			public override string Output (LinqParser parser)
			{
				return string.Format ("{0}{1};\n", Tabs(), ExternalCall.Output (parser));
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

			public override string Output (LinqParser parser)
			{
				return string.Format ("{0}return {1};\n", Tabs (), Value.Output (parser));
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
				Parameters = parameters.ToArray ();
				if (Parameters.Length != Target.Parameters.Length)
					throw new ArgumentException ("Wrong number of parameters", nameof (parameters));
				for (int i = 0; i < Parameters.Length; i++)
				{
					var par = Target.Parameters[i];
					if (par is MacroDefParam && !(Parameters[i] is MacroDefinition))
						throw new ArgumentException ("Macro parameter expected in position " + i);
					if (!(par is MacroDefParam) && !(Parameters[i] is Expression))
						throw new ArgumentException ("Expression expected in position " + i);
				}
			}

			public override string Output (LinqParser parser)
			{
				return string.Format ("{0} = {1} ({2})", 
					ResultVar.Output (parser), Target.Result.Name,
					Parameters.Select (p => p.Output (parser)).SeparateWith (", "));
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

		public abstract class Structure : Global
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
		}

		public class Declaration : Global
		{
			public readonly string Text;

			public Declaration (string text)
				: base (text)
			{
				Text = text;
			}

			public override string Output (LinqParser parser)
			{
				return Text;
			}
		}

		public class Function : Ast
		{
			public readonly string Name;
			public readonly Type ReturnType;
			public readonly Argument[] Arguments;
			public readonly Block Body;

			internal Function (string name, Type returnType, IEnumerable<Argument> arguments,
				Block body)
			{
				Name = name;
				ReturnType = returnType;
				Arguments = arguments.ToArray ();
				Body = body;
			}

			public override string Output (LinqParser parser)
			{
				var retType = ReturnType == null ? "void" : parser.MapType (ReturnType);
				return string.Format ("{0} {1} ({2})\n{3}", retType, Name,
					Arguments.Select (v => v.Output (parser)).SeparateWith (", "), 
					Body.Output (parser));
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

			public override string Output (LinqParser parser)
			{
				var functions = new StringBuilder ();
				foreach (var func in Functions)
				{
					functions.Append (func.Output (parser));
					functions.AppendLine ();
				}
				var globals = new StringBuilder ();
				foreach (var glob in Globals)
				{
					globals.Append (glob.Output (parser));
					globals.AppendLine ();
				}
				return globals.ToString () + "\n" + functions.ToString ();
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

		public static Variable Var (Type type, string name)
		{
			return new Variable (type, name, 0);
		}

		public static Variable Var (Type type, string name, int arraylen)
		{
			return new Variable (type, name, arraylen);
		}

		public static Field Fld (Type type, string name)
		{
			return new Field (type, name, 0);
		}

		public static Field Fld (Type type, string name, int arraylen)
		{
			return new Field (type, name, arraylen);
		}

		public static Field Fld (string name)
		{
			return new Field (null, name, 0);
		}

		public static Argument Arg (Type type, string name)
		{
			return new Argument (type, name, 0);
		}

		public static Argument Arg (Type type, string name, int arraylen)
		{
			return new Argument (type, name, arraylen);
		}

		public static Constant Const (Type type, string name, Expression value)
		{
			return new Constant (type, name, 0, value);
		}

		public static Constant Const (Type type, string name, int arraylen, Expression value)
		{
			return new Constant (type, name, arraylen, value);
		}

		public static VariableRef VRef (Variable variable)
		{
			return new VariableRef (variable);
		}

		public static ArrayRef ARef (Variable variable, Expression index)
		{
			return new ArrayRef (variable, index);
		}

		public static MacroParam MPar (Type type)
		{
			return new MacroParam (type);
		}

		public static MacroParam MDPar (MacroDefinition definition)
		{
			return new MacroDefParam (definition);
		}

		public static MacroResultVar MRes (Type type)
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

		public static Block Blk (params Statement[] statements)
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

		public static WhileLoop While (Expression condition, Statement body)
		{
			return new WhileLoop (condition, body);
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

		public static MacroCall MCall (MacroDefinition target, Variable returnVar,
			params Ast[] parameters)
		{
			return new MacroCall (target, returnVar, parameters);
		}

		public static Function Fun (string name, Type returnType, IEnumerable<Argument> arguments,
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
