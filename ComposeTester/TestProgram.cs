namespace ComposeTester
{
	using System;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using LinqCheck;
	using Visuals;

	public class TestProgram
	{
		public static VisualConsole VConsole = new VisualConsole ();		
		
		[STAThread]		
		static void Main (string[] args)
		{
			var wnd = new MaterialWindow ();
			wnd.Run ();
			//Task.Factory.StartNew (() =>
			//	Tester.RunTests (
			//		new VecTests (),
			//		new MatTests (),
			//		new QuatTests (),
			//		new SceneTests (),
			//		new IntervalTreeTests (),
			//		new BoundingTreeTests (),
			//		new KdTreeTests ()));
			//Application.Run (VConsole);
		}
	}
}