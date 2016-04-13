namespace Compose3D.Shaders
{
	using System;
	using System.Linq.Expressions;
	using GLTypes;
	using Textures;
	using OpenTK.Graphics.OpenGL;

	public class TextureFilter<T>
	{
		private Texture _input;
		private Texture _output;
		private Program _program;
		private Framebuffer _framebuffer;

		public TextureFilter (Texture input, Expression<Func<Shader<T>>> _filter)
		{
			_input = input;
			_framebuffer = new Framebuffer (FramebufferTarget.Framebuffer);
			_program = new Program (VertexShaders.Passthrough<PositionalVertex, Fragment> (),
				GLShader.Create (ShaderType.FragmentShader, _filter));
			var size = _input.Size;
			_output = new Texture (TextureTarget.Texture2D, _input.PixelInternalFormat, size.X, size.Y, _input.PixelFormat,
				_input.PixelType, IntPtr.Zero);
			_framebuffer.AddTexture (FramebufferAttachment.ColorAttachment0, _output);
		}

		public Texture Output
		{
			get { return _output; }
		}
	}
}
