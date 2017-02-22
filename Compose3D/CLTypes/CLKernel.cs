namespace Compose3D.CLTypes
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using Cloo;
	using Extensions;
	using Compiler;
	using Parallel;

	public class CLKernel
	{
		internal ComputeKernel _comKernel;
		internal string _name;
		internal LambdaExpression _expr;

		internal CLKernel (string name, LambdaExpression expr)
		{
			_name = name;
			_expr = expr;
		}

		protected void CheckIsInitialized ()
		{
			if (_comKernel == null)
				throw new InvalidOperationException ("Kernel is not compiled.");
		}

		protected void ExecuteSynchronously (CLCommandQueue queue, IEnumerable<KernelArg> args, 
			long[] workSizes)
		{
			var cq = queue._comQueue;
			cq.Execute (_comKernel, null, workSizes, null, null);
			foreach (var arg in args)
				arg.ReadResult (queue);
			cq.Finish ();
		}

		public static CLKernel<T1> Create<T1> (string name,
			Expression<Func<T1, Kernel<KernelResult>>> func)
			where T1 : KernelArg
		{
			return new CLKernel<T1> (name, func);
		}

		public static CLKernel<T1, T2> Create<T1, T2> (string name, 
			Expression<Func<T1, T2, Kernel<KernelResult>>> func)
			where T1 : KernelArg
			where T2 : KernelArg
		{
			return new CLKernel<T1, T2> (name, func);
		}

		public static CLKernel<T1, T2, T3> Create<T1, T2, T3> (string name, 
			Expression<Func<T1, T2, T3, Kernel<KernelResult>>> func)
			where T1 : KernelArg
			where T2 : KernelArg
			where T3 : KernelArg
		{
			return new CLKernel<T1, T2, T3> (name, func);
		}

		public static CLKernel<T1, T2, T3, T4> Create<T1, T2, T3, T4> (string name, 
			Expression<Func<T1, T2, T3, T4, Kernel<KernelResult>>> func)
			where T1 : KernelArg
			where T2 : KernelArg
			where T3 : KernelArg
			where T4 : KernelArg
		{
			return new CLKernel<T1, T2, T3, T4> (name, func);
		}

		public static Func<TRes> Function<TRes> (Expression<Func<Func<TRes>>> member, Expression<Func<TRes>> func)
		{
			ClcParser.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

		public static Func<T1, TRes> Function<T1, TRes> (Expression<Func<Func<T1, TRes>>> member,
			Expression<Func<T1, TRes>> func)
		{
			ClcParser.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

		public static Func<T1, T2, TRes> Function<T1, T2, TRes> (Expression<Func<Func<T1, T2, TRes>>> member,
			Expression<Func<T1, T2, TRes>> func)
		{
			ClcParser.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

		public static Func<T1, T2, T3, TRes> Function<T1, T2, T3, TRes> (Expression<Func<Func<T1, T2, T3, TRes>>> member,
			Expression<Func<T1, T2, T3, TRes>> func)
		{
			ClcParser.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

		public static Func<T1, T2, T3, T4, TRes> Function<T1, T2, T3, T4, TRes> (
			Expression<Func<Func<T1, T2, T3, T4, TRes>>> member,
			Expression<Func<T1, T2, T3, T4, TRes>> func)
		{
			ClcParser.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

		public static Func<T1, T2, T3, T4, T5, TRes> Function<T1, T2, T3, T4, T5, TRes> (
			Expression<Func<Func<T1, T2, T3, T4, T5, TRes>>> member,
			Expression<Func<T1, T2, T3, T4, T5, TRes>> func)
		{
			ClcParser.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

		public static Func<T1, T2, T3, T4, T5, T6, TRes> Function<T1, T2, T3, T4, T5, T6, TRes> (
			Expression<Func<Func<T1, T2, T3, T4, T5, T6, TRes>>> member,
			Expression<Func<T1, T2, T3, T4, T5, T6, TRes>> func)
		{
			ClcParser.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

		public static Macro<TRes> Macro<TRes> (
			Expression<Func<Macro<TRes>>> member,
			Expression<Macro<TRes>> macro)
		{
			ClcParser.CreateMacro ((member.Body as MemberExpression).Member, macro);
			return macro.Compile ();
		}

		public static Macro<T1, TRes> Macro<T1, TRes> (
			Expression<Func<Macro<T1, TRes>>> member,
			Expression<Macro<T1, TRes>> macro)
		{
			ClcParser.CreateMacro ((member.Body as MemberExpression).Member, macro);
			return macro.Compile ();
		}

		public static Macro<T1, T2, TRes> Macro<T1, T2, TRes> (
			Expression<Func<Macro<T1, T2, TRes>>> member,
			Expression<Macro<T1, T2, TRes>> macro)
		{
			ClcParser.CreateMacro ((member.Body as MemberExpression).Member, macro);
			return macro.Compile ();
		}

		public static Macro<T1, T2, T3, TRes> Macro<T1, T2, T3, TRes> (
			Expression<Func<Macro<T1, T2, T3, TRes>>> member,
			Expression<Macro<T1, T2, T3, TRes>> macro)
		{
			ClcParser.CreateMacro ((member.Body as MemberExpression).Member, macro);
			return macro.Compile ();
		}

		public static Macro<T1, T2, T3, T4, TRes> Macro<T1, T2, T3, T4, TRes> (
			Expression<Func<Macro<T1, T2, T3, T4, TRes>>> member,
			Expression<Macro<T1, T2, T3, T4, TRes>> macro)
		{
			ClcParser.CreateMacro ((member.Body as MemberExpression).Member, macro);
			return macro.Compile ();
		}

		public static Macro<T1, T2, T3, T4, T5, TRes> Macro<T1, T2, T3, T4, T5, TRes> (
			Expression<Func<Macro<T1, T2, T3, T4, T5, TRes>>> member,
			Expression<Macro<T1, T2, T3, T4, T5, TRes>> macro)
		{
			ClcParser.CreateMacro ((member.Body as MemberExpression).Member, macro);
			return macro.Compile ();
		}

		public static Macro<T1, T2, T3, T4, T5, T6, TRes> Macro<T1, T2, T3, T4, T5, T6, TRes> (
			Expression<Func<Macro<T1, T2, T3, T4, T5, T6, TRes>>> member,
			Expression<Macro<T1, T2, T3, T4, T5, T6, TRes>> macro)
		{
			ClcParser.CreateMacro ((member.Body as MemberExpression).Member, macro);
			return macro.Compile ();
		}
	}

	public class CLKernel<T1> : CLKernel
		where T1 : KernelArg
	{
		internal CLKernel (string name, LambdaExpression expr)
			: base (name, expr) { }

		public void Execute (CLCommandQueue queue, T1 arg1,	params long[] workSizes)
		{
			CheckIsInitialized ();
			arg1.PushToCLKernel (this, 0);
			ExecuteSynchronously (queue, new KernelArg[] { arg1 }, workSizes);
		}
	}

	public class CLKernel<T1, T2> : CLKernel
		where T1 : KernelArg
		where T2 : KernelArg
	{
		internal CLKernel (string name, LambdaExpression expr)
			: base (name, expr) { }

		public void Execute (CLCommandQueue queue, T1 arg1, T2 arg2, params long[] workSizes)
		{
			CheckIsInitialized ();
			arg1.PushToCLKernel (this, 0);
			arg2.PushToCLKernel (this, 1);
			ExecuteSynchronously (queue, new KernelArg[] { arg1, arg2 }, workSizes);
		}
	}

	public class CLKernel<T1, T2, T3> : CLKernel
		where T1 : KernelArg
		where T2 : KernelArg
		where T3 : KernelArg
	{
		internal CLKernel (string name, LambdaExpression expr)
			: base (name, expr) { }

		public void Execute (CLCommandQueue queue, T1 arg1, T2 arg2, T3 arg3, 
			params long[] workSizes)
		{
			CheckIsInitialized ();
			arg1.PushToCLKernel (this, 0);
			arg2.PushToCLKernel (this, 1);
			arg3.PushToCLKernel (this, 2);
			ExecuteSynchronously (queue, new KernelArg[] { arg1, arg2, arg3 }, workSizes);
		}
	}

	public class CLKernel<T1, T2, T3, T4> : CLKernel
		where T1 : KernelArg
		where T2 : KernelArg
		where T3 : KernelArg
		where T4 : KernelArg
	{
		internal CLKernel (string name, LambdaExpression expr)
			: base (name, expr) { }

		public void Execute (CLCommandQueue queue, T1 arg1, T2 arg2, T3 arg3, T4 arg4, 
			params long[] workSizes)
		{
			CheckIsInitialized ();
			arg1.PushToCLKernel (this, 0);
			arg2.PushToCLKernel (this, 1);
			arg3.PushToCLKernel (this, 2);
			arg4.PushToCLKernel (this, 3);
			ExecuteSynchronously (queue, new KernelArg[] { arg1, arg2, arg3, arg4 }, workSizes);
		}
	}
}