namespace Compose3D.Shaders
{
	using Compose3D.Maths;
	using Compose3D.GLTypes;
	using OpenTK.Graphics.OpenGL;

	public class Fragment
	{
		/// <summary>
		/// Contains the position of the current vertex. Output variable, available in 
		/// vertex, tessellation evaluation and geometry languages.
		/// </summary>
		[Builtin]
		public Vec4 gl_Position;
		
		/// <summary>
		/// Contains the window-relative coordinates of the current fragment. Input variable, 
		/// only available in fragment shaders.
		/// </summary>
		[Builtin]
		public Vec4 gl_FragCoord;
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

	public static class FragmentShaders
	{
		public static GLShader WhiteOutput ()
		{
			return GLShader.Create (ShaderType.FragmentShader, () =>
				from f in Shader.Inputs<Fragment> ()
				select new 
				{
					outputColor = new Vec3 (1f)
				});
		}

		public static GLShader DirectOutput ()
		{
			return GLShader.Create (ShaderType.FragmentShader, () =>
				from f in Shader.Inputs<ColoredFragment> ()
				select new 
				{
					outputColor = new Vec3 (f.vertexDiffuse)
				});
		}
	}
}