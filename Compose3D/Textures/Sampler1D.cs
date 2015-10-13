namespace Compose3D.Textures
{
    using Arithmetics;
	using GLTypes;

	[GLType ("sampler1D")]
    public struct Sampler1D
    {
		[GLFunction ("textureSize ({0})")]
		public int Size (int lod)
		{
			return default (int);
		}
	}
}