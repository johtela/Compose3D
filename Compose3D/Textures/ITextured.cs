namespace Compose3D.Textures
{
    using Arithmetics;
    using Geometry;
    using System.Collections.Generic;
    using System.Linq;

    public interface ITextured
    {
        /// <summary>
        /// The texture coordinate of the vertex.
        /// </summary>
        Vec2 TexturePos { get; set; }
    }

    public static class TexturePos
    {
        public static Vec2 BottomLeft
        {
            get { return new Vec2 (0f, 0f); }
        }

        public static Vec2 BottomRight
        {
            get { return new Vec2 (1f, 0f); }
        }

        public static Vec2 TopLeft
        {
            get { return new Vec2 (0f, 1f); }
        }

        public static Vec2 TopRight
        {
            get { return new Vec2 (1f, 1f); }
        }
    }

    public static class TextureHelpers
    {
        public static void ApplyTexture<V> (this Geometry<V> geometry, Mat4 projection, Vec2 minPos, Vec2 maxPos)
            where V : struct, IVertex, ITextured
        {
            var projected = geometry.Transform (projection);
            var range = maxPos - minPos;
            var bbox = projected.BoundingBox;
            var scaleX = range.X / bbox.Size.X;
            var scaleY = range.Y / bbox.Size.Y;
            var verts = geometry.Vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                var pv = projected.Vertices[i];
                if (pv.Normal.Z >= 0f)
                    verts[i].TexturePos = new Vec2 (
                        (pv.Position.X - bbox.Left) * scaleX + minPos.X,
                        (pv.Position.Y - bbox.Bottom) * scaleY + minPos.Y);
            }
        }
    }
}
