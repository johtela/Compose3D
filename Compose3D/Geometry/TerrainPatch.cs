namespace Compose3D.Geometry
{
	using System;
	using Maths;

	public class TerrainPatch<V> where V : struct, IVertex
	{
		public readonly Vec2i Start;
		public readonly Vec2i Size;
		
		public TerrainPatch (Vec2i start, Vec2i size)
		{
			Start = start;
			Size = size;
		}
		


	}
}

