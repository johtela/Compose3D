namespace Compose3D.Textures
{
	using Maths;
	using GLTypes;

	[GLType ("sampler1DArray")]
    public class Sampler1DArray : Sampler
    {
    	public Sampler1DArray () : base () {}

    	public Sampler1DArray (int texUnit) 
    		: base (texUnit) {}
    	
		[GLFunction ("textureSize ({0})")]
		public int Size (int lod)
		{
			return default (int);
		}
		[GLFunction ("texture ({0})")]
		public Vec4 Texture (Vec2 pos)
		{
			return default (Vec4);
		}

		[GLFunction ("texture ({0})")]
		public Vec4 Texture (Vec2 pos, float bias)
		{
			return default (Vec4);
		}
	}
}
