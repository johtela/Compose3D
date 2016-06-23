namespace Compose3D.Viewer
{
	using System;
	using System.Linq;
	using System.Reflection;
	using OpenTK;
	using Extensions;

	public static class Program
	{
		public static IView3D[] Views { get; private set; }

		[STAThread]
		static void Main (string[] args)
		{
			if (args.Length != 1)
				Console.WriteLine ("Usage: Compose3D.Viewer <assemblypath>");
			CreateViews (args[0]);
			if (Views.Length == 0)
				Console.WriteLine ("No views found in the assembly!");
			var viewer = new Viewer (Views[0]);
			viewer._window.Run ();
		}

		private static void CreateViews (string assemblyPath)
		{
			var appDomain = AppDomain.CreateDomain ("Compose3D.Viewer", null, 
				AppDomain.CurrentDomain.BaseDirectory, "", false);
			var assemblyName = AssemblyName.GetAssemblyName (assemblyPath).FullName;
			var finder = (ViewFinder)appDomain.CreateInstanceAndUnwrap (assemblyName, 
				typeof (ViewFinder).FullName);
				
			Views = finder.ViewTypes ().Map (vt => 
				(IView3D)appDomain.CreateInstanceAndUnwrap (assemblyName, vt.FullName));
		}
	}
}
