namespace Compose3D.Shaders
{
	using Compose3D.Maths;
	using Compose3D.GLTypes;

	[GLStruct ("DirectionalLight")]
	public struct DirectionalLight
	{
		public Vec3 intensity;
		public Vec3 direction;
	}

	[GLStruct ("PointLight")]
	public struct PointLight
	{
		public Vec3 position;
		public Vec3 intensity;
		public float linearAttenuation, quadraticAttenuation;
	}

	[GLStruct ("SpotLight")]
	public struct SpotLight
	{
		public PointLight pointLight;
		public Vec3 direction;
		public float cosSpotCutoff, spotExponent;
	}

	[GLStruct ("GlobalLight")]
	public struct GlobalLight
	{
		public Vec3 ambientLightIntensity;
		public float maxintensity;
		public float inverseGamma;
	}
}
