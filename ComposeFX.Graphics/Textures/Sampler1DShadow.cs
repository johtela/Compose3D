namespace ComposeFX.Graphics.Textures
{
	using Maths;
	using GLTypes;

	[GLType ("sampler1DShadow")]
    public class Sampler1DShadow : Sampler
    {
    	public Sampler1DShadow () : base () {}

    	public Sampler1DShadow (int texUnit) 
    		: base (texUnit) {}
    	
		[GLFunction ("textureSize ({0})")]
		public int Size (int lod)
		{
			return default (int);
		}
		[GLFunction ("texture ({0})")]
		public float Texture (Vec2 pos)
		{
			return default (float);
		}

		[GLFunction ("texture ({0})")]
		public float Texture (Vec2 pos, float bias)
		{
			return default (float);
		}
	}
}
