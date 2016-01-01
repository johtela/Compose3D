﻿namespace ComposeTester
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
		private static IVertexColor<Vec3> _color = VertexColor<Vec3>.Chrome;

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
					.RotateY (90f.Radians ())
					.Color (_color);

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
				Fuselage = Extrusion.Extrude<V, P> (false, false, nose.XSection,
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
					.Where (v => v.Position.Z > pivotPoint.Z))
					.Color (_color);

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

				var transforms = from z in Ext.Range (0f, -2f, -0.25f)
								 select Mat.Translation<Mat4> (0f, 0f, z);
				var paths = Ext.Append (
					cockpitFuselage.XSection.MorphTo (XSection, transforms),
					XSection.Translate (0f, 0f, -6f));
				Fuselage = paths.Extrude<V, P> (false, false)
					.Color (_color);
			}

			private Path<P, Vec3> CrossSection (Vec3 start, float top, int nodeCount)
			{
				start *= new Vec3 (2f, 1f, 1f);
				var cPoints = new Vec3[]
				{
					start,
					new Vec3 (start.X * 0.7f, top * 0.1f, start.Z),
					new Vec3 (start.X * 0.6f, top * 0.7f, start.Z),
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
			public readonly Path<P, Vec3> RearXSection;
			public readonly Path<P, Vec3> BellyXSection;
			public readonly Geometry<V> Belly;

			public EngineIntake (CockpitFuselage cockpitFuselage)
			{
				var startNode = cockpitFuselage.XSectionStart;
                XSection = CrossSection (startNode.Position,
					-cockpitFuselage.XSection.Nodes.Furthest (Dir3D.Up).First ().Position.Y, 20);

				var transforms =
					from s in Ext.Range (0.25f, 2f, 0.25f)
					let scaleFactor = 1f + (0.25f * s.Sqrt ())
					select Mat.Translation<Mat4> (0f, 0f, -s) *
						Mat.Scaling<Mat4> (scaleFactor, scaleFactor, 1f)
						.RelativeTo (new Vec3 (0f, startNode.Position.Y, 0f));
				Intake = XSection
					.Inset<P, V> (0.9f, 0.9f)
					.Stretch (transforms, true, false)
					.Color (_color);
				RearXSection = XSection.Transform (transforms.Last ());

				BellyXSection = Path<P, Vec3>.FromVecs (
					from v in cockpitFuselage.Fuselage.Vertices.Furthest (Dir3D.Back)
					where v.Position.Y < -0.1f
					select v.Position);
				var scalePoint = new Vec3 (0f, BellyXSection.Nodes.First ().Position.Y, 0f);
				Belly = Ext.Enumerate (BellyXSection, 
					BellyXSection.Transform (Mat.Translation<Mat4> (0f, 0f, -1f) * 
						Mat.Scaling<Mat4> (1.45f, 1f, 1f).RelativeTo (scalePoint)),
					BellyXSection.Transform (Mat.Translation<Mat4> (0f, 0f, -2f) * 
						Mat.Scaling<Mat4> (1.9f, 1.25f, 1f).RelativeTo (scalePoint)),
					BellyXSection.Transform (Mat.Translation<Mat4> (0f, 0f, -2.5f) * 
						Mat.Scaling<Mat4> (1.9f, 1.3f, 1f).RelativeTo (scalePoint)))
					.Extrude<V, P> (false, false)
					.Color (_color);
			}

			private Path<P, Vec3> CrossSection (Vec3 start, float bottom, int nodeCount)
			{
				var cPoints = new Vec3[]
				{
					start,
					new Vec3 (start.X * 1.2f, bottom * 0.6f, start.Z),
					new Vec3 (start.X * 0.8f, bottom * 1.1f, start.Z),
				};
				var spline = BSpline<Vec3>.FromControlPoints (2,
					cPoints.Append (new Vec3 (0f, bottom * 1.2f, start.Z))
					.Concat (cPoints.Select (v => new Vec3 (-v.X, v.Y, v.Z)).Reverse ())
					.ToArray ());
				return Path<P, Vec3>.FromBSpline (spline, nodeCount - 1);
			}
		}

		private class Underside
		{
			public readonly Geometry<V> Geometry;
			public Path<P, Vec3> XSection;

			public Underside (EngineIntake intake)
			{
				var nodes = 
					from n in intake.RearXSection.ReverseWinding ().Nodes
					where n.Position.Y <= -0.2f
					select n;				
				XSection = new Path<P, Vec3> (nodes);
				var firstNode = XSection.Nodes.First ();
				var paths =
					from s in Ext.Range (0f, 4f, 2f)
					let scaleFactor = 1f - (0.01f * s * s)
					select XSection.Transform (
						Mat.Translation<Mat4> (0f, 0f, -s) *
						Mat.Scaling<Mat4> (scaleFactor, scaleFactor, 1f)
						.RelativeTo (new Vec3 (0f, firstNode.Position.Y, 0f)));
				Geometry = paths.Extrude<V, P> (false, false)
					.Color (_color);
			}
		}
		
		private class Canopy
		{
			public readonly Geometry<V> Geometry;
			public Path<P, Vec3> Profile;
			
			public Canopy(float frontHeight, float backHeight, float bend, int numPoints)
			{
				var spline = BSpline<Vec3>.FromControlPoints (2,
					new Vec3 (-1.1f, 0f, 0f),
					new Vec3 (-1f, 0.2f, 0f),
					new Vec3 (-0f, frontHeight, 0f),
					new Vec3 (1f, backHeight, 0f),
					new Vec3 (2.5f, 0.1f, 0f),
					new Vec3 (2.5f, 0f, 0f)
				);
				Profile = Path<P, Vec3>.FromBSpline (spline, numPoints);
				var angle = 100f.Radians ();
				Geometry = Lathe<V>.Turn (Profile, Axis.X, new Vec3 (0f), MathHelper.Pi / numPoints, 
					-angle, angle)
					.RotateY (90f.Radians ())
					.RotateX (bend.Radians ())
					.Scale (0.85f, 1f, 1f)
					.Translate (0f, 0.57f, -2.3f)
					.Color (VertexColor<Vec3>.GreyPlastic);
			}
		}

		public FighterGeometry ()
		{
			var nose = new Nose (0.5f, 0.6f, 26, 0.4f);
			var cockpitFuselage = new CockpitFuselage (nose, 1.2f, 1.2f, 3f);
			var mainFuselage = new MainFuselage (cockpitFuselage);
			var intake = new EngineIntake (cockpitFuselage);
			var underside = new Underside (intake);
			var canopy = new Canopy (0.6f, 0.5f, 3f, 16);
			var path = canopy.Profile;
			var graySlide = new Vec3 (1f).Interpolate (new Vec3 (0f), path.Nodes.Length);
			path.Nodes.Color (graySlide);
			
			LineSegments = from p in new Path<P, Vec3>[] { path }
						   select new LineSegment<P, Vec3> (p.Close ());
			
			Fighter = Composite.Create (Stacking.StackBackward (cockpitFuselage.Fuselage, mainFuselage.Fuselage)
				.Concat (Ext.Enumerate (intake.Intake, intake.Belly, underside.Geometry, canopy.Geometry)))
				.Smoothen (0.85f)
				.Center ();
		}
	}
}