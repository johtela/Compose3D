namespace ComposeTester
{
	using System;
	using System.Threading.Tasks;
	using LinqCheck;
	using Compose3D.Reactive;
	using Visuals;
	using System.Windows.Forms;
	using OpenTK.Input;

	public class TestProgram
	{
		public static VisualConsole VConsole = new VisualConsole ();		
		
		[STAThread]		
		static void Main (string[] args)
		{
			var wnd = new MaterialWindow ();
			wnd.Run ();
//			Task.Factory.StartNew (() =>
//				Tester.RunTestsTimed (
//					new VecTests (),
//					new MatTests (),
//					new QuatTests (),
//					new SceneTests (),
//					new IntervalTreeTests (),
//					new BoundingTreeTests (),
//					new KdTreeTests ()));
//			Application.Run (VConsole);
		}
	}
}