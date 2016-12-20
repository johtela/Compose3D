namespace Compose3D.Parallel
{
	using System;
	using System.Linq.Expressions;
	using Cloo;
	using CLTypes;

	public class KernelExample : KernelArguments
	{
		CLInput<int> param1;
		CLBuffer<float> buffer;

		public KernelExample () : base (null)
		{
			param1 &= 1;
		}

		public static Expression<Func<KernelExample>> Example ()
		{
			return () => from arg in Kernel.Arguments<KernelExample> ()
						 select new
						 {
							 buffer = ((!arg.buffer)[1] = 1f)
						 };
		} 
	}
}
