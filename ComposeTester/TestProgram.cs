namespace ComposeTester
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using LinqCheck;
	using Compose3D.Reactive;
	using Visuals;
	using System.Windows.Forms;
	using OpenTK.Input;
	using Compose3D.CLTypes;
	using Compose3D.Parallel;
	using Compose3D.Compiler;

	public class TestProgram
	{
		public static VisualConsole VConsole = new VisualConsole ();		
		
		[STAThread]		
		static void Main (string[] args)
		{
			TestParallel ();
			//var wnd = new FighterWindow ();
			//wnd.Run ();
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

		private static void TestParallel ()
		{
			var device = CLContext.Gpus.First ();
			var context = CLContext.CreateContextForDevices (device);
			var kern = ParPerlin.Example ();
			var prog = new CLProgram (context, kern);
		}
	}
}