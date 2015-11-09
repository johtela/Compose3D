namespace Compose3D.SceneGraph
{
    using OpenTK.Graphics.OpenGL;
	using Maths;
	using Geometry;
	using GLTypes;

	public class LineSegment<P> : SceneNode
		where P : struct, IPositional<Vec3>
	{
		private VBO<P> _vertexBuffer;

		public LineSegment (Path<P, Vec3> path)
		{
			Path = path;
		}

		public Path<P, Vec3> Path { private get; set; }

		public VBO<P> VertexBuffer
		{
			get
			{
				if (_vertexBuffer == null)
					_vertexBuffer = new VBO<P> (Path.Vertices, BufferTarget.ArrayBuffer);
				return _vertexBuffer;
			}
		}
	}
}
