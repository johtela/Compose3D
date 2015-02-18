namespace Compose3D.Geometry
{
    using System;
    using GLSL;

    /// <summary>
    /// Enumeration that describes the alignment between two bounding box.
    /// </summary>
    public enum Align
    {
        None,		/// No alignment
        Negative,	/// Align along faces with the smaller coordinates.
        Center, 	/// Align along the center of the bounding boxes.
        Positive	/// Align along the faces with greater coordinates.
    }

    /// <summary>
    /// Class representing the bounding box of a geometry.
    /// </summary>
    public class BBox
    {
        /// <summary>
        /// The minimum X, Y, and Z coordinates of all 8 corners of the bounding box.
        /// </summary>
        /// <description>
        /// Effectively the coordinate back-left-bottom corner since the axis grow
        /// from back to front, left to right, and bottom to top.
        /// </description>
        public readonly Vec3 Position;

        /// <summary>
        /// The size of the bounding box along X, Y, and Z axis.
        /// </summary>
        /// <description>
        /// The components of the size vector are always positive.
        /// </description>
        public readonly Vec3 Size;

        public BBox (Vec3 position, Vec3 size)
        {
            Position = position;
            Size = size;
        }

        public BBox (Vec3 position)
        {
            Position = position;
        }

        /// <summary>
        /// The X-coordinate of the left face of the bounding box.
        /// </summary>
        public float Left
        {
            get { return Position.X; }
        }

        /// <summary>
        /// The X-coordinate of the right face of the bounding box.
        /// </summary>
        public float Right
        {
            get { return Position.X + Size.X; }
        }

        /// <summary>
        /// The Y-coordinate of the bottom face of the bounding box.
        /// </summary>
        public float Bottom
        {
            get { return Position.Y; }
        }

        /// <summary>
        /// The Y-coordinate of the top face of the bounding box.
        /// </summary>
        public float Top
        {
            get { return Position.Y + Size.Y; }
        }

        /// <summary>
        /// The Z-coordinate of the back face of the bounding box.
        /// </summary>
        public float Back
        {
            get { return Position.Z; }
        }

        /// <summary>
        /// The Z-coordinate of the front face of the bounding box.
        /// </summary>
        public float Front
        {
            get { return Position.Z + Size.Z; }
        }

        public Vec3 Center
        {
            get
            {
                return new Vec3 (Position.X + (Size.X / 2.0f),
                                 Position.Y + (Size.Y / 2.0f),
                                 Position.Z + (Size.Z / 2.0f));
            }
        }

        /// <summary>
        /// Gets the X offset of a bounding box when aligned with the current one along X axis.
        /// </summary>
        public float GetXOffset (BBox other, Align align)
        {
            switch (align)
            {
                case Align.Negative: return Left - other.Left;
                case Align.Positive: return Right - other.Right;
                case Align.Center: return Center.X - other.Center.X;
                default: return 0f;
            }
        }

        /// <summary>
        /// Gets the Y offset of a bounding box when aligned with the current one along Y axis.
        /// </summary>
        public float GetYOffset (BBox other, Align align)
        {
            switch (align)
            {
                case Align.Negative: return Bottom - other.Bottom;
                case Align.Positive: return Top - other.Top;
                case Align.Center: return Center.Y - other.Center.Y;
                default: return 0f;
            }
        }

        /// <summary>
        /// Gets the Y offset of a bounding box when aligned with the current one along Y axis.
        /// </summary>
        public float GetZOffset (BBox other, Align align)
        {
            switch (align)
            {
                case Align.Negative: return Back - other.Back;
                case Align.Positive: return Front - other.Front;
                case Align.Center: return Center.Z - other.Center.Z;
                default: return 0.0f;
            }
        }

        public static BBox operator + (BBox bbox, Vec3 vertex)
        {
            var pos = new Vec3 (Math.Min (bbox.Position.X, vertex.X),
                                Math.Min (bbox.Position.Y, vertex.Y),
                                Math.Min (bbox.Position.Z, vertex.Z));
            var size = new Vec3 (Math.Max (bbox.Size.X, vertex.X - pos.X),
                                 Math.Max (bbox.Size.Y, vertex.Y - pos.Y),
                                 Math.Max (bbox.Size.Z, vertex.Z - pos.Z));
            return new BBox (pos, size);
        }

        public static BBox operator + (BBox bbox, BBox other)
        {
            var pos = new Vec3 (Math.Min (bbox.Left, other.Left),
                                Math.Min (bbox.Bottom, other.Bottom),
                                Math.Min (bbox.Back, other.Back));
            var size = new Vec3 (Math.Max (bbox.Right, other.Right) - pos.X,
                                 Math.Max (bbox.Top, other.Top) - pos.Y,
                                 Math.Max (bbox.Front, other.Front) - pos.Z);
            return new BBox (pos, size);
        }
    }
}