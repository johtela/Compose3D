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

	[StructLayout(LayoutKind.Sequential)]
	public struct Vertex : IVertex, IVertexInitializer<Vertex>, IVertexColor<Vec3>, ITextured
	{
		internal Vec3 position;
        internal Vec3 normal;
        internal Vec3 diffuseColor;
        internal Vec3 specularColor;
		internal Vec2 texturePos;
        internal float shininess;
        [OmitInGlsl]
        internal int tag;

		Vec3 IPositional<Vec3>.Position
		{
			get { return position; }
			set { position = value; }
		}

		Vec3 IDiffuseColor<Vec3>.Diffuse
		{
			get { return diffuseColor; }
			set { diffuseColor = value; }
		}

		Vec3 ISpecularColor<Vec3>.Specular
		{
			get { return specularColor; }
			set { specularColor = value; }
		}

		float ISpecularColor<Vec3>.Shininess
        {
            get { return shininess; }
            set { shininess = value; }
        }

        Vec3 IPlanar<Vec3>.Normal
		{
			get { return normal; }
			set 
			{
				if (float.IsNaN (value.X) || float.IsNaN (value.Y) || float.IsNaN (value.Z))
					throw new ArgumentException ("Normal component NaN");
				normal = value; 
			}
		}

		Vec2 ITextured.TexturePos
		{
			get { return texturePos; }
			set { texturePos = value; }
		}

        int IVertex.Tag
		{
			get { return tag; }
			set { tag = value; }
		}

		void IVertexInitializer<Vertex>.Initialize (ref Vertex vertex)
		{
			vertex.texturePos = new Vec2 (float.PositiveInfinity);
		}

		public override string ToString ()
		{
			return string.Format ("[Vertex: Position={0}, DiffuseColor={1}, SpecularColor={2}, Normal={3}, Tag={4}]", 
				position, diffuseColor, specularColor, normal, tag);
		}
	}

	public static class ExampleShaders
	{
		public class Uniforms
		{
			internal Uniform<Mat4> worldMatrix;
			internal Uniform<Mat4> perspectiveMatrix;
			internal Uniform<Mat3> normalMatrix;
			internal Uniform<Lighting.GlobalLight> globalLighting;
			internal Uniform<Lighting.DirectionalLight> directionalLight;
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
					samplerNo == 1 ? TextureColor ((!u.samplers)[1], f.texturePosition - new Vec2 (10f)) :
					samplerNo == 2 ? TextureColor ((!u.samplers)[2], f.texturePosition - new Vec2 (20f)) :
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

		public static GLShader ShadowFragmentShader ()
		{
			return GLShader.Create
			(
				ShaderType.FragmentShader, () =>

				from f in Shader.Inputs<Fragment> ()
				select new
				{
					fragmentDepth = f.gl_Position.Z
				}
			);
		}
	}
}

