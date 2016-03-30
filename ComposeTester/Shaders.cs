namespace ComposeTester
{
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.GLTypes;
	using Compose3D.Shaders;
    using Compose3D.Textures;
	using OpenTK.Graphics.OpenGL;
	using System;
	using System.Linq;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public struct PathNode : IPositional<Vec3>, IDiffuseColor<Vec3>
	{
		public Vec3 position;
		public Vec3 color;

		Vec3 IPositional<Vec3>.Position
		{
			get { return position; }
			set { position = value; }
		}

		Vec3 IDiffuseColor<Vec3>.Diffuse
		{
			get { return color; }
			set { color = value; }
		}

		public override string ToString ()
		{
			return string.Format ("PathNode: Position={0}, Diffuse={1}", position, color);
		}
	}

	public static class ExampleShaders
	{
		public class ShadowUniforms : Uniforms
		{
			internal Uniform<Mat4> mvpMatrix;

			public ShadowUniforms (Program program) : base (program) { }
		}

		public class DiffuseFragment : Fragment, IDiffuseFragment
		{
			public Vec3 vertexDiffuse { get; set; }
		}

		public static Program PassThrough = new Program (
			GLShader.Create (ShaderType.VertexShader,
				() =>
				from v in Shader.Inputs<PathNode> ()
				select new DiffuseFragment ()
				{
					gl_Position = new Vec4 (v.position.X, v.position.Y, -1f, 1f),
					vertexDiffuse = v.color
				}
			),
			GLShader.Create (ShaderType.FragmentShader,
				() =>
				from f in Shader.Inputs<DiffuseFragment> ()
				select new
				{
					outputColor = f.vertexDiffuse
				}
			)
		);

		public static GLShader ShadowVertexShader ()
		{
			return GLShader.Create
			(
				ShaderType.VertexShader, () =>

				from v in Shader.Inputs<Vertex> ()
				from u in Shader.Uniforms<ShadowUniforms> ()
				select new Fragment ()
				{
					gl_Position = !u.mvpMatrix * new Vec4 (v.position, 1f)
				}
			);
		}
		
		public static GLShader ShadowFragmentShader ()
		{
			return GLShader.Create
			(
				ShaderType.FragmentShader, () =>

				from f in Shader.Inputs<Fragment> ()
				select new
				{
					fragmentDepth = f.gl_FragCoord.Z
				}
			);
		}
	}
}

