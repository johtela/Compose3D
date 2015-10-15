namespace Compose3D.SceneGraph
{
    using Arithmetics;
    using Geometry;
    using GLTypes;
    using Textures;
    using System;
    using OpenTK.Graphics.OpenGL;

    public class Mesh<V> : SceneNode 
        where V : struct, IVertex
    {
        private VBO<V> _vertexBuffer;
        private VBO<int> _indexBuffer;
        private VBO<V> _normalBuffer;

        public Mesh (Mat4 modelMatrix, Geometry<V> geometry, params Texture[] textures)
            : base (modelMatrix)
        {
            Geometry = geometry;
            Textures = textures;
        }

        public Mesh (Geometry<V> geometry)
            : this (new Mat4 (1f), geometry) { }

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

        public override BBox BoundingBox (Mat4 matrix)
        {
            return matrix * Geometry.BoundingBox;
        }
    }
}
