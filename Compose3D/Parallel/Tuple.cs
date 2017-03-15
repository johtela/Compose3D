namespace Compose3D.Parallel
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using CLTypes;

	[CLStruct]
	public struct CLTuple<T1, T2>
		where T1 : struct
		where T2 : struct
	{
		public T1 Item1;
		public T2 Item2;
	}
}
