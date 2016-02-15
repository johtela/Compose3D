namespace ComposeTester
{
	using System;
	using System.Threading.Tasks;
	using LinqCheck;
	using Visuals;
	using System.Windows.Forms;

	public class TestProgram
	{
		public static VisualConsole VConsole = new VisualConsole ();		
		
		[STAThread]		
		static void Main (string[] args)
		{
//			var wnd = new TestWindow ();
//			Console.WriteLine (GL.GetString (StringName.Version));
			//wnd.Init ();
			//wnd.Run ();
			Task.Factory.StartNew (() =>
				Tester.RunTestsTimed (
	//				new VecTests (),
	//				new MatTests (),
	//				new QuatTests (),
					new SceneTests (),
					new IntervalTreeTests (),
					new BoundingTreeTests ()));
			Application.Run (VConsole);
		}
	}
}