namespace Compose3D.CLTypes
{
	using System;
	using System.Linq.Expressions;
	using Cloo;
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

		public static CLKernel<TRes> Create<TRes> (string name,
			Expression<Func<Kernel<KernelResult<TRes>>>> func)
			where TRes : struct
		{
			return new CLKernel<TRes> (name, func);
		}

		public static CLKernel<T1, TRes> Create<T1, TRes> (string name,
			Expression<Func<T1, Kernel<KernelResult<TRes>>>> func)
			where TRes : struct
			where T1 : KernelArg
		{
			return new CLKernel<T1, TRes> (name, func);
		}

		public static CLKernel<T1, T2, TRes> Create<T1, T2, TRes> (string name, 
			Expression<Func<T1, Kernel<KernelResult<TRes>>>> func)
			where TRes : struct
			where T1 : KernelArg
			where T2 : KernelArg
		{
			return new CLKernel<T1, T2, TRes> (name, func);
		}

		public static CLKernel<T1, T2, T3, TRes> Create<T1, T2, T3, TRes> (string name, 
			Expression<Func<T1, T2, T3, Kernel<KernelResult<TRes>>>> func)
			where TRes : struct
			where T1 : KernelArg
			where T2 : KernelArg
			where T3 : KernelArg
		{
			return new CLKernel<T1, T2, T3, TRes> (name, func);
		}

		public static CLKernel<T1, T2, T3, T4, TRes> Create<T1, T2, T3, T4, TRes> (string name, 
			Expression<Func<T1, T2, T3, T4, Kernel<KernelResult<TRes>>>> func)
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
	}

	public class CLKernel<TRes> : CLKernel
		where TRes : struct
	{
		internal CLKernel (string name, LambdaExpression expr)
			: base (name, expr) { }
	}

	public class CLKernel<T1, TRes> : CLKernel
		where TRes : struct
		where T1 : KernelArg
	{
		internal CLKernel (string name, LambdaExpression expr)
			: base (name, expr) { }
	}

	public class CLKernel<T1, T2, TRes> : CLKernel
		where TRes : struct
		where T1 : KernelArg
		where T2 : KernelArg
	{
		internal CLKernel (string name, LambdaExpression expr)
			: base (name, expr) { }
	}

	public class CLKernel<T1, T2, T3, TRes> : CLKernel
		where TRes : struct
		where T1 : KernelArg
		where T2 : KernelArg
		where T3 : KernelArg
	{
		internal CLKernel (string name, LambdaExpression expr)
			: base (name, expr) { }
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
	}
}