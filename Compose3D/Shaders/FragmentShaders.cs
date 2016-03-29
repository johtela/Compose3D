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

	public class TexturedFragment<V> : SpecularFragment
		where V : struct, IVec<V, float>
	{
		public V texturePosition;
	}

	public class WindowFragment : Fragment
	{
		public Vec2 texturePosition;
	}

	public static class FragmentShaders
	{
		public static void Use () { }
		
		public static GLShader WhiteOutput<F> ()
			where F : Fragment
		{
			return GLShader.Create (ShaderType.FragmentShader, () =>
				from f in Shader.Inputs<F> ()
				select new 
				{
					outputColor = new Vec3 (1f)
				});
		}

		public static GLShader DirectOutput<F> ()
			where F : DiffuseFragment
		{
			return GLShader.Create (ShaderType.FragmentShader, () =>
				from f in Shader.Inputs<F> ()
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

		public static GLShader WindowOutput<F, U> ()
			where F : WindowFragment
			where U : WindowUniforms
		{
			return GLShader.Create (ShaderType.FragmentShader, () =>
				from f in Shader.Inputs<F> ()
				from u in Shader.Uniforms<U> ()
				select new
				{
					outputColor = TextureColor (!u.textureMap, f.texturePosition)
				});
		}
	}
}