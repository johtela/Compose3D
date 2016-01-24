namespace Compose3D.Textures
{
	using System;
	using OpenTK.Graphics.OpenGL;

	public class Framebuffer
	{
		private Texture _texture;
		internal int _glFramebuffer;
		
		public Framebuffer (Texture texture)
		{
			_texture = texture;
			_glFramebuffer = GL.GenFramebuffer ();
		}
		
		
	}
}

