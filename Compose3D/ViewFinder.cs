namespace Compose3D
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
		
	public class ViewFinder : MarshalByRefObject
	{
		public Type[] ViewTypes ()
		{
			return (from assy in AppDomain.CurrentDomain.GetAssemblies ()
					from type in assy.GetTypes ()
					where type.GetInterfaces ().Contains (typeof (IView3D))
					select type).ToArray ();
		}
	}
}