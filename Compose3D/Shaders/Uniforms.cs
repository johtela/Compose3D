namespace Compose3D.Shaders
{
	using Extensions;
	using Compose3D.Maths;
	using Compose3D.GLTypes;
	using Compose3D.SceneGraph;

	public class BasicUniforms
	{
		public Uniform<Mat4> worldMatrix;
		public Uniform<Mat4> perspectiveMatrix;
		public Uniform<Mat3> normalMatrix;
		public Uniform<Lighting.GlobalLight> globalLighting;
		public Uniform<Lighting.DirectionalLight> directionalLight;

		public void Initialize (Program program, SceneGraph scene)
		{
			using (program.Scope ())
			{
				scene.Root.Traverse ()
					.WhenOfType<SceneNode, GlobalLighting> (globalLight =>
						globalLighting &= new Lighting.GlobalLight ()
						{
							ambientLightIntensity = globalLight.AmbientLightIntensity,
							maxintensity = globalLight.MaxIntensity,
							inverseGamma = 1f / globalLight.GammaCorrection
						})
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
