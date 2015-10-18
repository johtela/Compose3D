
namespace Compose3D.Textures
{
	using System.Collections.Generic;
    using Arithmetics;
	using GLTypes;
	using OpenTK.Graphics.OpenGL;

	[GLType ("sampler1D")]
    public class Sampler1D : Sampler
    {
    	public Sampler1D () : base () {}

    	public Sampler1D (int texUnit, SamplerParams parameters) 
    		: base (texUnit, parameters) {}
    	
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
