﻿namespace Compose3D.Renderers
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
	public struct PathNode : IVertex<Vec3>, IDiffuseColor<Vec3>
	{
		public Vec3 position;
		public Vec3 diffuse;

		Vec3 IVertex<Vec3>.position
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

	public static class LineSegments
	{
		private static GLProgram _shader;

		public static Reaction<Camera> Renderer (SceneGraph sceneGraph)
		{
			_shader = PassThrough;

			return React.By<Camera> (Render)
				.Program (_shader);
		}

		private static void Render (Camera camera)
		{
			GL.ClearColor (0f, 0f, 0f, 1f);
			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			foreach (var ls in camera.Graph.Root.Traverse ().OfType<LineSegment<PathNode, Vec3>> ())
				_shader.DrawLinePath (ls.VertexBuffer);
		}

		public static GLProgram PassThrough = new GLProgram (
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