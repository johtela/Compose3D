namespace Compose3D.Textures
{
	using GLTypes;
	using System;
	using OpenTK.Graphics.OpenGL;

	public class Framebuffer : GLObject
	{
		private FramebufferTarget _target;
		private int _prevFrameBuffer;
		internal int _glFramebuffer;

		public Framebuffer (FramebufferTarget target)
		{
			_target = target;
			_glFramebuffer = GL.GenFramebuffer ();
		}

		public override void Use ()
		{
			_prevFrameBuffer = GL.GetInteger (GetPName.DrawFramebufferBinding);
			GL.BindFramebuffer (_target, _glFramebuffer);
		}

		public override void Release ()
		{
			GL.BindFramebuffer (_target, _prevFrameBuffer);
		}

		private void CheckStatus ()
		{
			var status = GL.CheckFramebufferStatus (_target);
			if (status != FramebufferErrorCode.FramebufferComplete)
				throw new GLError ("Could not initialize framebuffer. ErrorCode: " + status.ToString ());
		}

		public void AddTexture (FramebufferAttachment attachment, Texture texture)
		{
			using (texture.Scope ())
				ChangeTextureAttachment (attachment, texture._target, texture._glTexture);
		}

		public void RemoveTexture (FramebufferAttachment attachment, Texture texture)
		{
			ChangeTextureAttachment (attachment, texture._target, 0);
		}

		private void ChangeTextureAttachment (FramebufferAttachment attachment, TextureTarget textureTarget, 
			int glTexture)
		{
			using (Scope ())
			{
				switch (textureTarget)
				{
					case TextureTarget.Texture1D:
						GL.FramebufferTexture1D (_target, attachment, textureTarget, glTexture, 0);
						break;
					case TextureTarget.Texture2D:
						GL.FramebufferTexture2D (_target, attachment, textureTarget, glTexture, 0);
						break;
					default:
						GL.FramebufferTexture (_target, attachment, glTexture, 0);
						break;
				}
				if (glTexture != 0)
					CheckStatus ();
			}
		}

		public void AddRenderbuffer (FramebufferAttachment attachment, RenderbufferStorage storage, int width, int height)
		{
			using (Scope ())
			{
				var rb = GL.GenRenderbuffer ();
				GL.BindRenderbuffer (RenderbufferTarget.Renderbuffer, rb);
				GL.RenderbufferStorage (RenderbufferTarget.Renderbuffer, storage, width, height);
				GL.FramebufferRenderbuffer (_target, attachment, RenderbufferTarget.Renderbuffer, rb);
				CheckStatus ();
			}
		}
	}
}