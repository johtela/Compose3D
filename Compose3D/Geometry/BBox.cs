namespace Compose3D.Geometry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Compose3D.Maths;

    /// <summary>
    /// Enumeration that describes the alignment between two bounding box.
    /// </summary>
    public enum Alignment
    {
		/// No alignment
        None,
		/// Align along faces with the smaller coordinates.
        Negative,
		/// Align along the center of the bounding boxes.
        Center,
		/// Align along the faces with greater coordinates.
        Positive
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

        public IEnumerable<Vec3> Corners
        {
            get
            {
                yield return new Vec3 (Left, Bottom, Front);
                yield return new Vec3 (Left, Top, Front);
                yield return new Vec3 (Right, Top, Front);
                yield return new Vec3 (Right, Bottom, Front);
                yield return new Vec3 (Left, Bottom, Back);
                yield return new Vec3 (Left, Top, Back);
                yield return new Vec3 (Right, Top, Back);
                yield return new Vec3 (Right, Bottom, Back);
            }
        }

        /// <summary>
        /// Gets the X offset of a bounding box when aligned with the current one along X axis.
        /// </summary>
        public float GetXOffset (BBox other, Alignment align)
        {
            switch (align)
            {
                case Alignment.Negative: return Left - other.Left;
                case Alignment.Positive: return Right - other.Right;
                case Alignment.Center: return Center.X - other.Center.X;
                default: return 0f;
            }
        }

        /// <summary>
        /// Gets the Y offset of a bounding box when aligned with the current one along Y axis.
        /// </summary>
        public float GetYOffset (BBox other, Alignment align)
        {
            switch (align)
            {
                case Alignment.Negative: return Bottom - other.Bottom;
                case Alignment.Positive: return Top - other.Top;
                case Alignment.Center: return Center.Y - other.Center.Y;
                default: return 0f;
            }
        }

        /// <summary>
        /// Gets the Y offset of a bounding box when aligned with the current one along Y axis.
        /// </summary>
        public float GetZOffset (BBox other, Alignment align)
        {
            switch (align)
            {
                case Alignment.Negative: return Back - other.Back;
                case Alignment.Positive: return Front - other.Front;
                case Alignment.Center: return Center.Z - other.Center.Z;
                default: return 0.0f;
            }
        }

        public static BBox operator + (BBox bbox, Vec3 vertex)
        {
			return new BBox (bbox.Min.Min (vertex), bbox.Max.Max (vertex));
        }

        public static BBox operator + (BBox bbox, BBox other)
        {
			return new BBox (bbox.Min.Min (other.Min), bbox.Max.Max (other.Max));
        }

        public static BBox operator * (Mat4 matrix, BBox bbox)
        {
            var result = new BBox (matrix.Transform (bbox.Corners.First ()));
            foreach (var corner in bbox.Corners.Skip (1))
                result += matrix.Transform (corner);
            return result;
        }

		public static BBox FromPositions (IEnumerable<Vec3> vertices)
		{
			var result = new BBox (vertices.First ());
			foreach (var vertex in vertices.Skip (1))
				result += vertex;
			return result;
		}
	}
}