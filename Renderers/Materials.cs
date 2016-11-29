namespace Compose3D.Renderers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;
	using Maths;
	using Geometry;
	using GLTypes;
	using Reactive;
	using SceneGraph;
	using Shaders;
	using Textures;
	using OpenTK.Graphics.OpenGL4;

	[StructLayout (LayoutKind.Sequential, Pack = 4)]
	public struct MaterialVertex : IVertex, ITextured, INormalMapped
	{
		public Vec3 position;
		public Vec3 normal;
		public Vec2 texturePos;
		public Vec3 tangent;

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
					throw new ArgumentException ("Normal component NaN");
				normal = value;
			}
		}

		Vec3 INormalMapped.tangent
		{
			get { return tangent; }
			set { tangent = value; }
		}

		public override string ToString ()
		{
			return string.Format ("[Vertex: position={0}, normal={1}, texturePos={2}, tangent={3}]",
				position, normal, texturePos, tangent);
		}
	}
	
	public class MaterialFragment : Fragment
	{
		public Vec2 texPosition { get; set; }
		public Vec3 tangentLightDir { get; set; }
		public Vec3 tangentViewDir { get; set; }
	}

	public class Materials : Uniforms
	{
		public TransformUniforms transforms;
		public Uniform<Sampler2D> diffuseMap;
		public Uniform<Sampler2D> normalMap;

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

		public static Reaction<Camera> Renderer (Texture diffuseMap, Texture normalMap)
		{
			_materialShader = new Program (
				VertexShader (), 
				FragmentShader ());
			_materials = new Materials (_materialShader);

			return React.By<Camera> (_materials.Render)
				.BindSamplers (new Dictionary<Sampler, Texture> ()
				{
					{ !_materials.diffuseMap, diffuseMap },
					{ !_materials.normalMap, normalMap }
				})
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
				//_materialShader.DrawNormals (mesh.NormalBuffer);
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
				let tangent = (!t.normalMatrix * v.tangent).Normalized
				let normal = (!t.normalMatrix * v.normal).Normalized
				let bitangent = tangent.Cross (normal)
				let TBN = new Mat3 (tangent, bitangent, normal).Transposed
				select new MaterialFragment ()
				{
					gl_Position = !t.perspectiveMatrix * viewPos,
					texPosition = v.texturePos,
					tangentViewDir = TBN * new Vec3 (0f),
					tangentLightDir = TBN * new Vec3 (0f)
				});
		}

		private static GLShader FragmentShader ()
		{
			return GLShader.Create
			(
				ShaderType.FragmentShader, () =>

				from f in Shader.Inputs<MaterialFragment> ()
				from u in Shader.Uniforms<Materials> ()
				let diffuse = (!u.diffuseMap).Texture (f.texPosition)[Coord.x, Coord.y, Coord.z]
				let normal = (!u.normalMap).Texture (f.texPosition)[Coord.x, Coord.y, Coord.z]
				select new
				{
					outputColor = normal.Dot (f.tangentLightDir) * diffuse
				}
			);
		}
	}
}