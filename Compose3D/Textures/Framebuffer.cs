namespace Compose3D.Textures
{
	using GLTypes;
	using System;
	using OpenTK.Graphics.OpenGL;

	public class Framebuffer : GLObject
	{
		private FramebufferTarget _target;
		internal int _glFramebuffer;

		private Framebuffer (FramebufferTarget target)
		{
			_target = target;
			_glFramebuffer = GL.GenFramebuffer ();
		}

		public override void Use ()
		{
			GL.BindFramebuffer (_target, _glFramebuffer);
		}

		public override void Release ()
		{
			GL.BindFramebuffer (_target, 0);
		}

		private void BindRenderbuffer (Action action)
		{
			Use ();
			try
			{
				action ();
				var status = GL.CheckFramebufferStatus (_target);
				if (status != FramebufferErrorCode.FramebufferComplete)
					throw new GLError ("Could not initialize framebuffer. ErrorCode: " + status.ToString ());
			}
			finally
			{
				Release ();
			}
		}

		public void AddTexture (FramebufferAttachment attachment, Texture texture)
		{
			BindRenderbuffer (() => GL.FramebufferTexture (_target, attachment, texture._glTexture, 0));
		}

		public void AddRenderbuffer (FramebufferAttachment attachment, RenderbufferStorage storage, int width, int height)
		{
			BindRenderbuffer (() =>
			{
				var rb = GL.GenRenderbuffer ();
				GL.BindRenderbuffer (RenderbufferTarget.Renderbuffer, rb);
				GL.RenderbufferStorage (RenderbufferTarget.Renderbuffer, storage, width, height);
				GL.FramebufferRenderbuffer (_target, attachment, RenderbufferTarget.Renderbuffer, rb);
			});
		}
	}
}

