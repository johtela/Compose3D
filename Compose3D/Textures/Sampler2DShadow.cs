namespace Compose3D.Textures
{
	using Maths;
	using GLTypes;

	[GLType ("sampler2DShadow")]
    public class Sampler2DShadow : Sampler
    {
    	public Sampler2DShadow () : base () {}

    	public Sampler2DShadow (int texUnit) 
    		: base (texUnit) {}
    	
		[GLFunction ("textureSize ({0})")]
		public Vec2i Size (int lod)
		{
			return default (Vec2i);
		}
		[GLFunction ("texture ({0})")]
		public float Texture (Vec3 pos)
		{
			return default (float);
		}

		[GLFunction ("texture ({0})")]
		public float Texture (Vec3 pos, float bias)
		{
			return default (float);
		}
	}
}
