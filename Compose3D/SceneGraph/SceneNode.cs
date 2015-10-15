namespace Compose3D.SceneGraph
{
    using Arithmetics;
    using Geometry;

    public abstract class SceneNode
    {
        public SceneNode (Mat4 modelMatrix)
        {
            ModelMatrix = modelMatrix;
        }

        public Mat4 ModelMatrix { get; set; }

        public abstract BBox BoundingBox (Mat4 matrix);
    }
}
