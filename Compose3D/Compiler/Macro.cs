namespace Compose3D.Compiler
{
	public delegate TRes Macro<T1, TRes> (T1 arg1);
	public delegate TRes Macro<T1, T2, TRes> (T1 arg1, T2 arg2);
	public delegate TRes Macro<T1, T2, T3, TRes> (T1 arg1, T2 arg2, T3 arg3);
	public delegate TRes Macro<T1, T2, T3, T4, TRes> (T1 arg1, T2 arg2, T3 arg3, T4 arg4);
}
