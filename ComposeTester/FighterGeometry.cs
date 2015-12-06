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

	public static class FighterGeometry
	{
		public static Geometry<Vertex> Fighter (out IEnumerable<LineSegment<PathNode, Vec3>> lineSegments)
		{
			var noseCone = Lathe<Vertex>.Turn (Geometries.NoseProfile (), Axis.X, new Vec3 (0f), MathHelper.Pi / 13f, 0f, 0f)
				.ManipulateVertices (Manipulators.Scale<Vertex> (1f, 0.6f, 1f).Where (v => v.position.Y < 0f))
				.RotateY (90f.Radians ());

			var noseXSection = Path<PathNode, Vec3>.FromVecs (
				from v in noseCone.Vertices.Furthest (Dir3D.Back)
				select v.position);

			var cockpitFuselage = Solids.Extrude<Vertex, PathNode> (false, false, noseXSection,
			   noseXSection.Transform (Mat.Translation<Mat4> (0f, 0, -1.2f) * Mat.Scaling<Mat4> (1f, 1.2f, 1.2f)));

			cockpitFuselage = Composite.Create (Stacking.StackBackward (noseCone, cockpitFuselage));
			var fuselageXSection = Path<PathNode, Vec3>.FromVecs (
				from v in cockpitFuselage.Vertices.Furthest (Dir3D.Back)
				where v.position.Y >= -0.2f
				select v.position).Close ();
			var pivotPoint = fuselageXSection.Nodes.Furthest (Dir3D.Up).First ().position;
			cockpitFuselage = cockpitFuselage.ManipulateVertices (Manipulators.Transform<Vertex> (
				Mat.RotationX<Mat4> (5f.Radians ()).RelativeTo (pivotPoint))
				.Where (v => v.position.Z > pivotPoint.Z));

			var botLeftCorner = fuselageXSection.Nodes.Furthest (Dir3D.Down + Dir3D.Left).Single ();
			fuselageXSection = fuselageXSection.RenumberNodes (fuselageXSection.Nodes.IndexOf (botLeftCorner)).Open ();

			var mainFuselageXSection = Geometries.HullCrossSection (
				botLeftCorner.position,
				fuselageXSection.Nodes.Furthest (Dir3D.Up).First ().position.Y,
				fuselageXSection.Nodes.Length);

			var graySlide = new Vec3 (1f).Interpolate (new Vec3 (0f), fuselageXSection.Nodes.Length);
			fuselageXSection.Nodes.Color (graySlide);
			mainFuselageXSection.Nodes.Color (graySlide);

			var transforms = from z in Ext.Range (0f, -2f, -0.25f)
							 select Mat.Translation<Mat4> (0f, 0f, z);
			var hullPaths = Ext.Append (
				fuselageXSection.MorphTo (mainFuselageXSection, transforms),
				mainFuselageXSection.Translate (0f, 0f, -4f));
			var hull = hullPaths.Extrude<Vertex, PathNode> (false, true);

			var intakeXSection = Geometries.IntakeCrossSection (botLeftCorner.position,
				-fuselageXSection.Nodes.Furthest (Dir3D.Up).First ().position.Y, 20);
			graySlide = new Vec3 (1f).Interpolate (new Vec3 (0f), intakeXSection.Nodes.Length);
			intakeXSection.Nodes.Color (graySlide);

			var intakeTransforms =
				from s in Ext.Range (0.25f, 1f, 0.25f)
				select Mat.Translation<Mat4> (0f, 0f, -s) *
					Mat.Scaling<Mat4> (1f + (0.45f * s), 1f + (0.25f * s.Sqrt ()), 1f)
						.RelativeTo (new Vec3 (0f, botLeftCorner.position.Y, 0f));

			var intake = intakeXSection
				.Inset<PathNode, Vertex> (0.9f, 0.9f)
				.Stretch (intakeTransforms, true, false);

			var bellyXSection = Path<PathNode, Vec3>.FromVecs (
				from v in cockpitFuselage.Vertices.Furthest (Dir3D.Back)
				where v.position.Y < -0.2f
				select v.position);
			var belly = Ext.Enumerate (bellyXSection, bellyXSection.Transform (
					Mat.Translation<Mat4> (0f, 0f, -1f) * Mat.Scaling<Mat4> (1.45f, 1f, 1f)))
				.Extrude<Vertex, PathNode> (false, false);

			lineSegments = from p in new Path<PathNode, Vec3>[] { intakeXSection }
						   select new LineSegment<PathNode, Vec3> (p);

			return Composite.Create (Stacking.StackBackward (cockpitFuselage, hull).Concat (Ext.Enumerate (intake, belly)))
				.Smoothen (0.85f)
				.Color (VertexColor<Vec3>.Chrome)
				.Center ();
		}
	}
}
