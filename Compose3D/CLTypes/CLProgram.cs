namespace Compose3D.CLTypes
{
	using System;
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
			_comProgram.Build (null, null, null, IntPtr.Zero);
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

	}
}
