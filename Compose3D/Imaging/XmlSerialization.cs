namespace Compose3D.Imaging
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Xml.Linq;
	using System.Threading.Tasks;
	using Maths;
	using Extensions;

	internal static class XmlSerialization
	{
		public static int AttrInt (this XElement xelem, string attribute)
		{
			return int.Parse (xelem.Attribute (attribute).Value, CultureInfo.InvariantCulture);
		}

		public static float AttrFloat (this XElement xelem, string attribute)
		{
			return float.Parse (xelem.Attribute (attribute).Value, CultureInfo.InvariantCulture);
		}

		public static bool AttrBool (this XElement xelem, string attribute)
		{
			return bool.Parse (xelem.Attribute (attribute).Value);
		}

		public static V AttrVec<V> (this XElement xelem, string attribute)
			where V : struct, IVec<V, float>
		{
			return Vec.Parse<V> (xelem.Attribute (attribute).Value);
		}

		public static T AttrEnum<T> (this XElement xelem, string attribute)
		{
			return (T)Enum.Parse (typeof (T), xelem.Attribute (attribute).Value);
		}

		public static void SaveColorMap<V> (this XElement xelem, ColorMap<V> colorMap)
			where V : struct, IVec<V, float>
		{
			xelem.Add (new XElement ("ColorMap",
				from kv in colorMap
				select new XElement ("SamplePoint",
					new XAttribute ("Key", kv.Key),
					new XAttribute ("Value", kv.Value))));
		}

		public static ColorMap<V> LoadColorMap<V> (this XElement xelem)
			where V : struct, IVec<V, float>
		{
			return new ColorMap<V> (
				from sp in xelem.Element ("ColorMap").Descendants ("SamplePoint")
				select Tuple.Create (sp.AttrFloat ("Key"), sp.AttrVec<V> ("Value")));
		}
	}
}
