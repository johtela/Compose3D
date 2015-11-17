namespace Compose3D.SceneGraph
{
    using OpenTK.Graphics.OpenGL;
	using Maths;
	using Geometry;
	using GLTypes;

	public class LineSegment<P, V> : SceneNode
		where P : struct, IPositional<V>
		where V : struct, IVec<V, float>
	{
		private VBO<P> _vertexBuffer;

		public LineSegment (Path<P, V> path)
		{
			Path = path;
		}

		public Path<P, V> Path { private get; set; }

		public VBO<P> VertexBuffer
		{
			get
			{
				if (_vertexBuffer == null)
					_vertexBuffer = new VBO<P> (Path.Nodes, BufferTarget.ArrayBuffer);
				return _vertexBuffer;
			}
		}
	}
}
