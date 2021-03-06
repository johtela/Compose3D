﻿namespace Compose3D.Renderers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;
	using Compiler;
	using Maths;
	using Geometry;
	using GLTypes;
	using Reactive;
	using SceneGraph;
	using Shaders;
	using Textures;
	using OpenTK.Graphics.OpenGL4;

	[StructLayout (LayoutKind.Sequential, Pack = 4)]
	public struct MaterialVertex : IVertex3D, ITextured, INormalMapped
	{
		public Vec3 position;
		public Vec2 texturePos;
		public Vec3 normal;
		public Vec3 tangent;

		Vec3 IVertex<Vec3>.position
		{
			get { return position; }
			set { position = value; }
		}

		Vec2 ITextured.texturePos
		{
			get { return texturePos; }
			set { texturePos = value; }
		}

		Vec3 IVertex3D.normal
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
			set
			{
				if (value.IsNaN ())
					throw new ArgumentException ("Tangent component NaN");
				tangent = value;
			}
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
		public Uniform<Sampler2D> heightMap;

		public Materials (GLProgram program)
			: base (program)
		{
			using (program.Scope ())
			{
				transforms = new TransformUniforms (program);
				diffuseMap &= new Sampler2D (0).LinearFiltering ();
				normalMap &= new Sampler2D (1).LinearFiltering ();
				heightMap &= new Sampler2D (2).LinearFiltering ();
			}
		}

		private static Materials _materials;
		private static GLProgram _materialShader;

		public static Reaction<Camera> Renderer (Texture diffuseMap, Texture normalMap, Texture heightMap)
		{
			_materialShader = new GLProgram (
				VertexShader (), 
				FragmentShader ());
			_materials = new Materials (_materialShader);

			return React.By<Camera> (_materials.Render)
				.BindSamplers (new Dictionary<Sampler, Texture> ()
				{
					{ !_materials.diffuseMap, diffuseMap },
					{ !_materials.normalMap, normalMap },
					{ !_materials.heightMap, heightMap }
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
					tangentViewDir = (TBN * new Vec3 (0f, 0f, 1f)).Normalized,
					tangentLightDir = (TBN * new Vec3 (1f, 0f, 1f)).Normalized
				});
		}

		public static readonly Func<Sampler2D, Vec2, Vec3, float, Vec2>
			ParallaxMapping = GLShader.Function
			(
				() => ParallaxMapping,
				(heightMap, texCoords, viewDir, scale) => Shader.Evaluate
				(
					from con in Shader.Constants (new
					{
						numLayers = 5
					})
					let layerDepth = 1f / con.numLayers
					let p = (viewDir[Coord.x, Coord.y] * scale) / con.numLayers
					let layer = Control<int>.DoWhileSame (0, con.numLayers + 1, -1,
						(int i, int best) => Shader.Evaluate
						(
							from height in heightMap.Texture (texCoords - (p * i)).X.ToShader ()
							let depth = 1f - height
							let currLayerDepth = layerDepth * i
							select currLayerDepth >= depth ? i : best
						)
					)
					let currTexCoords = texCoords - (p * layer)
					let prevTexCoords = currTexCoords + p
					let currDepth = 1f - heightMap.Texture (currTexCoords).X
					let prevDepth = 1f - heightMap.Texture (prevTexCoords).X
					let currDist = (currDepth - (layer * layerDepth)).Abs ()
					let prevDist = (prevDepth - ((layer - 1) * layerDepth)).Abs ()
					let weight = currDist / (currDist + prevDist)
					select currTexCoords.Mix (prevTexCoords, weight)
				)
			);

		public static GLShader FragmentShader ()
		{
			return GLShader.Create
			(
				ShaderType.FragmentShader, () =>

				from f in Shader.Inputs<MaterialFragment> ()
				from u in Shader.Uniforms<Materials> ()
				let texCoords = ParallaxMapping (!u.heightMap, f.texPosition, f.tangentViewDir, 0.01f)
				let diffuse = (!u.diffuseMap).Texture (texCoords)[Coord.x, Coord.y, Coord.z]
				let normal = (!u.normalMap).Texture (texCoords)[Coord.x, Coord.y, Coord.z] * 2f - new Vec3 (1f)
				select new
				{
					outputColor = (normal.Dot (f.tangentLightDir).Clamp (0f, 1f) * diffuse).Clamp (0f, 1f)
				}
			);
		}
	}
}