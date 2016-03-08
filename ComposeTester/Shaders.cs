namespace ComposeTester
{
	using Compose3D.Maths;
	using Compose3D.Geometry;
	using Compose3D.GLTypes;
	using Compose3D.Shaders;
    using Compose3D.Textures;
	using OpenTK.Graphics.OpenGL;
	using System;
	using System.Linq;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public struct PathNode : IPositional<Vec3>, IDiffuseColor<Vec3>
	{
		internal Vec3 position;
		internal Vec3 color;

		Vec3 IPositional<Vec3>.Position
		{
			get { return position; }
			set { position = value; }
		}

		Vec3 IDiffuseColor<Vec3>.Diffuse
		{
			get { return color; }
			set { color = value; }
		}

		public override string ToString ()
		{
			return string.Format ("PathNode: Position={0}, Diffuse={1}", position, color);
		}
	}

	public static class ExampleShaders
	{
		public class TerrainUniforms
		{
			internal Uniform<Mat4> worldMatrix;
			internal Uniform<Mat4> perspectiveMatrix;
			internal Uniform<Mat3> normalMatrix;
			internal Uniform<Lighting.GlobalLight> globalLighting;
			internal Uniform<Lighting.DirectionalLight> directionalLight;			
		}
		
		public class Uniforms : TerrainUniforms
		{
			[GLArray (4)] internal Uniform<Lighting.PointLight[]> pointLights;
			[GLArray (4)] internal Uniform<Sampler2D[]> samplers;
		}

		public class ShadowUniforms
		{
			internal Uniform<Mat4> mvpMatrix;
		}

		public static readonly Func<Sampler2D, Vec2, Vec3> TextureColor =
			GLShader.Function
			(
				() => TextureColor,

				(Sampler2D sampler, Vec2 texturePos) =>
					sampler.Texture (texturePos)[Coord.x, Coord.y, Coord.z]
			);

		public static GLShader VertexShader ()
		{
			return GLShader.Create 
			(
				ShaderType.VertexShader, () =>

				from v in Shader.Inputs<Vertex> ()
				from u in Shader.Uniforms<Uniforms> ()
				let worldPos = !u.worldMatrix * new Vec4 (v.position, 1f)
				select new TexturedFragment ()
				{
					gl_Position = !u.perspectiveMatrix * worldPos,
					vertexPosition = worldPos[Coord.x, Coord.y, Coord.z],
					vertexNormal = (!u.normalMatrix * v.normal).Normalized,
					vertexDiffuse = v.diffuseColor,
					vertexSpecular = v.specularColor,
					vertexShininess = v.shininess,
					texturePosition = v.texturePos
				}
			);
		}

		public static GLShader TerrrainVertexShader ()
		{
			return GLShader.Create 
				(
					ShaderType.VertexShader, () =>

					from v in Shader.Inputs<TerrainVertex> ()
					from u in Shader.Uniforms<TerrainUniforms> ()
					let worldPos = !u.worldMatrix * new Vec4 (v.position, 1f)
					select new DiffuseFragment ()
					{
						gl_Position = !u.perspectiveMatrix * worldPos,
						vertexPosition = worldPos[Coord.x, Coord.y, Coord.z],
						vertexNormal = (!u.normalMatrix * v.normal).Normalized,
						vertexDiffuse = v.diffuseColor,
					}
				);
		}
		
		public static GLShader ShadowVertexShader ()
		{
			return GLShader.Create
			(
				ShaderType.VertexShader, () =>

				from v in Shader.Inputs<Vertex> ()
				from u in Shader.Uniforms<ShadowUniforms> ()
				select new Fragment ()
				{
					gl_Position = !u.mvpMatrix * new Vec4 (v.position, 1f)
				}
			);
		}

		public static GLShader FragmentShader ()
		{
			Lighting.Use ();
			return GLShader.Create 
			(
				ShaderType.FragmentShader, () =>

				from f in Shader.Inputs<TexturedFragment> ()
				from u in Shader.Uniforms<Uniforms> ()
				let samplerNo = (f.texturePosition.X / 10f).Truncate ()
				let fragDiffuse =
					samplerNo == 0 ? TextureColor ((!u.samplers)[0], f.texturePosition) :
					//samplerNo == 1 ? TextureColor ((!u.samplers)[1], f.texturePosition - new Vec2 (10f)) :
					//samplerNo == 2 ? TextureColor ((!u.samplers)[2], f.texturePosition - new Vec2 (20f)) :
					samplerNo == 3 ? TextureColor ((!u.samplers)[3], f.texturePosition - new Vec2 (30f)) :
					f.vertexDiffuse
				let diffuse = Lighting.DirectionalLightIntensity (!u.directionalLight, f.vertexNormal) * fragDiffuse
				let specular = (!u.pointLights).Aggregate
				(
					new Vec3 (0f),
					(spec, pl) =>
						pl.intensity == new Vec3 (0f) ?
							spec :
							spec + Lighting.PointLightIntensity (pl, f.vertexPosition, f.vertexNormal, fragDiffuse,
								f.vertexSpecular, f.vertexShininess)
				)
				select new 
				{
					outputColor = Lighting.GlobalLightIntensity (!u.globalLighting, fragDiffuse, diffuse + specular)
				}
			);
		}

		public static GLShader TerrainFragmentShader ()
		{
			Lighting.Use ();
			return GLShader.Create 
				(
					ShaderType.FragmentShader, () =>

					from f in Shader.Inputs<DiffuseFragment> ()
					from u in Shader.Uniforms<TerrainUniforms> ()
					let diffuse = Lighting.DirectionalLightIntensity (!u.directionalLight, f.vertexNormal) * f.vertexDiffuse
					select new 
					{
						outputColor = Lighting.GlobalLightIntensity (!u.globalLighting, diffuse * 3f, new Vec3 (0f))
					}
				);
		}
		
		public static GLShader ShadowFragmentShader ()
		{
			return GLShader.Create
			(
				ShaderType.FragmentShader, () =>

				from f in Shader.Inputs<Fragment> ()
				select new
				{
					fragmentDepth = f.gl_FragCoord.Z
				}
			);
		}
	}
}

