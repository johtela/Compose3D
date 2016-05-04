namespace Compose3D.Shaders
{
	using System;
	using System.Collections.Generic;
	using Geometry;
	using GLTypes;
	using Maths;
	using Reactive;
	using Textures;
	using OpenTK.Graphics.OpenGL;

	public class TextureFilter
	{
		public class TexturedFragment : Fragment, IFragmentTexture<Vec2>
		{
			public Vec2 fragTexturePos { get; set; }
		}

		private Program _program;
		private TextureUniforms _uniforms;
		private Framebuffer _framebuffer;
		private VBO<TexturedVertex> _vertexBuffer;
		private VBO<int> _indexBuffer;
		private Texture _input;
		private Texture _output;

		private TextureFilter (Program program)
		{
			_program = program;
			_uniforms = new TextureUniforms (_program, new Sampler2D (0).LinearFiltering ()
				.ClampToEdges (Axes.X | Axes.Y));
			
			_framebuffer = new Framebuffer (FramebufferTarget.Framebuffer);
			_framebuffer.AddTexture (FramebufferAttachment.ColorAttachment0, _output);
			
			var rectangle = Quadrilateral<TexturedVertex>.Rectangle (2f, 2f);
			rectangle.ApplyTextureFront (1f, new Vec2 (0f), new Vec2 (1f));
			_vertexBuffer = new VBO<TexturedVertex> (rectangle.Vertices, BufferTarget.ArrayBuffer);
			_indexBuffer = new VBO<int> (rectangle.Indices, BufferTarget.ElementArrayBuffer);
		}

		public static Reaction<Tuple<Texture, Texture>> Renderer (Program program)
		{
			var filter = new TextureFilter (program);

			var render = React.By<Tuple<Texture, Texture>> (filter.Run);

			//return render.Select (t => new Dictionary<Sampler, Texture> ()
			//	{
			//		{ (!filter._uniforms.textureMap), t.Item1 }
			//	});
			return null;
		}

		public void Run ()
		{
			using (_framebuffer.Scope ())
			using (_output.Scope ())
			using (_program.Scope ())
			{
				GL.Enable (EnableCap.CullFace);
				GL.CullFace (CullFaceMode.Back);
				GL.FrontFace (FrontFaceDirection.Cw);
				GL.Disable (EnableCap.DepthTest);
				GL.Disable (EnableCap.Blend);
				
				var size = _output.Size;
				GL.Viewport (new System.Drawing.Size (size.X, size.Y));
			
				(!_uniforms.textureMap).Bind (_input);
				_program.DrawElements (PrimitiveType.Triangles, _vertexBuffer, _indexBuffer);
				(!_uniforms.textureMap).Unbind (_input);
			}
		}
	}
}
