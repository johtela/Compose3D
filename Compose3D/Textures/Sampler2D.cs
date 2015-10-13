namespace Compose3D.Textures
{
    using Arithmetics;
	using GLTypes;

	[GLType ("sampler2D")]
    public struct Sampler2D
    {
		[GLFunction ("textureSize ({0})")]
		public Vec2i Size (int lod)
		{
			return default (Vec2i);
		}
	}
}