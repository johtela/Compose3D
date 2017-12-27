namespace ComposeFX.Maths
{
    using System;

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

        public float Intersects (Aabb<Vec3> aabb)
        {
            var bounds = aabb.Bounds;
            var tmin = (bounds[_dirSign.X].X - Origin.X) * _invDir.X;
            var tmax = (bounds[1 - _dirSign.X].X - Origin.X) * _invDir.X;
            var tmin2 = (bounds[_dirSign.Y].Y - Origin.Y) * _invDir.Y;
            var tmax2 = (bounds[1 - _dirSign.Y].Y - Origin.Y) * _invDir.Y;

            if (tmin > tmax2 || tmin2 > tmax)
                return -1f;

            tmin = Math.Max (tmin, tmin2);
            tmax = Math.Min (tmax, tmax2);
            tmin2 = (bounds[_dirSign.Z].Z - Origin.Z) * _invDir.Z;
            tmax2 = (bounds[1 - _dirSign.Z].Z - Origin.Z) * _invDir.Z;

            return tmin > tmax2 || tmin2 > tmax ? -1 : Math.Max (tmin, tmin2);
        }
    }
}
