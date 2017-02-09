namespace Compose3D.CLTypes
{
	using System;
	using System.Linq;
	using System.Linq.Expressions;
	using Cloo;
	using Parallel;

	public class CLProgram
	{
		internal ComputeProgram _comProgram;
		internal ComputeKernel _comKernel;

		public CLProgram (CLContext context, string kernelName, string source)
		{
			_comProgram = new ComputeProgram (context._comContext, source);
			try
			{
				_comProgram.Build (null, null, null, IntPtr.Zero);
			}
			catch (BuildProgramFailureComputeException)
			{
				throw new CLError ("Error building program. Build log:\n" +
					_comProgram.GetBuildLog (_comProgram.Devices.First ()));
			}
			_comKernel = _comProgram.CreateKernel (kernelName);
		}

		private static string Compile (string name, LambdaExpression expr)
		{
			var args = new KernelArguments ();
			var source = CLCCompiler.CreateKernel (name, expr, args);
			Console.WriteLine (source);
			return source;
		}

		public static CLProgram<TRes> Create<TRes> (CLContext context, string name,
			Expression<Func<Kernel<KernelResult<TRes>>>> func)
			where TRes : struct
		{
			return new CLProgram<TRes> (context, name, Compile (name, func));
		}

		public static CLProgram<T1, TRes> Create<T1, TRes> (CLContext context, string name,
			Expression<Func<T1, Kernel<KernelResult<TRes>>>> func)
			where TRes : struct
			where T1 : KernelArg
		{
			return new CLProgram<T1, TRes> (context, name, Compile (name, func));
		}

		public static CLProgram<T1, T2, TRes> Create<T1, T2, TRes> (CLContext context, 
			string name, Expression<Func<T1, Kernel<KernelResult<TRes>>>> func)
			where TRes : struct
			where T1 : KernelArg
			where T2 : KernelArg
		{
			return new CLProgram<T1, T2, TRes> (context, name, Compile (name, func));
		}

		public static CLProgram<T1, T2, T3, TRes> Create<T1, T2, T3, TRes> (CLContext context,
			string name, Expression<Func<T1, T2, T3, Kernel<KernelResult<TRes>>>> func)
			where TRes : struct
			where T1 : KernelArg
			where T2 : KernelArg
			where T3 : KernelArg
		{
			return new CLProgram<T1, T2, T3, TRes> (context, name, Compile (name, func));
		}

		public static CLProgram<T1, T2, T3, T4, TRes> Create<T1, T2, T3, T4, TRes> (CLContext context,
			string name, Expression<Func<T1, T2, T3, T4, Kernel<KernelResult<TRes>>>> func)
			where TRes : struct
			where T1 : KernelArg
			where T2 : KernelArg
			where T3 : KernelArg
			where T4 : KernelArg
		{
			return new CLProgram<T1, T2, T3, T4, TRes> (context, name, Compile (name, func));
		}

		public static Func<TRes> Function<TRes> (Expression<Func<Func<TRes>>> member, Expression<Func<TRes>> func)
		{
			CLCCompiler.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

		public static Func<T1, TRes> Function<T1, TRes> (Expression<Func<Func<T1, TRes>>> member,
			Expression<Func<T1, TRes>> func)
		{
			CLCCompiler.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

		public static Func<T1, T2, TRes> Function<T1, T2, TRes> (Expression<Func<Func<T1, T2, TRes>>> member,
			Expression<Func<T1, T2, TRes>> func)
		{
			CLCCompiler.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

		public static Func<T1, T2, T3, TRes> Function<T1, T2, T3, TRes> (Expression<Func<Func<T1, T2, T3, TRes>>> member,
			Expression<Func<T1, T2, T3, TRes>> func)
		{
			CLCCompiler.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

		public static Func<T1, T2, T3, T4, TRes> Function<T1, T2, T3, T4, TRes> (
			Expression<Func<Func<T1, T2, T3, T4, TRes>>> member,
			Expression<Func<T1, T2, T3, T4, TRes>> func)
		{
			CLCCompiler.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

		public static Func<T1, T2, T3, T4, T5, TRes> Function<T1, T2, T3, T4, T5, TRes> (
			Expression<Func<Func<T1, T2, T3, T4, T5, TRes>>> member,
			Expression<Func<T1, T2, T3, T4, T5, TRes>> func)
		{
			CLCCompiler.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}

		public static Func<T1, T2, T3, T4, T5, T6, TRes> Function<T1, T2, T3, T4, T5, T6, TRes> (
			Expression<Func<Func<T1, T2, T3, T4, T5, T6, TRes>>> member,
			Expression<Func<T1, T2, T3, T4, T5, T6, TRes>> func)
		{
			CLCCompiler.CreateFunction ((member.Body as MemberExpression).Member, func);
			return func.Compile ();
		}
	}

	public class CLProgram<TRes> : CLProgram 
		where TRes : struct
	{
		public CLProgram (CLContext context, string kernelName, string source)
			: base (context, kernelName, source) { }
	}

	public class CLProgram<T1, TRes> : CLProgram
		where TRes : struct
		where T1 : KernelArg
	{
		public CLProgram (CLContext context, string kernelName, string source)
			: base (context, kernelName, source) { }
	}

	public class CLProgram<T1, T2, TRes> : CLProgram
		where TRes : struct
		where T1 : KernelArg
		where T2 : KernelArg
	{
		public CLProgram (CLContext context, string kernelName, string source)
			: base (context, kernelName, source) { }
	}

	public class CLProgram<T1, T2, T3, TRes> : CLProgram
		where TRes : struct
		where T1 : KernelArg
		where T2 : KernelArg
		where T3 : KernelArg
	{
		public CLProgram (CLContext context, string kernelName, string source)
			: base (context, kernelName, source) { }
	}

	public class CLProgram<T1, T2, T3, T4, TRes> : CLProgram
		where TRes : struct
		where T1 : KernelArg
		where T2 : KernelArg
		where T3 : KernelArg
		where T4 : KernelArg
	{
		public CLProgram (CLContext context, string kernelName, string source)
			: base (context, kernelName, source) { }
	}
}
