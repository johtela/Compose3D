namespace Compose3D.CLTypes
{
	using System.Linq;
	using Cloo;

	public class CLCommandQueue
	{
		private CLContext _context;
		internal ComputeCommandQueue _comQueue;

		public CLCommandQueue (CLContext context, ComputeDevice device, ComputeCommandQueueFlags flags)
		{
			_context = context;
			_comQueue = new ComputeCommandQueue (context._comContext, device, flags);
		}

		public CLCommandQueue (CLContext context)
			: this (context, context.Devices.First (), ComputeCommandQueueFlags.None) { }
	}
}
