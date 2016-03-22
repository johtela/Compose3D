namespace Compose3D.Shaders
{
	using Extensions;
	using Compose3D.Maths;
	using Compose3D.GLTypes;
	using Compose3D.SceneGraph;

	public class BasicUniforms
	{
		public Uniform<Mat4> viewMatrix;
		public Uniform<Mat4> perspectiveMatrix;
		public Uniform<Mat3> normalMatrix;
		public Uniform<Lighting.GlobalLight> globalLighting;
		public Uniform<Lighting.DirectionalLight> directionalLight;

		public void Initialize (Program program, SceneGraph scene)
		{
			using (program.Scope ())
			{
				var gl = scene.GlobalLighting;
				if (gl != null)
				{
					globalLighting &= new Lighting.GlobalLight ()
					{
						ambientLightIntensity = gl.AmbientLightIntensity,
						maxintensity = gl.MaxIntensity,
						inverseGamma = 1f / gl.GammaCorrection
					};
				}
				scene.Root.Traverse ()
					.WhenOfType<SceneNode, DirectionalLight> (dirLight =>
						directionalLight &= new Lighting.DirectionalLight ()
						{
							direction = dirLight.Direction,
							intensity = dirLight.Intensity
						})
					.ToVoid ();
			}
		}
	}
}
