namespace Compose3D.Filters
{
	using System;
	using System.Collections.Generic;
	using Geometry;
	using GLTypes;
	using Maths;
	using Reactive;
	using Shaders;
	using Textures;
	using OpenTK.Graphics.OpenGL4;
	using Filter = Reactive.Reaction<System.Tuple<Textures.Texture, Textures.Texture>>;

	public class TextureFilter
	{
		private Program _program;
		private TextureUniforms _uniforms;
		private Framebuffer _framebuffer;
		private VBO<TexturedVertex> _vertexBuffer;
		private VBO<int> _indexBuffer;

		private TextureFilter (Program program)
		{
			_program = program;
			_uniforms = new TextureUniforms (_program, new Sampler2D (0).LinearFiltering ()
				.ClampToEdges (Axes.X | Axes.Y));
			
			_framebuffer = new Framebuffer (FramebufferTarget.Framebuffer);
			
			var rectangle = Quadrilateral<TexturedVertex>.Rectangle (2f, 2f);
			rectangle.ApplyTextureFront (1f, new Vec2 (0f), new Vec2 (1f));
			_vertexBuffer = new VBO<TexturedVertex> (rectangle.Vertices, BufferTarget.ArrayBuffer);
			_indexBuffer = new VBO<int> (rectangle.Indices, BufferTarget.ElementArrayBuffer);
		}

		public static Filter Renderer (Program program)
		{
			var filter = new TextureFilter (program);

			return React.By<Tuple<Texture, Texture>> (t => 
				filter._program.DrawElements (PrimitiveType.Triangles, filter._vertexBuffer, 
					filter._indexBuffer))
				.BindSamplers (t => new Dictionary<Sampler, Texture> ()
				{
					{ (!filter._uniforms.textureMap), t.Item1 }
				})
				.Viewport (t => t.Item2.Size)
				.Culling ()
				.Program (program)
				.Texture (t => t.Item1)
				.FramebufferTexture (t => Tuple.Create (filter._framebuffer, 
					FramebufferAttachment.ColorAttachment0, t.Item2));
		}
	}
}
