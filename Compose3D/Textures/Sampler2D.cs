namespace Compose3D.Textures
{
	using Maths;
	using GLTypes;

	[GLType ("sampler2D")]
    public class Sampler2D : Sampler
    {
    	public Sampler2D () : base () {}

    	public Sampler2D (int texUnit) 
    		: base (texUnit) {}
    	
		[GLFunction ("textureSize ({0})")]
		public Vec2i Size (int lod)
		{
			return default (Vec2i);
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
