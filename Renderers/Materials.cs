namespace Compose3D.Renderers
{
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.GLTypes;
	using Compose3D.Reactive;
	using Compose3D.SceneGraph;
	using Compose3D.Shaders;
	using OpenTK.Graphics.OpenGL4;
	using System;
	using System.Linq;
	using System.Runtime.InteropServices;

	[StructLayout (LayoutKind.Sequential, Pack = 4)]
	public struct MaterialVertex : IVertex, IDiffuseColor<Vec3>
	{
		public Vec3 position;
		public Vec3 normal;
		public Vec3 diffuse;

		Vec3 IPositional<Vec3>.position
		{
			get { return position; }
			set { position = value; }
		}

		Vec3 IDiffuseColor<Vec3>.diffuse
		{
			get { return diffuse; }
			set { diffuse = value; }
		}

		Vec3 IPlanar<Vec3>.normal
		{
			get { return normal; }
			set
			{
				if (value.IsNaN ())
					throw new ArgumentException ("Normal component NaN");
				normal = value;
			}
		}

		public override string ToString ()
		{
			return string.Format ("[Vertex: position={0}, diffuse={1}, normal={3}]",
				position, diffuse, normal);
		}
	}
	
	public class MaterialFragment : Fragment, IFragmentPosition, IFragmentDiffuse
	{
		public Vec3 fragPosition { get; set; }
		public Vec3 fragNormal { get; set; }
		public Vec3 fragDiffuse { get; set; }
	}

	public class Materials
	{
		private static Program _materialShader;

		public static Reaction<Mesh<MaterialVertex>> Renderer ()
		{
			_materialShader = new Program (
				VertexShader (), 
				FragmentShader ());

			return React.By<Mesh<MaterialVertex>> (Render)
				.DepthTest ()
				.Culling ()
				.Program (_materialShader);
		}

		private static void Render (Mesh<MaterialVertex> mesh)
		{
			_materialShader.DrawElements (PrimitiveType.Triangles, mesh.VertexBuffer, mesh.IndexBuffer);
		}

		public static GLShader VertexShader ()
		{
			return GLShader.Create (ShaderType.VertexShader, () =>
				from v in Shader.Inputs<MaterialVertex> ()
				select new MaterialFragment ()
				{
					gl_Position = new Vec4 (v.position, 1f),
					fragPosition = v.position,
					fragNormal = v.normal,
					fragDiffuse = v.diffuse,
				});
		}

		private static GLShader FragmentShader ()
		{
			return GLShader.Create
			(
				ShaderType.FragmentShader, () =>

				from f in Shader.Inputs<MaterialFragment> ()
				select new
				{
					outputColor = f.fragDiffuse
				}
			);
		}
	}
}