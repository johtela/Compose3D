namespace Compose3D.Shaders
{
	using System;
	using System.Linq;
	using Maths;
	using GLTypes;
	using Geometry;
	using SceneGraph;
	using Textures;
	using OpenTK.Graphics.OpenGL;

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
		public Uniform<Mat4> lightSpaceMatrix;
		
		public TransformUniforms (Program program) : base (program)	{ }

		public void UpdateModelViewAndNormalMatrices (Mat4 modelView)
		{
			modelViewMatrix &= modelView;
			normalMatrix &= new Mat3 (modelView).Inverse.Transposed;
		}

		public void UpdateLightSpaceMatrix (Mat4 lightSpace)
		{
			lightSpaceMatrix &= lightSpace;
		}
	}

	public class LightingUniforms : Uniforms
	{
		public Uniform<Lighting.GlobalLight> globalLighting;
		public Uniform<Lighting.DirectionalLight> directionalLight;
		public Uniform<Sampler2D> shadowMap;

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
				shadowMap &= new Sampler2D (0, new SamplerParams ()
				{
					{ SamplerParameterName.TextureMagFilter, All.Linear },
					{ SamplerParameterName.TextureMinFilter, All.Linear },
					{ SamplerParameterName.TextureWrapR, All.ClampToEdge },
					{ SamplerParameterName.TextureWrapS, All.ClampToEdge },
					{ SamplerParameterName.TextureWrapT, All.ClampToEdge }
				});
			}
		}

		public void UpdateDirectionalLight (Camera camera)
		{
			var dirLight = camera.Graph.Root.Traverse ().OfType<DirectionalLight> ().First ();
			directionalLight &= new Lighting.DirectionalLight ()
			{
				direction = dirLight.DirectionInCameraSpace (camera),
				intensity = dirLight.Intensity
			};
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