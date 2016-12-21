namespace Compose3D.Parallel
{
	using System;
	using System.Linq.Expressions;
	using Cloo;
	using CLTypes;
	using Extensions;

	public static class KernelExample
	{
		public static CLProgram Example (ComputeContext context)
		{
			return CLProgram.Create (context, nameof (Example),
				() => from arg in Kernel.Argument<int> ()
					  from buffer in Kernel.Buffer<float> ()
					  select buffer.Replace (arg, 0f));
		} 
	}
}
