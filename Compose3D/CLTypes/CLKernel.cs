﻿namespace Compose3D.CLTypes
{
	using System;
	using System.Linq.Expressions;
	using Cloo;
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

		protected void ExecuteSynchronously<TRes> (CLCommandQueue queue, Buffer<TRes> result, long[] workSizes)
			where TRes : struct
		{
			var cq = queue._comQueue;
			cq.Execute (_comKernel, null, workSizes, null, null);
			cq.ReadFromBuffer (result._comBuffer, ref result._data, true, null);
			cq.Finish ();
		}

		public static CLKernel<TRes> Create<TRes> (string name,
			Expression<Func<Kernel<BufferResult<TRes>>>> func)
			where TRes : struct
		{
			return new CLKernel<TRes> (name, func);
		}

		public static CLKernel<T1, TRes> Create<T1, TRes> (string name,
			Expression<Func<T1, Kernel<BufferResult<TRes>>>> func)
			where TRes : struct
			where T1 : KernelArg
		{
			return new CLKernel<T1, TRes> (name, func);
		}

		public static CLKernel<T1, T2, TRes> Create<T1, T2, TRes> (string name, 
			Expression<Func<T1, Kernel<BufferResult<TRes>>>> func)
			where TRes : struct
			where T1 : KernelArg
			where T2 : KernelArg
		{
			return new CLKernel<T1, T2, TRes> (name, func);
		}

		public static CLKernel<T1, T2, T3, TRes> Create<T1, T2, T3, TRes> (string name, 
			Expression<Func<T1, T2, T3, Kernel<BufferResult<TRes>>>> func)
			where TRes : struct
			where T1 : KernelArg
			where T2 : KernelArg
			where T3 : KernelArg
		{
			return new CLKernel<T1, T2, T3, TRes> (name, func);
		}

		public static CLKernel<T1, T2, T3, T4, TRes> Create<T1, T2, T3, T4, TRes> (string name, 
			Expression<Func<T1, T2, T3, T4, Kernel<BufferResult<TRes>>>> func)
			where TRes : struct
			where T1 : KernelArg
			where T2 : KernelArg
			where T3 : KernelArg
			where T4 : KernelArg
		{
			return new CLKernel<T1, T2, T3, T4, TRes> (name, func);
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

	public class CLKernel<TRes> : CLKernel
		where TRes : struct
	{
		internal CLKernel (string name, LambdaExpression expr)
			: base (name, expr) { }

		public void Execute (CLCommandQueue queue, Buffer<TRes> result, params long[] workSizes)
		{
			CheckIsInitialized ();
			result.PushToCLKernel (this, 0);
			ExecuteSynchronously (queue, result, workSizes);
		}
	}

	public class CLKernel<T1, TRes> : CLKernel
		where TRes : struct
		where T1 : KernelArg
	{
		internal CLKernel (string name, LambdaExpression expr)
			: base (name, expr) { }

		public void Execute (CLCommandQueue queue, T1 arg1, Buffer<TRes> result, 
			params long[] workSizes)
		{
			CheckIsInitialized ();
			arg1.PushToCLKernel (this, 0);
			result.PushToCLKernel (this, 1);
			ExecuteSynchronously (queue, result, workSizes);
		}
	}

	public class CLKernel<T1, T2, TRes> : CLKernel
		where TRes : struct
		where T1 : KernelArg
		where T2 : KernelArg
	{
		internal CLKernel (string name, LambdaExpression expr)
			: base (name, expr) { }

		public void Execute (CLCommandQueue queue, T1 arg1, T2 arg2, Buffer<TRes> result, 
			params long[] workSizes)
		{
			CheckIsInitialized ();
			arg1.PushToCLKernel (this, 0);
			arg2.PushToCLKernel (this, 1);
			result.PushToCLKernel (this, 2);
			ExecuteSynchronously (queue, result, workSizes);
		}
	}

	public class CLKernel<T1, T2, T3, TRes> : CLKernel
		where TRes : struct
		where T1 : KernelArg
		where T2 : KernelArg
		where T3 : KernelArg
	{
		internal CLKernel (string name, LambdaExpression expr)
			: base (name, expr) { }

		public void Execute (CLCommandQueue queue, T1 arg1, T2 arg2, T3 arg3, Buffer<TRes> result,
			params long[] workSizes)
		{
			CheckIsInitialized ();
			arg1.PushToCLKernel (this, 0);
			arg2.PushToCLKernel (this, 1);
			arg3.PushToCLKernel (this, 2);
			result.PushToCLKernel (this, 3);
			ExecuteSynchronously (queue, result, workSizes);
		}
	}

	public class CLKernel<T1, T2, T3, T4, TRes> : CLKernel
		where TRes : struct
		where T1 : KernelArg
		where T2 : KernelArg
		where T3 : KernelArg
		where T4 : KernelArg
	{
		internal CLKernel (string name, LambdaExpression expr)
			: base (name, expr) { }

		public void Execute (CLCommandQueue queue, T1 arg1, T2 arg2, T3 arg3, T4 arg4, 
			Buffer<TRes> result, params long[] workSizes)
		{
			CheckIsInitialized ();
			arg1.PushToCLKernel (this, 0);
			arg2.PushToCLKernel (this, 1);
			arg3.PushToCLKernel (this, 2);
			arg4.PushToCLKernel (this, 3);
			result.PushToCLKernel (this, 4);
			ExecuteSynchronously (queue, result, workSizes);
		}
	}
}