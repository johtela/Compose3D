namespace ComposeTester
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
	public struct TerrainVertex : IVertex, IDiffuseColor<Vec3>
	{
		internal Vec3 position;
		internal Vec3 normal;
		internal Vec3 diffuseColor;
		[OmitInGlsl]
		internal int tag;

		Vec3 IPositional<Vec3>.Position
		{
			get { return position; }
			set { position = value; }
		}

		Vec3 IDiffuseColor<Vec3>.Diffuse
		{
			get { return diffuseColor; }
			set { diffuseColor = value; }
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
			return string.Format ("[Vertex: Position={0}, DiffuseColor={1}, Normal={3}, Tag={4}]",
				position, diffuseColor, normal, tag);
		}
	}

	public class Terrain
	{
		public readonly Program TerrainShader;
		public readonly BasicUniforms Uniforms;

		public Terrain ()
		{
			TerrainShader = new Program (VertexShader (), FragmentShader ());
			TerrainShader.InitializeUniforms (Uniforms = new BasicUniforms ());
		}

		public SceneNode CreateScene (SceneGraph sceneGraph)
		{
			return new SceneGroup (sceneGraph,
				from x in EnumerableExt.Range (0, 5000, 20)
				from y in EnumerableExt.Range (0, 5000, 20)
				select new TerrainMesh<TerrainVertex> (sceneGraph, new Vec2i (x, y), new Vec2i (21, 21)))
				.OffsetOrientAndScale (new Vec3 (-2500f, -10f, -2500f), new Vec3 (0f), new Vec3 (2f));
		}

		public void Render (SceneGraph scene, Camera camera)
		{
			GL.Enable (EnableCap.CullFace);
			GL.CullFace (CullFaceMode.Back);
			GL.FrontFace (FrontFaceDirection.Cw);
			GL.Enable (EnableCap.DepthTest);
			GL.DepthMask (true);
			GL.DepthFunc (DepthFunction.Less);
			using (TerrainShader.Scope ())
				foreach (var mesh in scene.Index.Overlap (camera.BoundingBox).Values ()
					.OfType<TerrainMesh<TerrainVertex>> ())
				{
					Uniforms.worldMatrix &= camera.WorldToCamera * mesh.Transform;
					Uniforms.normalMatrix &= new Mat3 (mesh.Transform).Inverse.Transposed;
					TerrainShader.DrawElements (PrimitiveType.TriangleStrip, mesh.VertexBuffer, mesh.IndexBuffer);
				}
		}

		public void UpdateViewMatrix (Mat4 matrix)
		{
			using (TerrainShader.Scope ())
				Uniforms.perspectiveMatrix &= matrix;
		}

		public static GLShader VertexShader ()
		{
			return GLShader.Create
			(
				ShaderType.VertexShader, () =>

				from v in Shader.Inputs<TerrainVertex> ()
				from u in Shader.Uniforms<BasicUniforms> ()
				let worldPos = !u.worldMatrix * new Vec4 (v.position, 1f)
				select new DiffuseFragment ()
				{
					gl_Position = !u.perspectiveMatrix * worldPos,
					vertexPosition = worldPos[Coord.x, Coord.y, Coord.z],
					vertexNormal = (!u.normalMatrix * v.normal).Normalized,
					vertexDiffuse = v.diffuseColor,
				}
			);
		}

		public static GLShader FragmentShader ()
		{
			Lighting.Use ();
			return GLShader.Create
			(
				ShaderType.FragmentShader, () =>

				from f in Shader.Inputs<DiffuseFragment> ()
				from u in Shader.Uniforms<BasicUniforms> ()
				let diffuse = Lighting.DirectionalLightIntensity (!u.directionalLight, f.vertexNormal) * f.vertexDiffuse
				select new
				{
					outputColor = Lighting.GlobalLightIntensity (!u.globalLighting, diffuse * 3f, new Vec3 (0f))
				}
			);
		}
	}
}