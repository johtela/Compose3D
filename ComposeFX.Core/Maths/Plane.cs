namespace ComposeFX.Maths
{
	using DataStructures;

	public struct Plane
	{
		public readonly Vec3 Normal;
		public readonly float Distance;
		
		public Plane (Vec3 normal, float distance)
		{
			Normal = normal;
			Distance = distance;
		}
		
		public Plane (Vec3 p0, Vec3 p1, Vec3 p2)
		{
			Normal = p0.CalculateNormal (p1, p2);
			Distance = -Normal.Dot (p0);
		}
		
		public float DistanceFromPoint (Vec3 p)
		{
			return p.Dot (Normal) + Distance;
		}
		
		public Vec3 ProjectPoint (Vec3 p)
		{
			return p - DistanceFromPoint (p) * Normal;
		}

		public bool PointInside (Vec3 p)
		{
			return DistanceFromPoint (p) >= 0f;
		}

		public bool BoundingBoxInside (Aabb<Vec3> bb)
		{
			foreach (var p in bb.Corners)
				if (DistanceFromPoint (p) >= 0f)
					return true;
			return false;
		}
	}
}

