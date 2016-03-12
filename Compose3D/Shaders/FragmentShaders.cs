namespace Compose3D.Shaders
{
	using System;
	using Maths;
	using GLTypes;
	using Textures;
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

	public class DiffuseFragment : Fragment
	{
		public Vec3 vertexPosition;
		public Vec3 vertexNormal;
		public Vec3 vertexDiffuse;		
	}
	
	public class SpecularFragment : DiffuseFragment
	{
		public Vec3 vertexSpecular;
		public float vertexShininess;
	}

	public class TexturedFragment : SpecularFragment
	{
		public Vec2 texturePosition;
	}

	public static class FragmentShaders
	{
		public static void Use () { }
		
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
				from f in Shader.Inputs<DiffuseFragment> ()
				select new 
				{
					outputColor = new Vec3 (f.vertexDiffuse)
				});
		}

		public static readonly Func<Sampler2D, Vec2, Vec3> TextureColor =
			GLShader.Function
			(
				() => TextureColor,

				(Sampler2D sampler, Vec2 texturePos) =>
				sampler.Texture (texturePos)[Coord.x, Coord.y, Coord.z]
			);
	}
}