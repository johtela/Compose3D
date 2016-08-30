namespace ComposeTester
{
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.GLTypes;
	using Compose3D.Reactive;
	using Compose3D.SceneGraph;
	using Compose3D.Shaders;
	using OpenTK.Graphics.OpenGL4;
	using System.Linq;
	using System.Runtime.InteropServices;

	[StructLayout (LayoutKind.Sequential)]
	public struct PathNode : IPositional<Vec3>, IDiffuseColor<Vec3>
	{
		public Vec3 position;
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

		public override string ToString ()
		{
			return string.Format ("PathNode: position={0}, diffuse={1}", position, diffuse);
		}
	}

	public class DiffuseFragment : Fragment, IFragmentDiffuse
	{
		public Vec3 fragDiffuse { get; set; }
	}

	public class MaterialRenderer : Uniforms
	{
		public LightingUniforms lighting;
		public TransformUniforms transforms;

		public MaterialRenderer (Program program, SceneGraph scene)
			: base (program)
		{
			using (program.Scope ())
			{
				lighting = new LightingUniforms (program, scene);
				transforms = new TransformUniforms (program);
			}
		}

		private static Program _shader;
		private static MaterialRenderer _renderer;

		public static Reaction<Camera> Renderer (SceneGraph sceneGraph)
		{
			_shader = PassThrough;
			_renderer = new MaterialRenderer (_shader, sceneGraph);

			return React.By<Camera> (_renderer.Render)
				.Program (_shader);
		}

		private void Render (Camera camera)
		{
			lighting.UpdateDirectionalLight (camera);

			foreach (var ls in camera.Graph.Root.Traverse ().OfType<LineSegment<PathNode, Vec3>> ())
			{
				transforms.UpdateModelViewAndNormalMatrices (camera.WorldToCamera * ls.Transform);
				_shader.DrawLinePath (ls.VertexBuffer);
			}
		}

		public static Reaction<Mat4> UpdatePerspectiveMatrix ()
		{
			return React.By<Mat4> (matrix => _renderer.transforms.perspectiveMatrix &= matrix)
				.Program (_shader);
		}

		public static Program PassThrough = new Program (
			GLShader.Create (ShaderType.VertexShader,
				() =>
				from v in Shader.Inputs<PathNode> ()
				select new DiffuseFragment ()
				{
					gl_Position = new Vec4 (v.position.X, v.position.Y, -1f, 1f),
					fragDiffuse = v.diffuse
				}
			),
			GLShader.Create (ShaderType.FragmentShader,
				() =>
				from f in Shader.Inputs<DiffuseFragment> ()
				select new
				{
					outputColor = f.fragDiffuse
				}
			)
		);
	}
}
