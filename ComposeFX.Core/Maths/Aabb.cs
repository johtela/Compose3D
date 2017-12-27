namespace ComposeFX.Maths
{
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Enumeration that describes the alignment between two bounding boxes.
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
	/// Class representing an axis aligned bounding box.
	/// </summary>
	public class Aabb<V>
		where V : struct, IVec<V, float>
	{
		public readonly V Min;
		public readonly V Max;

		public Aabb (V pos)
		{
			Min = pos;
			Max = pos;
		}

		public Aabb (V pos1, V pos2)
		{
			Min = pos1.Min (pos2);
			Max = pos2.Max (pos1);
		}

		/// <summary>
		/// The X-coordinate of the left face of the bounding box.
		/// </summary>
		public float Left
		{
			get { return Min[0]; }
		}

		/// <summary>
		/// The X-coordinate of the right face of the bounding box.
		/// </summary>
		public float Right
		{
			get { return Max[0]; }
		}

		/// <summary>
		/// The Y-coordinate of the bottom face of the bounding box.
		/// </summary>
		public float Bottom
		{
			get { return Min[1]; }
		}

		/// <summary>
		/// The Y-coordinate of the top face of the bounding box.
		/// </summary>
		public float Top
		{
			get { return Max[1]; }
		}

		/// <summary>
		/// The Z-coordinate of the back face of the bounding box.
		/// </summary>
		public float Back
		{
			get { return Min[2]; }
		}

		/// <summary>
		/// The Z-coordinate of the front face of the bounding box.
		/// </summary>
		public float Front
		{
			get { return Max[2]; }
		}

		public V Size
		{
			get { return Max.Subtract (Min); }
		}

		public V Center
		{
			get { return Min.Add (Max).Divide (2f); }
		}

        public V[] Bounds
        {
            get { return new V[] { Min, Max }; }
        }

		private float[] Corner (int index, int dim)
		{
			var result = new float[dim];
			for (int i = 0; i < dim; i++)
			{
				result[i] = (index & 1) == 1 ? Max[i] : Min[i];
				index >>= 1;
			}
			return result;
		}

		public IEnumerable<V> Corners
		{
			get
			{
				var dim = Min.Dimensions;
				for (int i = 0; i < 1 << dim; i++)
					yield return Vec.FromArray<V, float> (Corner (i, dim));
			}
		}

		/// <summary>
		/// Gets the offset of a bounding boxwhen aligned with the current one along a given dimension.
		/// </summary>
		public float GetAlignmentOffset (Aabb<V> other, int dimension, Alignment align)
		{
			switch (align)
			{
				case Alignment.Negative: return Min[dimension] - other.Min[dimension];
				case Alignment.Positive: return Max[dimension] - other.Max[dimension];
				case Alignment.Center: return Center[dimension] - other.Center[dimension];
				default: return 0f;
			}
		}

		public static Aabb<V> operator + (Aabb<V> bbox, V pos)
		{
			return bbox == null ? 
				new Aabb<V> (pos) :
				new Aabb<V> (bbox.Min.Min (pos), bbox.Max.Max (pos));
		}

		public static Aabb<V> operator + (Aabb<V> bbox, Aabb<V> other)
		{
			return bbox == null || other == null ?
				bbox ?? other :
				new Aabb<V> (bbox.Min.Min (other.Min), bbox.Max.Max (other.Max));
		}

		public static bool operator & (Aabb<V> bbox, Aabb<V> other)
		{
			for (int i = 0; i < bbox.Min.Dimensions; i++)
				if (bbox.Max[i] < other.Min[i] || bbox.Min[i] > other.Max[i])
					return false;
			return true;
		}

		public static bool operator & (Aabb<V> bbox, V pos)
		{
			for (int i = 0; i < bbox.Min.Dimensions; i++)
				if (bbox.Max[i] < pos[i] || bbox.Min[i] > pos[i])
					return false;
			return true;
		}
		
		public static Aabb<V> operator * (Mat4 matrix, Aabb<V> bbox)
		{
			if (bbox == null)
				return null;
			var result = new Aabb<V> (matrix.Transform (bbox.Corners.First ()));
			foreach (var corner in bbox.Corners.Skip (1))
				result += matrix.Transform (corner);
			return result;
		}

		public static Aabb<V> FromPositions (IEnumerable<V> positions)
		{
			var result = new Aabb<V> (positions.First ());
			foreach (var vertex in positions.Skip (1))
				result += vertex;
			return result;
		}

		public override bool Equals (object obj)
		{
			var other = obj as Aabb<V>;
			return other != null && Min.Equals (other.Min) && Max.Equals (other.Max);
		}

		public override int GetHashCode ()
		{
			return Min.GetHashCode () ^ Max.GetHashCode ();
		}

		public override string ToString ()
		{
			return string.Format ("{0} -> {1}", Min, Max);
		}
	}
}