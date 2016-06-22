namespace Compose3D.Viewer
{
	using System;
	using System.Linq;
	using System.Reflection;
	using OpenTK;

	public static class Program
	{
		private static IView3D[] _views;

		[STAThread]
		static void Main (string[] args)
		{
			if (args.Length != 1)
				Console.WriteLine ("Usage: Compose3D.Viewer <assemblypath>");
			CreateViews (args[0]);
			var wnd = new GameWindow ();
			wnd.Run ();
		}

		private static void CreateViews (string assemblyPath)
		{
			var appDomain = AppDomain.CreateDomain ("Compose3D.Viewer", null, 
				AppDomain.CurrentDomain.BaseDirectory, "", false);
			var assemblyName = AssemblyName.GetAssemblyName (assemblyPath);
			var assy = appDomain.Load (assemblyName);
			_views = (from vt in assy.GetTypes ()
					  where vt.GetInterfaces ().Contains (typeof (IView3D))
					  select (IView3D)Activator.CreateInstance (vt)).ToArray ();
		}
	}
}
