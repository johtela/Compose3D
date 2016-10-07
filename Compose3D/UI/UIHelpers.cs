namespace Compose3D.UI
{
	using System.Drawing;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK.Input;
	using Visuals;
	using Extensions;
	using Maths;

	public static class UIHelpers
	{
		public static Vec3 ToVec3 (this Color color)
		{
			return new Vec3 (color.R / 255f, color.G / 255f, color.B / 255f);
		}

		public static Vec4 ToVec4 (this Color color)
		{
			return new Vec4 (color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
		}

		public static Color ToColor (this Vec3 vec)
		{
			return VisualHelpers.ColorFromRGB (vec.X, vec.Y, vec.Z);
		}

		public static Color ToColor (this Vec4 vec)
		{
			return VisualHelpers.ColorFromRGBA (vec.X, vec.Y, vec.Z, vec.W);
		}
	}
}
