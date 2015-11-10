namespace Compose3D.Shaders
{
	using Compose3D.Maths;
	using Compose3D.GLTypes;

	public class Fragment
	{
		[Builtin]
		public Vec4 gl_Position;
	}

	public class ColoredFragment : Fragment
	{
		public Vec3 vertexPosition;
		public Vec3 vertexNormal;
		public Vec3 vertexDiffuse;
		public Vec3 vertexSpecular;
		public float vertexShininess;
	}

	public class TexturedFragment : ColoredFragment
	{
		public Vec2 texturePosition;
	}
}