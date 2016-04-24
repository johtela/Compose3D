namespace Compose3D.SceneGraph
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DataStructures;
	using Maths;
	using Extensions;

	public class Camera : SceneNode
	{
		public Vec3 Position;
		public Vec3 Target;
		public Vec3 UpDirection;

		private ViewingFrustum _frustum;
		private ViewingFrustum[] _splitFrustums;
		private float _logarithmicWeight;

		public Camera (SceneGraph graph, Vec3 position, Vec3 target, Vec3 upDirection, ViewingFrustum frustum, 
			float aspectRatio) : base (graph)
		{
			Position = position;
			Target = target;
			UpDirection = upDirection;
			Frustum = frustum;
		}
		
		internal override void AddToIndex () { }
		
		internal override void RemoveFromIndex () { }

		public IEnumerable<T> NodesInView<T> () where T : SceneNode
		{
			var cullingPlanes = Frustum.CullingPlanes (CameraToWorld);
			return from kv in Graph.Index.Overlap (BoundingBox)
				   where kv.Value is T && cullingPlanes.All (cp => cp.BoundingBoxInside (kv.Key))
				   select kv.Value as T;
		}

		public override Mat4 Transform
		{
			get { return CameraToWorld; }
		}

		public override Aabb<Vec3> BoundingBox
		{
			get	{ return Aabb<Vec3>.FromPositions (Frustum.Corners.Map (pos => Transform.Transform (pos)));	}
		}

		public Mat4 WorldToCamera 
		{
			get { return base.Transform * Mat.LookAt (Position, Target, UpDirection); }
		}

		public Mat4 CameraToWorld
		{
			get { return WorldToCamera.Inverse; }
		}

		public ViewingFrustum Frustum
		{
			get { return _frustum; }
 			set
			{
				_frustum = value;
				_splitFrustums = null;
			}
		}

		public ViewingFrustum[] SplitFrustumsForCascadedShadowMaps (int splitCount, 
			float logarithmicWeight = 0.5f)
		{
			if (_splitFrustums == null || _splitFrustums.Length != splitCount ||
				_logarithmicWeight != logarithmicWeight)
			{
				_splitFrustums = CreateSplitFrustums (splitCount, logarithmicWeight);
				_logarithmicWeight = logarithmicWeight;
			}
			return _splitFrustums;
		}

		private float[] CSMFrustumSplit (int splitCount, float logarithmicWeight)
		{
			if (logarithmicWeight < 0f || logarithmicWeight > 1f)
				throw new ArgumentException ("Logarithmic weight must be between 0f and 1f", "logarithmicWeight");
			var result = new float[splitCount + 1];
			var n = _frustum.Near;
			var f = _frustum.Far;
			result[0] = n;
			for (int i = 1; i < splitCount; i++)
			{
				var factor = (float)i / (float)splitCount;
				var log = n * (f / n).Pow (factor);
				var lin = n + (f - n) * factor;
				result[i] = log * logarithmicWeight + lin * (1 - logarithmicWeight);
			}
			result[splitCount] = f;
			return result;
		}

		private ViewingFrustum[] CreateSplitFrustums (int splitCount, float logarithmicWeight)
		{
			var splits = CSMFrustumSplit (splitCount, logarithmicWeight);
			var result = new ViewingFrustum[splitCount];
			for (int i = 0; i < splitCount; i++)
			{
				var nearPlane = _frustum.XYPlaneAtZ (splits[i]);
				result[i] = new ViewingFrustum (_frustum.Kind, nearPlane.X, nearPlane.Z, nearPlane.Y,
					nearPlane.W, splits[i], splits[i + 1]);
			}
			return result;
		}
	}
}	