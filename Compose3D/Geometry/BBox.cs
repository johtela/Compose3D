namespace Compose3D.Geometry
{
    using System;
    using Arithmetics;

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
		public readonly Vec3 Min;

		/// <summary>
		/// The maximum X, Y, and Z coordinates of all 8 corners of the bounding box.
		/// </summary>
		/// <description>
		/// Effectively the coordinate front-right-top corner since the axis grow
		/// from back to front, left to right, and bottom to top.
		/// </description>
		public readonly Vec3 Max;

		public BBox (Vec3 min, Vec3 max)
        {
			Min = min;
			Max = max;
        }

        public BBox (Vec3 position)
        {
			Min = position;
			Max = position;
        }

        /// <summary>
        /// The X-coordinate of the left face of the bounding box.
        /// </summary>
        public float Left
        {
			get { return Min.X; }
        }

        /// <summary>
        /// The X-coordinate of the right face of the bounding box.
        /// </summary>
        public float Right
        {
			get { return Max.X; }
        }

        /// <summary>
        /// The Y-coordinate of the bottom face of the bounding box.
        /// </summary>
        public float Bottom
        {
			get { return Min.Y; }
        }

        /// <summary>
        /// The Y-coordinate of the top face of the bounding box.
        /// </summary>
        public float Top
        {
			get { return Max.Y; }
        }

        /// <summary>
        /// The Z-coordinate of the back face of the bounding box.
        /// </summary>
        public float Back
        {
			get { return Min.Z; }
        }

        /// <summary>
        /// The Z-coordinate of the front face of the bounding box.
        /// </summary>
        public float Front
        {
			get { return Max.Z; }
        }

		public Vec3 Size
		{
			get { return Max - Min; }
		}

        public Vec3 Center
        {
			get { return (Min + Max) / 2; }
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
			var min = new Vec3 (
				Math.Min (bbox.Min.X, vertex.X), 
				Math.Min (bbox.Min.Y, vertex.Y), 
				Math.Min (bbox.Min.Z, vertex.Z));
			var max = new Vec3 (
				Math.Max (bbox.Max.X, vertex.X), 
				Math.Max (bbox.Max.Y, vertex.Y), 
				Math.Max (bbox.Max.Z, vertex.Z));
			return new BBox (min, max);
        }

        public static BBox operator + (BBox bbox, BBox other)
        {
			var min = new Vec3 (
				Math.Min (bbox.Min.X, other.Min.X),
				Math.Min (bbox.Min.Y, other.Min.Y),
				Math.Min (bbox.Min.Z, other.Min.Z));
			var max = new Vec3 (
				Math.Max (bbox.Max.X, other.Max.X),
				Math.Max (bbox.Max.Y, other.Max.Y),
				Math.Max (bbox.Max.Z, other.Max.Z));
			return new BBox (min, max);
        }
    }
}