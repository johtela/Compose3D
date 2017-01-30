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
		private KernelArguments _arguments;

		public CLProgram (CLContext context, string kernelName, string source,
			KernelArguments arguments)
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
			_arguments = arguments;
			_arguments._program = this;
		}

		public static CLProgram Create<T> (CLContext context, string name, 
			Expression<Func<Kernel<T>>> func)
		{
			var args = new KernelArguments ();
			var source = CLCCompiler.CreateKernel (name, func, args);
			Console.WriteLine (source);
			return new CLProgram (context, name, source, args);
		}

		public static CLProgram Create<T> (CLContext context, string name,
			Expression<Func<Kernel<KernelResult<T>>>> func)
		{
			var args = new KernelArguments ();
			var source = CLCCompiler.CreateKernel (name, func, args);
			Console.WriteLine (source);
			return new CLProgram (context, name, source, args);
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
}
