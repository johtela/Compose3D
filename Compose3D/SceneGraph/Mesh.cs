namespace Compose3D.SceneGraph
{
    using Compose3D.Maths;
    using Geometry;
    using GLTypes;
    using OpenTK.Graphics.OpenGL;
    using Textures;

    public class Mesh<V> : SceneNode 
        where V : struct, IVertex
    {
        private VBO<V> _vertexBuffer;
        private VBO<int> _indexBuffer;
        private VBO<V> _normalBuffer;

        public Mesh (Geometry<V> geometry, params Texture[] textures)
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
                    _normalBuffer = new VBO<V> (Geometry.Normals, BufferTarget.ArrayBuffer);
                return _normalBuffer;
            }
        }

        public BBox BoundingBox (Mat4 matrix)
        {
            return matrix * Geometry.BoundingBox;
        }
    }
}
