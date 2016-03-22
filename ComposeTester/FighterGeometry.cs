namespace ComposeTester
{
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.SceneGraph;
	using Compose3D;
	using System.Collections.Generic;
	using System.Linq;
	using Extensions;
	using OpenTK;

	public class FighterGeometry<V, P>
		where V : struct, IVertex, IVertexColor<Vec3>
		where P : struct, IPositional<Vec3>, IDiffuseColor<Vec3>
	{
		public readonly Geometry<V> Fighter;
		public readonly IEnumerable<Path<P, Vec3>> Paths;
		private static IVertexColor<Vec3> _color = VertexColor<Vec3>.GreyPlastic;

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

				Cone = Lathe<V>.Turn (Profile, 
						turnAxis: Axis.X, 
						offset: new Vec3 (0f), 
						stepAngle: MathHelper.TwoPi / numPoints)
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
			public readonly Path<P, Vec3> RearXSection;

			public MainFuselage (CockpitFuselage cockpitFuselage)
			{
				XSection = CrossSection (
					cockpitFuselage.XSectionStart.Position,
					cockpitFuselage.XSection.Nodes.Furthest (Dir3D.Up).First ().Position.Y,
					cockpitFuselage.XSection.Nodes.Length);

				var transforms = from z in EnumerableExt.Range (0f, -2f, -0.25f)
								 select Mat.Translation<Mat4> (0f, 0f, z);
				var paths = EnumerableExt.Append (
					cockpitFuselage.XSection.MorphTo (XSection, transforms),
					XSection.Translate (0f, 0f, -6.75f));
				RearXSection = paths.Last ();
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
					from s in EnumerableExt.Range (0.25f, 2f, 0.25f)
					let scaleFactor = 1f + (0.2f * s.Pow (0.5f))
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
				Belly = EnumerableExt.Enumerate (BellyXSection, 
					BellyXSection.Transform (Mat.Translation<Mat4> (0f, 0f, -1f) * 
						Mat.Scaling<Mat4> (1.45f, 1f, 1f).RelativeTo (scalePoint)),
					BellyXSection.Transform (Mat.Translation<Mat4> (0f, 0f, -2f) * 
						Mat.Scaling<Mat4> (1.9f, 1.25f, 1f).RelativeTo (scalePoint)),
					BellyXSection.Transform (Mat.Translation<Mat4> (0f, 0f, -3f) * 
						Mat.Scaling<Mat4> (1.9f, 1.3f, 1f).RelativeTo (scalePoint)))
					.Extrude<V, P> (false, false)
					.Color (_color);
			}

			private Path<P, Vec3> CrossSection (Vec3 start, float bottom, int nodeCount)
			{
				start.Z -= 0.75f;
				var cPoints = new Vec3[]
				{
					start,
					new Vec3 (start.X * 1.2f, bottom * 0.6f, start.Z),
					new Vec3 (start.X * 0.9f, bottom * 1.2f, start.Z),
				};
				var spline = BSpline<Vec3>.FromControlPoints (2,
					cPoints.Append (new Vec3 (0f, bottom * 1.35f, start.Z))
					.Concat (cPoints.Select (v => new Vec3 (-v.X, v.Y, v.Z)).Reverse ())
					.ToArray ());
				return Path<P, Vec3>.FromBSpline (spline, nodeCount - 1);
			}
		}

		private class Underside
		{
			public readonly Geometry<V> Geometry;
			public readonly Path<P, Vec3> XSection;
			public readonly Path<P, Vec3> RearXSection;

			public Underside (EngineIntake intake)
			{
				var nodes = 
					from n in intake.RearXSection.ReverseWinding ().Nodes
					where n.Position.Y <= -0.2f
					select n;				
				XSection = new Path<P, Vec3> (nodes);
				var firstNode = XSection.Nodes.First ();
				var paths =
					from s in EnumerableExt.Range (0f, 4f, 1f)
					let scaleFactor = (0.15f * s).Pow (2f)
					select XSection.Transform (
						Mat.Translation<Mat4> (0f, 0f, -s) *
						Mat.Scaling<Mat4> (1f - (scaleFactor * 0.25f), 1f - scaleFactor, 1f)
						.RelativeTo (new Vec3 (0f, firstNode.Position.Y, 0f)));
				RearXSection = paths.Last ();
				Geometry = paths.Extrude<V, P> (false, false)
					.Color (_color);
			}
		}

		private class Rear
		{
			public readonly Geometry<V> Geometry;
			public readonly Path<P, Vec3> XSection;
			public readonly Path<P, Vec3> RearXSection;
			public readonly Path<P, Vec3> EngineXSection;
			public readonly Path<P, Vec3> ExhaustXSection;

			public Rear (MainFuselage fuselage, Underside underside)
			{
				var pos1 = new P () { Position = fuselage.RearXSection.Nodes.First ().Position + new Vec3 (0f, -0.1f, 0f) };
				var pos2 = new P () { Position = fuselage.RearXSection.Nodes.Last ().Position + new Vec3 (0f, -0.1f, 0f) };
				XSection = +(fuselage.RearXSection + pos2 + underside.RearXSection + pos1);
				RearXSection = +(fuselage.RearXSection.Scale (1f, 0.9f) + pos2 + 
					BottomXSection (underside.RearXSection) + pos1);
				var transforms = 
					from s in EnumerableExt.Range (0f, 2.5f, 0.5f)
					select Mat.Translation<Mat4> (0f, s / 25f, -s);
				var paths = XSection.MorphTo (RearXSection, transforms);
				var rear = paths.Extrude<V, P> (false, false);
				RearXSection = paths.Last (); 
				EngineXSection = new Path<P, Vec3> (
					from n in RearXSection.Nodes
					where n.Position.X >= -0.9f && n.Position.X <= 0.9f
					select new P () 
					{ 
						Position = new Vec3 (n.Position.X.Clamp (-0.75f, 0.75f), n.Position.Y, n.Position.Z)
					})
					.Close ();
				ExhaustXSection = EngineXSection.Scale (0.8f, 0.7f);
				transforms =
					from s in EnumerableExt.Range (0f, 1f, 0.5f)
					select Mat.Translation<Mat4> (0f, s / 10f, -s);
				var engine = EngineXSection.MorphTo (ExhaustXSection, transforms)
					.Extrude<V, P> (false, true);
				Geometry = Composite.Create (rear, engine)
					.Color (_color);
			}

			private Path<P, Vec3> BottomXSection (Path<P, Vec3> underside)
			{
				var first = underside.Nodes.First (); 
				var radiusX = first.Position.X;
				var radiusY = -underside.Nodes.Furthest (Dir3D.Down).First ().Position.Y * 0.8f;
				return Path<P, Vec3>.FromPie (radiusX, radiusY, 340f.Radians (), 200f.Radians (), underside.Nodes.Length)
					.Translate (0f, 0f, first.Position.Z);
			}
		}

		private class Exhaust
		{
			public readonly Geometry<V> Geometry;
			public readonly Geometry<V> StabilizerFlange;
			public readonly Path<P, Vec3> FlangeXSection;
			public readonly Path<P, Vec3> FlangeEndXSection;

			public Exhaust (Rear rear, float length)
			{
				var plateSide = Quadrilateral<V>.Trapezoid (0.38f, length, 0f, -0.1f);
				var nozzlePlate = Composite.Create (plateSide, plateSide.ReflectZ ());
				var plateCnt = 12;
				var plates = new Geometry<V>[plateCnt]; 
				var angle = 0f;
				for (int i = 0; i < plateCnt; i++, angle += MathHelper.TwoPi / plateCnt)
					plates[i] = nozzlePlate
						.Translate (0f, length / 2f, 0f)
						.RotateX (-105f.Radians ())
						.Translate (0f, 0.62f, 0f)
						.RotateZ (angle);
				var snapToVertex = rear.Geometry.Vertices.Furthest (Dir3D.Back).Furthest (Dir3D.Up).First ();
				var exhaust = Composite.Create (plates);
				Geometry = exhaust
					.SnapVertex (exhaust.Vertices.Furthest (Dir3D.Up).First (), snapToVertex, Axes.Y | Axes.Z)
					.Translate (0f, -0.02f, 0.02f)
					.Color (VertexColor<Vec3>.BlackPlastic); 
				
				FlangeXSection = new Path<P, Vec3> (
					from n in rear.RearXSection.Nodes
					where n.Position.X < -0.65f && n.Position.Y < 0.4f
					select n)
					.Close ();
				FlangeEndXSection = new Path<P, Vec3> (
					from n in FlangeXSection.Nodes
					select new P ()
					{
						Position = new Vec3 (n.Position.X, n.Position.Y.Clamp (-0.2f, 0.1f), n.Position.Z)
					});
				var center = FlangeEndXSection.Nodes.Center<P, Vec3> ();
				StabilizerFlange = FlangeXSection.MorphTo (FlangeEndXSection,
						EnumerableExt.Range (0f, -1.25f, -0.25f).Select (s => Mat.Translation<Mat4> (0f, 0f, s) *
							Mat.Scaling<Mat4> (1f, 1f - (s * 0.3f).Pow (2f)).RelativeTo (center)))
					.Extrude<V, P> (false, true)
					.Color (_color);
			}
		}

		private class Canopy
		{
			public readonly Geometry<V> Geometry;
			public readonly Path<P, Vec3> Profile;
			
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
				var angle = 105f.Radians ();
				Geometry = Lathe<V>.Turn (Profile, Axis.X, new Vec3 (0f), MathHelper.Pi / numPoints, 
					-angle, angle)
					.RotateY (90f.Radians ())
					.RotateX (bend.Radians ())
					.Scale (0.85f, 1f, 1f)
					.Translate (0f, 0.6f, -2.3f)
					.Color (VertexColor<Vec3>.BluePlastic);
			}
		}
		
		private class Wing
		{
			public readonly Geometry<V> Geometry;
			
			public Wing (float width, float length)
			{
				var botHalf = Quadrilateral<V>.Trapezoid (length, width, length * 0.75f, 0f)
					.ExtrudeToScale (
						depth: 0.1f, 
						targetScale: 0.5f, 
						steepness: 3f,
						numSteps: 5,
						includeFrontFace: false,
						scaleAround: new Vec3 (length / 4f, -width / 2f, 0f))
					.FilterVertices (v => !v.Facing (Dir3D.Down));
				Geometry = Composite.Create (
						Stacking.StackForward (botHalf, botHalf.ReflectZ ()))
					.RotateX (-MathHelper.PiOver2)
					.RotateY (MathHelper.PiOver2)
					.Translate (-2.7f, -0.25f, -7f)
					.Color (_color);
			}
		}

		private class TailFin
		{
			public readonly Geometry<V> Geometry;

			public TailFin ()
			{
				var half = Polygon<V>.FromVec2s (
					new Vec2 (-4f, 0f),
					new Vec2 (-1.75f, 0.5f),
					new Vec2 (0f, 3f),
					new Vec2 (1.25f, 3f),
					new Vec2 (0.5f, 0.5f),
					new Vec2 (0.5f, 0f))
					.ExtrudeToScale (
						depth: 0.1f,
						targetScale: 0.5f,
						steepness: 3f,
						numSteps: 5,
						includeFrontFace: false);
				Geometry = Composite.Create (
						Stacking.StackForward (half, half.ReflectZ ()))
					.RotateY (MathHelper.PiOver2)
					.Translate(0f, 0.9f, -12.5f)
					.Color (_color);
			}
		}
		
		private class Stabilizer
		{
			public readonly Geometry<V> Geometry;

			public Stabilizer ()
			{
				var half = Polygon<V>.FromVec2s (
					new Vec2 (-2f, 0.5f),
					new Vec2 (0f, 2f),
					new Vec2 (0f, -0.5f),
					new Vec2 (-1.75f, -0.5f),
					new Vec2 (-2f, -0.25f))
					.ExtrudeToScale (
						depth: 0.07f,
						targetScale: 0.5f,
						steepness: 3f,
						numSteps: 5,
						includeFrontFace: false);
				Geometry = Composite.Create (
					Stacking.StackForward (half, half.ReflectZ ()))
					.RotateX (MathHelper.PiOver2)
					.RotateZ (5f.Radians ())
					.Translate(-1.1f, -0.1f, -12.8f)
					.Color (_color);
			}
		}

		private class BottomFin
		{
			public readonly Geometry<V> Geometry;

			public BottomFin ()
			{
				var half = Polygon<V>.FromVec2s (
					new Vec2 (-0.8f, 0f),
					new Vec2 (0.8f, 0f),
					new Vec2 (0.6f, -0.5f),
					new Vec2 (-0f, -0.6f))
					.ExtrudeToScale (
						depth: 0.05f,
						targetScale: 0.7f,
						steepness: 3f,
						numSteps: 2,
						includeFrontFace: false)
					.FilterVertices (v => !v.Facing (Dir3D.Up));
				Geometry = Composite.Create (
						Stacking.StackForward (half, half.ReflectZ ()))
					.RotateY (MathHelper.PiOver2)
					.RotateZ (15f.Radians ())
					.RotateX (6f.Radians ())
					.Translate (0.5f, -0.6f, -10.25f)
					.Color (_color);
			}
		}

		public FighterGeometry ()
		{
			var nose = new Nose (0.5f, 0.6f, 26, 0.4f);
			var cockpitFuselage = new CockpitFuselage (nose, 1.2f, 1.2f, 3f);
			var mainFuselage = new MainFuselage (cockpitFuselage);
			var intake = new EngineIntake (cockpitFuselage);
			var underside = new Underside (intake);
			var canopy = new Canopy (0.65f, 0.5f, 3f, 16);
			var wing = new Wing (4.5f, 4.5f);
			var rear = new Rear (mainFuselage, underside);
			var tailFin = new TailFin ();
			var stabilizer = new Stabilizer ();
			var exhaust = new Exhaust (rear, 0.6f);
			var bottomFin = new BottomFin ();

			var path = exhaust.FlangeEndXSection;
			var graySlide = new Vec3 (1f).Interpolate (new Vec3 (0f), path.Nodes.Length);
			path.Nodes.Color (graySlide);
			Paths = EnumerableExt.Enumerate (path);
			
			Fighter = Composite.Create (Stacking.StackBackward (cockpitFuselage.Fuselage, mainFuselage.Fuselage)
				.Concat (EnumerableExt.Enumerate (intake.Intake, intake.Belly, underside.Geometry, canopy.Geometry, 
					wing.Geometry, wing.Geometry.ReflectX (), rear.Geometry, exhaust.Geometry,
					exhaust.StabilizerFlange, exhaust.StabilizerFlange.ReflectX (),
					tailFin.Geometry, stabilizer.Geometry, stabilizer.Geometry.ReflectX (),
					bottomFin.Geometry, bottomFin.Geometry.ReflectX ())))
				.Smoothen (0.85f)
				.Center ();
		}
	}
}