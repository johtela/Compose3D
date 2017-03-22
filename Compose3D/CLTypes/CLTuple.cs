namespace Compose3D.CLTypes
{
	[CLStruct]
	public struct CLTuple<T1, T2>
		where T1 : struct
		where T2 : struct
	{
		public T1 Item1;
		public T2 Item2;
	}
}