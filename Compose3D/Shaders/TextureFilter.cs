namespace Compose3D.Shaders
{
	using System;
	using System.Linq.Expressions;
	using Geometry;
	using GLTypes;
	using Maths;
	using Textures;
	using OpenTK.Graphics.OpenGL;

	public class TextureFilter
	{
		public class TexturedFragment : Fragment, IFragmentTexture<Vec2>
		{
			public Vec2 fragTexturePos { get; set; }
		}

		private Texture _input;
		private Texture _output;
		private Program _program;
		private TextureUniforms _uniforms;
		private Framebuffer _framebuffer;
		private VBO<TexturedVertex> _vertexBuffer;
		private VBO<int> _indexBuffer;

		private TextureFilter (Texture input, GLShader _filter)
		{
			_input = input;
			_program = new Program (VertexShaders.PassthroughTexture<TexturedVertex, TexturedFragment> (),
				_filter);
			_uniforms = new TextureUniforms (_program, new Sampler2D (0).LinearFiltering ()
				.ClampToEdges (Axes.X | Axes.Y));
			
			var size = _input.Size;
			_framebuffer = new Framebuffer (FramebufferTarget.Framebuffer);
			_output = new Texture (_input.Target, _input.PixelInternalFormat, size.X, size.Y, 
				_input.PixelFormat, _input.PixelType, IntPtr.Zero);
			_framebuffer.AddTexture (FramebufferAttachment.ColorAttachment0, _output);
			
			var rectangle = Quadrilateral<TexturedVertex>.Rectangle (2f, 2f);
			rectangle.ApplyTextureFront (1f, new Vec2 (0f), new Vec2 (1f));
			_vertexBuffer = new VBO<TexturedVertex> (rectangle.Vertices, BufferTarget.ArrayBuffer);
			_indexBuffer = new VBO<int> (rectangle.Indices, BufferTarget.ElementArrayBuffer);
		}

		public Texture Output
		{
			get { return _output; }
		}
		
		public void Apply ()
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
		
		public static TextureFilter Create<T> (Texture input, Expression<Func<Shader<T>>> _filter)
		{
			return new TextureFilter (input, GLShader.Create (ShaderType.FragmentShader, _filter));
		}
	}
}
