namespace Compose3D.Textures
{
	using Maths;
	using GLTypes;

	[GLType ("sampler2DArray")]
    public class Sampler2DArray : Sampler
    {
    	public Sampler2DArray () : base () {}

    	public Sampler2DArray (int texUnit) 
    		: base (texUnit) {}
    	
		[GLFunction ("textureSize ({0})")]
		public Vec3i Size (int lod)
		{
			return default (Vec3i);
		}


		[GLFunction ("texture ({0})")]
		public Vec4 Texture (Vec3 pos)
		{
			return default (Vec4);
		}

		[GLFunction ("texture ({0})")]
		public Vec4 Texture (Vec3 pos, float bias)
		{
			return default (Vec4);
		}

	}
}
