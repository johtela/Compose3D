namespace Compose3D.Shaders
{
	using Extensions;
	using Maths;
	using GLTypes;
	using Geometry;
	using SceneGraph;
	using Textures;

	public class TransformUniforms
	{
		public Uniform<Mat4> modelViewMatrix;
		public Uniform<Mat4> perspectiveMatrix;
		public Uniform<Mat3> normalMatrix;
	}

	public class BasicUniforms
	{
		public TransformUniforms Transforms;
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

	public class WindowUniforms
	{
		public Uniform<Sampler2D> textureMap;
	}
}