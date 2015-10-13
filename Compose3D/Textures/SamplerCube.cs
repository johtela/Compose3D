namespace Compose3D.Textures
{
	using System.Collections.Generic;
    using Arithmetics;
	using GLTypes;
	using OpenTK.Graphics.OpenGL;

	[GLType ("samplerCube")]
    public class SamplerCube : Sampler
    {
    	public SamplerCube () : base () {}

    	public SamplerCube (int texUnit, IDictionary<SamplerParameterName, object> parameters) 
    		: base (texUnit, parameters) {}
    	
		[GLFunction ("textureSize ({0})")]
		public Vec2i Size (int lod)
		{
			return default (Vec2i);
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
