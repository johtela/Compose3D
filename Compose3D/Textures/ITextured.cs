namespace Compose3D.Textures
{
	using Maths;
	using Geometry;
	using OpenTK;
	using System.Linq;
	using DataStructures;
	using OpenTK.Graphics.OpenGL4;

	/// <summary>
	/// Interface for textured vertices.
	/// </summary>
    public interface ITextured
    {
        /// <summary>
        /// The texture coordinate of the vertex.
        /// </summary>
        Vec2 texturePos { get; set; }
    }

	/// <summary>
	/// Interface for vertices that have a normal map. The tangent vector is 
	/// needed for calculating the tangent space transformation matrix.
	/// </summary>
	public interface INormalMapped : ITextured
	{
		/// <summary>
		/// The tangent vector of the vertex.
		/// </summary>
		Vec3 tangent { get; set; }
	}

    public static class TexturePos
    {
        public static Vec2 BottomLeft
        {
            get { return new Vec2 (0f, 0f); }
        }

        public static Vec2 BottomRight
        {
            get { return new Vec2 (1f, 0f); }
        }

        public static Vec2 TopLeft
        {
            get { return new Vec2 (0f, 1f); }
        }

        public static Vec2 TopRight
        {
            get { return new Vec2 (1f, 1f); }
        }
    }

    public static class TextureHelpers
    {
		/// <summary>
		/// Set the texture coordinates for all the vertices facing towards the viewer when
		/// vertices are transformed according to a given projection matrix. 
		/// </summary>
		/// <description>>
		/// When applying textures to complex geometries it can be tricky to set the texture coordinates
		/// for faces. The idea of texture projection is used to make this process easier. A texture is
		/// applied to the geometry that is viewed from a specified angle and using typically an
		/// orthographic projection. This way it is easier to select the faces you want to apply the 
		/// texture. It is also possible to devise more complex ways of setting the texture coordinates
		/// by doing the projection multiple times from different angles. By that you can build for example 
		/// a cylindrical or spherical projection and apply a panoramic texture, i.e. a texture that exceeds 
		/// the given field of view.
		/// </description>
		/// <param name="geometry">Geometry to which the texture coordinates are applied.</param>
		/// <param name="projection">The projection matrix used to orient and project the geometry for
		/// applying the texture coordinates.</param>
		/// <param name="minCosAngle">The cosine of the minimum angle between the inverse viewing vector 
		/// ([0, 0, 1] in the projected coordinate system) and the normal of the vertex for which the
		/// texture positions are set. This parameter can be used to fine tune which faces are filtered
		/// out from the process.</param>
		/// <param name="minPos">The minimum texture position that is set. I.e. the lower left corner
		/// of the applied texture viewport.</param>
		/// <param name="maxPos">The maximum texture position that is set. Corresponds to the top right corner
		/// of the applied texture viewport.</param>
		/// <typeparam name="V">The vertex type. Must implement the IVertex and ITextured interfaces.</typeparam>
		public static void ApplyTextureCoordinates<V> (this Geometry<V> geometry, Mat4 projection, float minCosAngle,
			 Vec2 minPos, Vec2 maxPos) where V : struct, IVertex, ITextured
        {
            var projected = geometry.Transform (projection);
            var range = maxPos - minPos;
			var invView = new Vec3 (0f, 0f, 1f);
			var bbox = Aabb<Vec3>.FromPositions (projected.Vertices.Where (
				v => v.normal.Dot (invView) >= minCosAngle).Select (v => v.position));
            var scaleX = range.X / bbox.Size.X;
            var scaleY = range.Y / bbox.Size.Y;
            for (int i = 0; i < geometry.Vertices.Length; i++)
            {
                var pv = projected.Vertices[i];
				if (pv.normal.Dot (invView) >= minCosAngle)
					geometry.Vertices[i].texturePos = new Vec2 (
                        (pv.position.X - bbox.Left) * scaleX + minPos.X,
                        (pv.position.Y - bbox.Bottom) * scaleY + minPos.Y);
            }
        }

		public static void ApplyTextureFront<V> (this Geometry<V> geometry, float minCosAngle, Vec2 minPos, Vec2 maxPos)
			 where V : struct, IVertex, ITextured
		{
			ApplyTextureCoordinates<V> (geometry, new Mat4 (1f), minCosAngle, minPos, maxPos);
		}

		public static void ApplyTextureBack<V> (this Geometry<V> geometry, float minCosAngle, Vec2 minPos, Vec2 maxPos)
			 where V : struct, IVertex, ITextured
		{
			ApplyTextureCoordinates<V> (geometry, Mat.RotationY<Mat4> (MathHelper.Pi), minCosAngle, minPos, maxPos);
		}

		public static void ApplyTextureLeft<V> (this Geometry<V> geometry, float minCosAngle, Vec2 minPos, Vec2 maxPos)
			 where V : struct, IVertex, ITextured
		{
			ApplyTextureCoordinates<V> (geometry, Mat.RotationY<Mat4> (MathHelper.PiOver2), minCosAngle, minPos, maxPos);
		}

		public static void ApplyTextureRight<V> (this Geometry<V> geometry, float minCosAngle, Vec2 minPos, Vec2 maxPos)
			 where V : struct, IVertex, ITextured
		{
			ApplyTextureCoordinates<V> (geometry, Mat.RotationY<Mat4> (-MathHelper.PiOver2), minCosAngle, minPos, maxPos);
		}

		public static void ApplyTextureTop<V> (this Geometry<V> geometry, float minCosAngle, Vec2 minPos, Vec2 maxPos)
			 where V : struct, IVertex, ITextured
		{
			ApplyTextureCoordinates<V> (geometry, Mat.RotationX<Mat4> (MathHelper.PiOver2), minCosAngle, minPos, maxPos);
		}
		
		public static void ApplyTextureBottom<V> (this Geometry<V> geometry, float minCosAngle, Vec2 minPos, Vec2 maxPos)
			 where V : struct, IVertex, ITextured
		{
			ApplyTextureCoordinates<V> (geometry, Mat.RotationX<Mat4> (-MathHelper.PiOver2), minCosAngle, minPos, maxPos);
		}

		public static void UpdateTangents<V> (this Geometry<V> geometry, BeginMode primitive)
			 where V : struct, IVertex, INormalMapped
		{
			for (int i = 0; i < geometry.Vertices.Length; i++)
				geometry.Vertices[i].tangent = new Vec3 (0f);
			var edges = geometry.GetEdges (primitive).ToArray ();
			for (int i = 0; i < edges.Length; i+= 3)
			{
				var i1 = edges[i].Index1;
				var i2 = edges[i + 1].Index1;
				var i3 = edges[i + 2].Index1;
				var tangent = CalculateTangent (
					geometry.Vertices[i2].position - geometry.Vertices[i1].position,
					geometry.Vertices[i3].position - geometry.Vertices[i1].position,
					geometry.Vertices[i2].texturePos - geometry.Vertices[i1].texturePos,
					geometry.Vertices[i3].texturePos - geometry.Vertices[i1].texturePos).Normalized;
				geometry.Vertices[i1].tangent += tangent;
				geometry.Vertices[i2].tangent += tangent;
				geometry.Vertices[i3].tangent += tangent;
			}
			for (int i = 0; i < geometry.Vertices.Length; i++)
				geometry.Vertices[i].tangent = geometry.Vertices[i].tangent.Normalized;
		}

		private static Vec3 CalculateTangent (Vec3 edge1, Vec3 edge2, Vec2 deltaUV1, Vec2 deltaUV2)
		{
			var f = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV2.X * deltaUV1.Y);
			return new Vec3 (
				f * (deltaUV2.Y * edge1.X - deltaUV1.Y * edge2.X),
				f * (deltaUV2.Y * edge1.Y - deltaUV1.Y * edge2.Y),
				f * (deltaUV2.Y * edge1.Z - deltaUV1.Y * edge2.Z)).Normalized;
		}
	}
}
