namespace Compose3D.Shaders
{
	using System;
	using System.Runtime.InteropServices;
	using Maths;
	using GLTypes;
	using Geometry;
	using Textures;
	using OpenTK.Graphics.OpenGL;

	public static class VertexShaders
	{
		public static GLShader Passthrough<V, F> ()
			where V : struct, IVertex
			where F : Fragment, new ()
		{
			return GLShader.Create (ShaderType.VertexShader, () =>
				from v in Shader.Inputs<V> ()
				select new F ()
				{
					gl_Position = new Vec4 (v.position, 1f)
				});
		}
		
		public static GLShader PassthroughTexture<V, F> ()
			where V : struct, IVertex, ITextured
			where F : Fragment, IFragmentTexture<Vec2>, new ()
		{
			return GLShader.Create (ShaderType.VertexShader, () =>
				from v in Shader.Inputs<V> ()
				select new F ()
				{
					gl_Position = new Vec4 (v.position, 1f),
					fragTexturePos = v.texturePos
				});
		}			

		public static GLShader TransformedTexture<V, F, U> ()
			where V : struct, IVertex, ITextured
			where F : Fragment, IFragmentTexture<Vec2>, new ()
			where U : TransformUniforms
		{
			return GLShader.Create (ShaderType.VertexShader, () =>
				from v in Shader.Inputs<V> ()
				from u in Shader.Uniforms<U> ()
				select new F ()
				{
					gl_Position = !u.modelViewMatrix * new Vec4 (v.position, 1f),
					fragTexturePos = v.texturePos
				});
		}			

		public static GLShader BasicShader<V, F, U> ()
			where V : struct, IVertex, IVertexColor<Vec3>, ITextured, IReflective
			where F : Fragment, IFragmentPosition, IFragmentDiffuse, IFragmentSpecular, IFragmentTexture<Vec2>, 
					  IFragmentReflectivity, new ()
			where U : TransformUniforms
		{
			return GLShader.Create (ShaderType.VertexShader, () =>
				from v in Shader.Inputs<V> ()
				from t in Shader.Uniforms<U> ()
				let viewPos = !t.modelViewMatrix * new Vec4 (v.position, 1f)
				select new F ()
				{
					gl_Position = !t.perspectiveMatrix * viewPos,
					fragPosition = viewPos[Coord.x, Coord.y, Coord.z],
					fragNormal = (!t.normalMatrix * v.normal).Normalized,
					fragDiffuse = v.diffuse,
					fragSpecular = v.specular,
					fragShininess = v.shininess,
					fragTexturePos = v.texturePos,
					fragReflectivity = v.reflectivity
				});
		}
	}
}