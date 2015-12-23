namespace ComposeTester
{
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.SceneGraph;
	using Compose3D;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;

	public class FighterGeometry<V, P>
		where V : struct, IVertex, IVertexColor<Vec3>
		where P : struct, IPositional<Vec3>, IDiffuseColor<Vec3>
	{
		public readonly Geometry<V> Fighter;
		public readonly IEnumerable<LineSegment<P, Vec3>> LineSegments;

        private class Nose
		{
			public readonly Path<P, Vec3> Profile;
			public readonly Geometry<V> Cone;
			public readonly Path<P, Vec3> XSection;

			public Nose (float middleHeight, float baseHeight, int numPoints, float bottomFlatness)
			{
				var spline = BSpline<Vec3>.FromControlPoints (2,
					new Vec3 (-1f, 0f, 0f),
					new Vec3 (0f, middleHeight, 0f),
					new Vec3 (1f, baseHeight, 0f));
				Profile = Path<P, Vec3>.FromBSpline (spline, 8);

				Cone = Lathe<V>.Turn (Profile, Axis.X, new Vec3 (0f), MathHelper.TwoPi / numPoints)
					.ManipulateVertices (Manipulators.Scale<V> (1f, 1f - bottomFlatness, 1f).Where (v => v.Position.Y < 0f))
					.RotateY (90f.Radians ());

				XSection = Path<P, Vec3>.FromVecs (
					from v in Cone.Vertices.Furthest (Dir3D.Back)
					select v.Position);
			}
		}

		private class CockpitFuselage
		{
			public readonly Geometry<V> Fuselage;
			public readonly Path<P, Vec3> XSection;
			public readonly P XSectionStart;

			public CockpitFuselage (Nose nose, float length, float baseScale, float bend)
			{
				Fuselage = Solids.Extrude<V, P> (false, false, nose.XSection,
					nose.XSection.Transform (Mat.Translation<Mat4> (0f, 0, -length) * 
						Mat.Scaling<Mat4> (1f, baseScale, baseScale)));

				Fuselage = Composite.Create (Stacking.StackBackward (nose.Cone, Fuselage));
				XSection = Path<P, Vec3>.FromVecs (
					from v in Fuselage.Vertices.Furthest (Dir3D.Back)
					where v.Position.Y >= -0.2f
					select v.Position).Close ();
				var pivotPoint = XSection.Nodes.Furthest (Dir3D.Up).First ().Position;
				Fuselage = Fuselage.ManipulateVertices (Manipulators.Transform<V> (
					Mat.RotationX<Mat4> (bend.Radians ()).RelativeTo (pivotPoint))
					.Where (v => v.Position.Z > pivotPoint.Z));

				XSectionStart = XSection.Nodes.Furthest (Dir3D.Down + Dir3D.Left).Single ();
				XSection = XSection.RenumberNodes (XSection.Nodes.IndexOf (XSectionStart)).Open ();
			}
		}

		private class MainFuselage
		{
			public readonly Geometry<V> Fuselage;
			public readonly Path<P, Vec3> XSection;

			public MainFuselage (CockpitFuselage cockpitFuselage)
			{
				XSection = CrossSection (
					cockpitFuselage.XSectionStart.Position,
					cockpitFuselage.XSection.Nodes.Furthest (Dir3D.Up).First ().Position.Y,
					cockpitFuselage.XSection.Nodes.Length);

				var graySlide = new Vec3 (1f).Interpolate (new Vec3 (0f), cockpitFuselage.XSection.Nodes.Length);
				cockpitFuselage.XSection.Nodes.Color (graySlide);
				XSection.Nodes.Color (graySlide);

				var transforms = from z in Ext.Range (0f, -2f, -0.25f)
								 select Mat.Translation<Mat4> (0f, 0f, z);
				var paths = Ext.Append (
					cockpitFuselage.XSection.MorphTo (XSection, transforms),
					XSection.Translate (0f, 0f, -4f));
				Fuselage = paths.Extrude<V, P> (false, true);
			}

			private Path<P, Vec3> CrossSection (Vec3 start, float top, int nodeCount)
			{
				start *= new Vec3 (2f, 1f, 1f);
				var cPoints = new Vec3[]
				{
					start,
					new Vec3 (start.X * 0.6f, top * 0.2f, start.Z),
					new Vec3 (start.X * 0.5f, top * 0.8f, start.Z),
				};
				var spline = BSpline<Vec3>.FromControlPoints (2,
					cPoints.Append (new Vec3 (0f, top * 1.5f, start.Z))
					.Concat (cPoints.Select (v => new Vec3 (-v.X, v.Y, v.Z)).Reverse ())
					.ToArray ());
				return Path<P, Vec3>.FromBSpline (spline, nodeCount);
			}
		}

		private class EngineIntake
		{
			public readonly Geometry<V> Intake;
			public readonly Path<P, Vec3> XSection;
			public readonly Path<P, Vec3> BellyXSection;
			public readonly Geometry<V> Belly;

			public EngineIntake (CockpitFuselage cockpitFuselage)
			{
				var startNode = cockpitFuselage.XSectionStart;
                XSection = CrossSection (startNode.Position,
					-cockpitFuselage.XSection.Nodes.Furthest (Dir3D.Up).First ().Position.Y, 20);
				var graySlide = new Vec3 (1f).Interpolate (new Vec3 (0f), XSection.Nodes.Length);
				XSection.Nodes.Color (graySlide);

				var transforms =
					from s in Ext.Range (0.25f, 1f, 0.25f)
					select Mat.Translation<Mat4> (0f, 0f, -s) *
						Mat.Scaling<Mat4> (1f + (0.45f * s), 1f + (0.25f * s.Sqrt ()), 1f)
							.RelativeTo (new Vec3 (0f, startNode.Position.Y, 0f));
				Intake = XSection
					.Inset<P, V> (0.9f, 0.9f)
					.Stretch (transforms, true, false);

				BellyXSection = Path<P, Vec3>.FromVecs (
					from v in cockpitFuselage.Fuselage.Vertices.Furthest (Dir3D.Back)
					where v.Position.Y < -0.1f
					select v.Position);
				Belly = Ext.Enumerate (BellyXSection, BellyXSection.Transform (
						Mat.Translation<Mat4> (0f, 0f, -1f) * Mat.Scaling<Mat4> (1.45f, 1f, 1f)))
					.Extrude<V, P> (false, false);
			}

			private Path<P, Vec3> CrossSection (Vec3 start, float bottom, int nodeCount)
			{
				var cPoints = new Vec3[]
				{
					start,
					new Vec3 (start.X * 1.1f, bottom * 0.6f, start.Z),
					new Vec3 (start.X * 0.8f, bottom * 1.0f, start.Z),
				};
				var spline = BSpline<Vec3>.FromControlPoints (2,
					cPoints.Append (new Vec3 (0f, bottom * 1.2f, start.Z))
					.Concat (cPoints.Select (v => new Vec3 (-v.X, v.Y, v.Z)).Reverse ())
					.ToArray ());
				return Path<P, Vec3>.FromBSpline (spline, nodeCount - 1);
			}
		}

		public FighterGeometry ()
		{
			var nose = new Nose (0.5f, 0.6f, 26, 0.4f);
			var cockpitFuselage = new CockpitFuselage (nose, 1.2f, 1.2f, 3f);
			var mainFuselage = new MainFuselage (cockpitFuselage);
			var intake = new EngineIntake (cockpitFuselage);

			LineSegments = from p in new Path<P, Vec3>[] { intake.XSection }
						   select new LineSegment<P, Vec3> (p);

			Fighter = Composite.Create (Stacking.StackBackward (cockpitFuselage.Fuselage, mainFuselage.Fuselage)
				.Concat (Ext.Enumerate (intake.Intake, intake.Belly)))
				.Smoothen (0.85f)
				.Color (VertexColor<Vec3>.Chrome)
				.Center ();
		}
	}
}