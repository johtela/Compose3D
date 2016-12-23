namespace Compose3D.CLTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cloo;
    using Parallel;

    public class CLContext
    {
        internal ComputeContext _comContext;
        private static List<ComputeDevice> _cpus;
        private static List<ComputeDevice> _gpus;

        static CLContext ()
        {
            _cpus = new List<ComputeDevice> ();
            _gpus = new List<ComputeDevice> ();

            var devices = from platform in ComputePlatform.Platforms
                          from device in platform.Devices
                          where device.Available
                          select device;

            foreach (var device in devices)
                (device.Type == ComputeDeviceTypes.Gpu ? _gpus : _cpus).Add (device);
        }

        private CLContext (ComputeContext context)
        {
            _comContext = context;
        }

        public static ICollection<ComputeDevice> Cpus
		{
			get	{ return _cpus.AsReadOnly (); }
		}

		public static ICollection<ComputeDevice> Gpus
		{
			get { return _gpus.AsReadOnly (); }
		}

		public static CLContext CreateContextForDevices (params ComputeDevice[] devices)
		{
			var platform = devices[0].Platform;
			if (!devices.All (d => d.Platform == platform))
				throw new ArgumentException ("Devices must belong to the same platform");
			var col = new List<ComputeDevice> (devices);
			var props = new ComputeContextPropertyList (platform);
			return new CLContext (new ComputeContext (col, props, null, IntPtr.Zero));
		}
	}
}
