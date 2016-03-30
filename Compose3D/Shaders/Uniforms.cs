namespace Compose3D.Shaders
{
	using System;
	using Extensions;
	using Maths;
	using GLTypes;
	using Geometry;
	using SceneGraph;
	using Textures;

	public abstract class Uniforms
	{
		public Uniforms (Program program)
		{
			foreach (var field in GetType ().GetUniforms ())
				field.SetValue (this, Activator.CreateInstance (field.FieldType, program, field));
		}
	}
	
	public class TransformUniforms : Uniforms
	{
		public Uniform<Mat4> modelViewMatrix;
		public Uniform<Mat4> perspectiveMatrix;
		public Uniform<Mat3> normalMatrix;
		
		public TransformUniforms (Program program) : base (program)	{ }
	}

	public class LightingUniforms : Uniforms
	{
		public Uniform<Lighting.GlobalLight> globalLighting;
		public Uniform<Lighting.DirectionalLight> directionalLight;

		public LightingUniforms (Program program, SceneGraph scene)
			: base (program)
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

	public class TextureUniforms : Uniforms
	{
		public Uniform<Sampler2D> textureMap;

		public TextureUniforms (Program program, Sampler2D sampler) : base (program) 
		{
			textureMap &= sampler;
		}
	}
}