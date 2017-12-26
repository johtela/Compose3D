namespace ComposeFX.Graphics.SceneGraph
{
	using System.Linq;
    using Maths;
    using Geometry;
    using GLTypes;
    using OpenTK.Graphics.OpenGL4;
	using DataStructures;
    using Textures;

    public class Mesh<V> : SceneNode 
        where V : struct, IVertex3D
    {
        private VBO<V> _vertexBuffer;
        private VBO<int> _indexBuffer;
        private VBO<V> _normalBuffer;

        public Mesh (SceneGraph graph, Geometry<V> geometry, params Texture[] textures)
			: base (graph)
        {
            Geometry = geometry;
            Textures = textures;
        }

        public Geometry<V> Geometry { get; private set; }

        public Texture[] Textures { get; set; }

        public VBO<V> VertexBuffer
        {
            get
            {
                if (_vertexBuffer == null)
                    _vertexBuffer = new VBO<V> (Geometry.Vertices, BufferTarget.ArrayBuffer);
                return _vertexBuffer;
            }
        }

        public VBO<int> IndexBuffer
        {
            get
            {
                if (_indexBuffer == null)
                    _indexBuffer = new VBO<int> (Geometry.Indices, BufferTarget.ElementArrayBuffer);
                return _indexBuffer;
            }
        }

        public VBO<V> NormalBuffer
        {
            get
            {
                if (_normalBuffer == null)
                    _normalBuffer = new VBO<V> (Geometry.Normals ().ToArray (), BufferTarget.ArrayBuffer);
                return _normalBuffer;
            }
        }

		public override Aabb<Vec3> BoundingBox
        {
            get { return Transform * Geometry.BoundingBox; }
        }
    }
}
