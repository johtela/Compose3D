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
			var clickFSM = new MouseFSM.Click (MouseButton.Left,
				React.By<MouseDevice> (() => Console.WriteLine ("Left down")),
				React.By<MouseDevice> (() => Console.WriteLine ("Left click")));

			var input = new MouseDevice ();
			clickFSM.Run (input);
			clickFSM.Run (input);

			//var wnd = new TestWindow ();
			//wnd.Run ();
			//Task.Factory.StartNew (() =>
			//	Tester.RunTestsTimed (
			//		new VecTests (),
			//		new MatTests (),
			//		new QuatTests (),
			//		new SceneTests (),
			//		new IntervalTreeTests (),
			//		new BoundingTreeTests ()));
			//Application.Run (VConsole);
		}
	}
}