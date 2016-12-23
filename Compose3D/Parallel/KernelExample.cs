namespace Compose3D.Parallel
{
	using System;
	using Cloo;
	using CLTypes;
	using Extensions;

	public static class KernelExample
	{
		public static CLProgram Example (CLContext context)
		{
			return CLProgram.Create (context, nameof (Example), () =>
				from buffer in Kernel.Buffer<float> ()
				let i = Kernel.GetGlobalId (0)
				select new
				{
					result = new KernelResult<float> { { i, buffer[i] } }
				}
			);
		} 
	}
}
