namespace ComposeFX.Graphics.SceneGraph
{
	using System.Linq;
	using OpenTK.Graphics.OpenGL4;
	using Maths;
	using Geometry;
	using GLTypes;
	using DataStructures;

	public class LineSegment<P, V> : SceneNode
		where P : struct, IVertex<V>
		where V : struct, IVec<V, float>
	{
		private VBO<P> _vertexBuffer;

		public LineSegment (SceneGraph graph, Path<P, V> path) : base (graph)
		{
			Path = path;
		} 

		public Path<P, V> Path { private get; set; }

		public override Aabb<Vec3> BoundingBox
		{
			get
			{
				return Aabb<Vec3>.FromPositions (Path.Vertices.Select (node =>	
					node.position.Convert<V, Vec3, float> ()));
			}
		}

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
