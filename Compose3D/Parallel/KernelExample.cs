namespace Compose3D.Parallel
{
	using System;
	using Cloo;
	using CLTypes;
	using Extensions;

	public static class KernelExample
	{
		public static CLProgram Example (ComputeContext context)
		{
			return CLProgram.Create (context, nameof (Example), () =>
				from arg in Kernel.Argument<int> ()
				from buffer in Kernel.Buffer<float> ()
				select new
				{
					result = new KernelResult<float> { { arg, buffer[arg] } }
				}
			);
		} 
	}
}
