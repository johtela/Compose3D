namespace Compose3D.Shaders
{
	using System;
	using System.Runtime.InteropServices;
	using Maths;
	using GLTypes;
	using Geometry;
	using Textures;
	using OpenTK.Graphics.OpenGL;

	[StructLayout (LayoutKind.Sequential, Pack = 4)]
	public struct PositionalVertex : IVertex
	{
		public Vec3 position;
		[OmitInGlsl]
		public Vec3 normal;

		Vec3 IPositional<Vec3>.position
		{
			get { return position; }
			set { position = value; }
		}

		Vec3 IPlanar<Vec3>.normal
		{
			get { return normal; }
			set
			{
				if (value.IsNaN ())
					throw new ArgumentException ("Normal component NaN.");
				normal = value;
			}
		}

		public override string ToString ()
		{
			return string.Format ("[Vertex: Position={0}, Normal={1}]", position, normal);
		}
	}

	[StructLayout (LayoutKind.Sequential, Pack = 4)]
	public struct TexturedVertex : IVertex, ITextured
	{
		public Vec3 position;
		public Vec2 texturePos;
		[OmitInGlsl]
		public Vec3 normal;

		Vec3 IPositional<Vec3>.position
		{
			get { return position; }
			set { position = value; }
		}

		Vec2 ITextured.texturePos
		{
			get { return texturePos; }
			set { texturePos = value; }
		}

		Vec3 IPlanar<Vec3>.normal
		{
			get { return normal; }
			set
			{
				if (value.IsNaN ())
					throw new ArgumentException ("Normal component NaN.");
				normal = value;
			}
		}

		public override string ToString ()
		{
			return string.Format ("[Vertex: position={0}, texturePos={1}]", position, texturePos);
		}
	}

	public class TransformUniforms : Uniforms
	{
		public Uniform<Mat4> modelViewMatrix;
		public Uniform<Mat4> perspectiveMatrix;
		public Uniform<Mat3> normalMatrix;

		public TransformUniforms (Program program) : base (program) { }

		public void UpdateModelViewAndNormalMatrices (Mat4 modelView)
		{
			modelViewMatrix &= modelView;
			normalMatrix &= new Mat3 (modelView).Inverse.Transposed;
		}
	}

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
				}
			);
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
					gl_Position = !u.perspectiveMatrix * !u.modelViewMatrix * new Vec4 (v.position, 1f),
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
					fragReflectivity = v.reflectivity,
				});
		}
	}
}