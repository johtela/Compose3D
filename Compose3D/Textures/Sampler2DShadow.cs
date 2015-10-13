namespace Compose3D.Textures
{
	using System.Collections.Generic;
    using Arithmetics;
	using GLTypes;
	using OpenTK.Graphics.OpenGL;

	[GLType ("sampler2DShadow")]
    public class Sampler2DShadow : Sampler
    {
    	public Sampler2DShadow () : base () {}

    	public Sampler2DShadow (int texUnit, IDictionary<SamplerParameterName, object> parameters) 
    		: base (texUnit, parameters) {}
    	
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
