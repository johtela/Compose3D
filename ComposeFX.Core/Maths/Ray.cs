namespace ComposeFX.Maths
{
    public class Ray
    {
        public readonly Vec3 Origin;
        public readonly Vec3 Direction;

        private readonly Vec3 _invDir;
        private readonly Vec3i _dirSign;

        public Ray (Vec3 origin, Vec3 direction)
        {
            Origin = origin;
            Direction = direction;
            _invDir = 1f / Direction;
            _dirSign = Direction.Sign ().Max (new Vec3 (0)).ToVeci ();
        }

        public bool Intersects (Aabb<Vec3> aabb)
        {
            Vec3 tmin, tmax;
            var bounds = aabb.Bounds;

            tmin.X = (bounds[_dirSign.X].X - Origin.X) * _invDir.X;
            tmax.X = (bounds[1 - _dirSign.X].X - Origin.X) * _invDir.X;
            tmin.Y = (bounds[_dirSign.Y].Y - Origin.Y) * _invDir.Y;
            tmax.Y = (bounds[1 - _dirSign.Y].Y - Origin.Y) * _invDir.Y;

            if ((tmin.X > tmax.Y) || (tmin.Y > tmax.X))
                return false;
            if (tmin.Y > tmin.X)
                tmin.X = tmin.Y;
            if (tmax.Y < tmax.X)
                tmax.X = tmax.Y;

            tmin.Z = (bounds[_dirSign.Z].Z - Origin.Z) * _invDir.Z;
            tmax.Z = (bounds[1 - _dirSign.Z].Z - Origin.Z) * _invDir.Z;

            return tmin.X <= tmax.Z && tmin.Z <= tmax.X;
        }
    }
}
