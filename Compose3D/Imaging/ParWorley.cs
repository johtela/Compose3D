namespace Compose3D.Imaging
{
	using System;
	using CLTypes;
	using Compiler;
	using Maths;
	using Parallel;

	public static class ParWorley
	{
		private static Func<Vec3, Vec3>
			Permute = CLKernel.Function
			(
				() => Permute,
				x => ((x * 34f + new Vec3 (1f)) * x).Mod (289f)
			);

		private static Func<Vec3, Vec3, int, Vec3>
			Distance3 = CLKernel.Function
			(
				() => Distance3,
				(v1, v2, distKind) =>
					distKind == (int)DistanceKind.Manhattan ? 
						v1.Abs () + v2.Abs () :
						(v1 * v1) + (v2 * v2)
			);

		private static Func<Vec2, int, float>
			Distance = CLKernel.Function
			(
				() => Distance,
				(vec, distKind) =>
					distKind == (int)DistanceKind.Manhattan ?
						Math.Abs (vec.X) + Math.Abs (vec.Y) :
						vec.Dot (vec)
			);

		private static Func<Vec3, Vec2, float, float, int, Vec3>
			GridDistances = CLKernel.Function
			(
				() => GridDistances,
				(p, Pf,	xoffs, jitter, distKind) => Kernel.Evaluate
				(
					from con in Kernel.Constants (new
					{
						K = new Vec3 (0.142857142857f), // 1/7
						Ko = new Vec3(0.428571428571f), // 3/7
						of = new Vec3 (-0.5f, 0.5f, 1.5f)
					})
					let pK = p * con.K
					let ox = pK.Fraction () - con.Ko
					let oy = pK.Floor ().Mod (7f) * con.K - con.Ko
					let dx = jitter * ox + new Vec3 (Pf.X + xoffs)
					let dy = jitter * oy + new Vec3 (Pf.Y) - con.of
					select Distance3 (dx, dy, distKind)
				)
			);

		public static Func<Vec2, float, int, Vec2>
			Noise2D = CLKernel.Function
			(
				() => Noise2D,
				(P, jitter, distKind) => Kernel.Evaluate
				(
					from con in Kernel.Constants (new
					{
						oi = new Vec3 (-1f, 0f, 1f),
					})
					let Pi = P.Floor ().Mod (289f)
					let Pf = P.Fraction ()
					let px = Permute (con.oi + new Vec3 (Pi.X))
					let d1 = GridDistances (Permute (new Vec3 (px.X + Pi.Y) + con.oi), Pf, 0.5f, jitter, distKind)
					let d2 = GridDistances (Permute (new Vec3 (px.Y + Pi.Y) + con.oi), Pf, -0.5f, jitter, distKind)
					let d3 = GridDistances (Permute (new Vec3 (px.Z + Pi.Y) + con.oi), Pf, -1.5f, jitter, distKind)
					let d1a = d1.Min (d2)
					let d2a = d1.Max (d2).Min (d3)
					let d1b = d1a.Min (d2a)
					let d2b = d1a.Max (d2a)
					let d1c = d1b.X < d1b.Y ? d1b : d1b[Coord.y, Coord.x, Coord.z]
					let d1d = d1c.X < d1c.Z ? d1c : d1c[Coord.z, Coord.y, Coord.x]
					let dyz = d1d[Coord.y, Coord.z].Min (d2b[Coord.y, Coord.z])
					let dy = Math.Min (dyz.X, Math.Min (dyz.Y, d2b.X))
					select new Vec2 (d1d.X, dy).Sqrt ()
				)
			);

		public static Func<Buffer<Vec2>, int, int, Vec2, Vec3>
			Noise2DCP = CLKernel.Function
			(
				() => Noise2DCP,
				(controlPoints, count, distKind, pos) =>
					Control<Vec3>.For (0, count, new Vec3 (1000000f),

						(i, res) => Kernel.Evaluate
						(
							from dist in Distance ((!controlPoints)[i] - pos, distKind).ToKernel ()
							select
								dist < res.X ? new Vec3 (dist, res.X, res.Y) :
								dist < res.Y ? new Vec3 (res.X, dist, res.Y) :
								dist < res.Z ? new Vec3 (res.X, res.Y, dist) :
								res
						)
					)
					.Sqrt ()
			);
	}
}
