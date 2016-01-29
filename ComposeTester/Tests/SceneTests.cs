namespace ComposeTester
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using LinqCheck;
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.SceneGraph;

	public class SceneTests
	{
		[Test]
		public void TestOrthographicCullingPlanes ()
		{
			var vf = new ViewingFrustum (FrustumKind.Orthographic, 2f, 2f, 1f, 100f);
			var cp = vf.CullingPlanes (new Mat4 (1f));

			Check.AreEqual (1, cp[0].Distance);
			Check.AreEqual (Dir3D.Right, cp[0].Normal);
			Check.AreEqual (1, cp[1].Distance);
			Check.AreEqual (Dir3D.Left, cp[1].Normal);
			Check.AreEqual (1, cp[2].Distance);
			Check.AreEqual (Dir3D.Up, cp[2].Normal);
			Check.AreEqual (1, cp[3].Distance);
			Check.AreEqual (Dir3D.Down, cp[3].Normal);
			Check.AreEqual (-1, cp[4].Distance);
			Check.AreEqual (Dir3D.Back, cp[4].Normal);
			Check.AreEqual (100, cp[5].Distance);
			Check.AreEqual (Dir3D.Front, cp[5].Normal);
		}

		[Test]
		public void TestPerspectiveCullingPlanes ()
		{
			var vf = new ViewingFrustum (FrustumKind.Perspective, 2f, 2f, 1f, 100f);
			var cp = vf.CullingPlanes (new Mat4 (1f));

			Check.AreEqual (0f, cp[0].Distance);
			Check.AreEqual ((Dir3D.Right + Dir3D.Back).Normalized, cp[0].Normal);
			Check.AreEqual (0f, cp[1].Distance);
			Check.AreEqual ((Dir3D.Left + Dir3D.Back).Normalized, cp[1].Normal);
			Check.AreEqual (0f, cp[2].Distance);
			Check.AreEqual ((Dir3D.Up + Dir3D.Back).Normalized, cp[2].Normal);
			Check.AreEqual (0f, cp[3].Distance);
			Check.AreEqual ((Dir3D.Down + Dir3D.Back).Normalized, cp[3].Normal);
			Check.AreEqual (-1, cp[4].Distance);
			Check.AreEqual (Dir3D.Back, cp[4].Normal);
			Check.AreEqual (100, cp[5].Distance);
			Check.AreEqual (Dir3D.Front, cp[5].Normal);

			Check.IsFalse (InsideFrustum (cp, new Vec3 (-5f, 3f, -3f)));
			Check.IsTrue (InsideFrustum (cp, new Vec3 (-2, 3f, -3f)));
			Check.IsTrue (InsideFrustum (cp, new Vec3 (-3, 3f, -3f)));
			Check.IsFalse (InsideFrustum (cp, new Vec3 (-3, 3f, 0f)));
			Check.IsTrue (InsideFrustum (cp, new Vec3 (3, 3f, -100f)));
			Check.IsFalse (InsideFrustum (cp, new Vec3 (3, 3f, -101f)));
		}

		private bool InsideFrustum (Plane[] cullingPlanes, Vec3 point)
		{
			return cullingPlanes.All (cp => cp.DistanceFromPoint (point) >= 0f);
		}
	}
}
