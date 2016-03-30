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

	public interface IVertexFragment
	{
		Vec3 vertexPosition { get; set; }
		Vec3 vertexNormal { get; set; }
	}

	public interface IDiffuseFragment
	{
		Vec3 vertexDiffuse { get; set; }
	}
	
	public interface ISpecularFragment
	{
		Vec3 vertexSpecular { get; set; }
		float vertexShininess { get; set; }
	}

	public interface ITexturedFragment<V>
		where V : struct, IVec<V, float>
	{
		V texturePosition { get; set; }
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
			where F : IDiffuseFragment
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
			where F : ITexturedFragment<Vec2>
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