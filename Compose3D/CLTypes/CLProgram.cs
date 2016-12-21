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
		private CLArguments _arguments;

		public CLProgram (ComputeContext context, string kernelName, string source,
			CLArguments arguments)
		{
			_comProgram = new ComputeProgram (context, source);
			_comKernel = _comProgram.CreateKernel (kernelName);
			_arguments = arguments;
			_arguments._program = this;
		}

		public static CLProgram Create<T> (ComputeContext context, string name, 
			Expression<Func<Kernel<T>>> func)
		{
			//var source = CLCCompiler.CreateShader (func);
			//Console.WriteLine (source);
			return new CLProgram (context, name, null, new CLArguments ());
		}

	}
}
