namespace Compose3D.CLTypes
{
    using System;

	public class CLError : Exception
	{
		public CLError (string msg) : base (msg) { }
	}
}
