namespace ComposeFX.Graphics.Textures
{
	using Maths;
	using GLTypes;

	[GLType ("sampler1D")]
    public class Sampler1D : Sampler
    {
    	public Sampler1D () : base () {}

    	public Sampler1D (int texUnit) 
    		: base (texUnit) {}
    	
		[GLFunction ("textureSize ({0})")]
		public int Size (int lod)
		{
			return default (int);
		}
		[GLFunction ("texture ({0})")]
		public Vec4 Texture (float pos)
		{
			return default (Vec4);
		}

		[GLFunction ("texture ({0})")]
		public Vec4 Texture (float pos, float bias)
		{
			return default (Vec4);
		}
	}
}
