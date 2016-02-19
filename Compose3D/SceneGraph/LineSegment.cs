namespace Compose3D.SceneGraph
{
	using System.Linq;
	using OpenTK.Graphics.OpenGL;
	using Maths;
	using Geometry;
	using GLTypes;
	using DataStructures;

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

		public override Aabb<Vec3> BoundingBox
		{
			get
			{
				return Aabb<Vec3>.FromPositions (Path.Nodes.Select (node =>	
					node.Position.Convert<V, Vec3, float> ()));
			}
		}

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
