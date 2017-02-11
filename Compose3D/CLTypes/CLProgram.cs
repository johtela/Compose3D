namespace Compose3D.CLTypes
{
	using System;
	using System.Linq;
	using Cloo;

	public class CLProgram
	{
		internal ComputeProgram _comProgram;

		public CLProgram (CLContext context, params CLKernel[] kernels)
		{
			var source = ClcParser.CompileKernels (kernels);
			Console.WriteLine (source);
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
			foreach (var kernel in kernels)
				kernel._comKernel = _comProgram.CreateKernel (kernel._name);
		}
	}
}
