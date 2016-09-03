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

	public class Materials : Uniforms
	{
		public TransformUniforms transforms;

		public Materials (Program program)
			: base (program)
		{
			using (program.Scope ())
			{
				transforms = new TransformUniforms (program);
			}
		}

		private static Materials _materials;
		private static Program _materialShader;

		public static Reaction<Camera> Renderer ()
		{
			_materialShader = new Program (
				VertexShader (), 
				FragmentShader ());
			_materials = new Materials (_materialShader);

			return React.By<Camera> (_materials.Render)
				.DepthTest ()
				.Culling ()
				.Program (_materialShader);
		}

		private void Render (Camera camera)
		{
			foreach (var mesh in camera.NodesInView<Mesh<MaterialVertex>> ())
			{
				transforms.UpdateModelViewAndNormalMatrices (camera.WorldToCamera * mesh.Transform);
				_materialShader.DrawElements (PrimitiveType.Triangles, mesh.VertexBuffer, mesh.IndexBuffer);
				_materialShader.DrawNormals (mesh.NormalBuffer);
			}
		}

		public static Reaction<Mat4> UpdatePerspectiveMatrix ()
		{
			return React.By<Mat4> (matrix => _materials.transforms.perspectiveMatrix &= matrix)
				.Program (_materialShader);
		}

		public static GLShader VertexShader ()
		{
			return GLShader.Create (ShaderType.VertexShader, () =>
				from v in Shader.Inputs<MaterialVertex> ()
				from t in Shader.Uniforms<TransformUniforms> ()
				let viewPos = !t.modelViewMatrix * new Vec4 (v.position, 1f)
				select new MaterialFragment ()
				{
					gl_Position = !t.perspectiveMatrix * viewPos,
					fragPosition = viewPos[Coord.x, Coord.y, Coord.z],
					fragNormal = (!t.normalMatrix * v.normal).Normalized,
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
					outputColor = f.fragNormal.Dot (new Vec3 (0f, 0f, 1f)) * f.fragDiffuse
				}
			);
		}
	}
}