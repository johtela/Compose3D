
namespace Compose3D.Textures
{
	using System.Collections.Generic;
    using Arithmetics;
	using GLTypes;
	using OpenTK.Graphics.OpenGL;

	[GLType ("sampler3D")]
    public class Sampler3D : Sampler
    {
    	public Sampler3D () : base () {}

    	public Sampler3D (int texUnit, SamplerParams parameters) 
    		: base (texUnit, parameters) {}
    	
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
