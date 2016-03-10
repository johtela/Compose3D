﻿namespace ComposeTester
{
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.GLTypes;
	using Compose3D.SceneGraph;
	using Compose3D.Shaders;
	using OpenTK.Graphics.OpenGL;
	using System;
	using System.Linq;
	using System.Runtime.InteropServices;
	using LinqCheck;
	using Extensions;

	[StructLayout (LayoutKind.Sequential)]
	public struct TerrainVertex : IVertex
	{
		internal Vec3 position;
		internal Vec3 normal;
		[OmitInGlsl]
		internal int tag;

		Vec3 IPositional<Vec3>.Position
		{
			get { return position; }
			set { position = value; }
		}

		Vec3 IPlanar<Vec3>.Normal
		{
			get { return normal; }
			set
			{
				if (float.IsNaN (value.X) || float.IsNaN (value.Y) || float.IsNaN (value.Z))
					throw new ArgumentException ("Normal component NaN");
				normal = value;
			}
		}

		int IVertex.Tag
		{
			get { return tag; }
			set { tag = value; }
		}

		public override string ToString ()
		{
			return string.Format ("[Vertex: Position={0}, Normal={3}, Tag={4}]",
				position, normal, tag);
		}
	}

	public class Terrain
	{
		public class TerrainFragment : DiffuseFragment
		{
			public float visibility;
		}

		public class TerrainUniforms : BasicUniforms
		{
			public Uniform<Vec3> skyColor;

			public void Initialize (Program program, SceneGraph scene, Vec3 skyCol)
			{
				base.Initialize (program, scene);
				using (program.Scope ())
					skyColor &= skyCol;
			}
		}
		
		public readonly Program TerrainShader;
		public readonly TerrainUniforms Uniforms;

		public Terrain ()
		{
			TerrainShader = new Program (VertexShader (), FragmentShader ());
			TerrainShader.InitializeUniforms (Uniforms = new TerrainUniforms ());
		}

		public SceneNode CreateScene (SceneGraph sceneGraph)
		{
			return new SceneGroup (sceneGraph,
				from x in EnumerableExt.Range (0, 5000, 58)
				from y in EnumerableExt.Range (0, 5000, 58)
				select new TerrainMesh<TerrainVertex> (sceneGraph, new Vec2i (x, y), new Vec2i (64, 64),
					20f, 0.039999f, 3, 5f, 4f))
				.OffsetOrientAndScale (new Vec3 (-5000f, -10f, -5000f), new Vec3 (0f), new Vec3 (2f));
		}

		public void Render (Camera camera)
		{
			GL.Enable (EnableCap.CullFace);
			GL.CullFace (CullFaceMode.Back);
			GL.FrontFace (FrontFaceDirection.Cw);
			GL.Enable (EnableCap.DepthTest);
			GL.DepthMask (true);
			GL.DepthFunc (DepthFunction.Less);

			var worldToCamera = camera.WorldToCamera;
			using (TerrainShader.Scope ())
				foreach (var mesh in camera.NodesInView<TerrainMesh<TerrainVertex>> ())
				{
					if (mesh.VertexBuffer != null && mesh.IndexBuffers != null)
					{
						Uniforms.worldMatrix &= worldToCamera * mesh.Transform;
						Uniforms.normalMatrix &= new Mat3 (mesh.Transform).Inverse.Transposed;
						var distance = -(worldToCamera * mesh.BoundingBox).Front;
						var lod = distance < 150 ? 0 :
								  distance < 250 ? 1 :
								  2;
						TerrainShader.DrawElements (PrimitiveType.TriangleStrip, mesh.VertexBuffer, 
							mesh.IndexBuffers[lod]);
					}
				}
		}

		public void UpdateViewMatrix (Mat4 matrix)
		{
			using (TerrainShader.Scope ())
				Uniforms.perspectiveMatrix &= matrix;
		}

		public static GLShader VertexShader ()
		{
			Lighting.Use ();
			return GLShader.Create
			(
				ShaderType.VertexShader, () =>

				from v in Shader.Inputs<TerrainVertex> ()
				from u in Shader.Uniforms<TerrainUniforms> ()
				let viewPos = !u.worldMatrix * new Vec4 (v.position, 1f)
				let vertPos = viewPos[Coord.x, Coord.y, Coord.z]
				let height = v.position.Y
				let slope = v.normal.Dot (new Vec3 (0f, 1f, 0f))
				let blend = GLMath.SmoothStep (0.9f, 0.99f, slope)
				let rockColor = new Vec3 (0.3f, 0.4f, 0.5f)
				let grassColor = new Vec3 (0.3f, 1f, 0.2f)
				let snowColor = new Vec3 (1f, 1f, 1f)
				select new TerrainFragment ()
				{
					gl_Position = !u.perspectiveMatrix * viewPos,
					vertexPosition = vertPos,
					vertexNormal = (!u.normalMatrix * v.normal).Normalized,
					vertexDiffuse = height < -2f ? 
							rockColor.Mix (grassColor, blend) : 
							rockColor.Mix (snowColor, blend),
					visibility = Lighting.FogVisibility (vertPos.Z, 0.004f, 3f)
				}
			);
		}

		public static GLShader FragmentShader ()
		{
			return GLShader.Create
			(
				ShaderType.FragmentShader, () =>

				from f in Shader.Inputs<TerrainFragment> ()
				from u in Shader.Uniforms<TerrainUniforms> ()
				let diffuse = Lighting.DirectionalLightIntensity (!u.directionalLight, f.vertexNormal) * f.vertexDiffuse
				select new
				{
					outputColor = Lighting.GlobalLightIntensity (!u.globalLighting, diffuse * 3f, new Vec3 (0f))
						.Mix (!u.skyColor, f.visibility)
				}
			);
		}
	}
}