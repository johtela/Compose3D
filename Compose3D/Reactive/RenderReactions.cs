namespace Compose3D.Reactive
{
	using System;
	using System.Collections.Generic;
	using GLTypes;
	using Maths;
	using Textures;
	using OpenTK;
	using OpenTK.Graphics.OpenGL4;

	public static class Render
	{
		private static int GetState (GetPName parameter)
		{
			int value;
			GL.GetInteger (parameter, out value);
			return value;
		}

		public static Reaction<T> Culling<T> (this Reaction<T> render,
			Func<T, Tuple<CullFaceMode, FrontFaceDirection>> getParams)
		{
			return input =>
			{
				var oldCap = GL.IsEnabled (EnableCap.CullFace);
				var oldMode = GetState (GetPName.CullFaceMode);
				var oldFrontFace = GetState (GetPName.FrontFace);

				GL.Enable (EnableCap.CullFace);
				var pars = getParams (input);
				GL.CullFace (pars.Item1);
				GL.FrontFace (pars.Item2);
				var result = render (input);
				GL.FrontFace ((FrontFaceDirection)oldFrontFace);
				GL.CullFace ((CullFaceMode)oldMode);
				if (!oldCap)
					GL.Disable (EnableCap.CullFace);
				return result;
			};
		}

		public static Reaction<T> Culling<T> (this Reaction<T> render, CullFaceMode mode = CullFaceMode.Back,
			FrontFaceDirection frontFace = FrontFaceDirection.Cw)
		{
			return render.Culling (i => Tuple.Create (mode, frontFace));
		}

		public static Reaction<T> DepthTest<T> (this Reaction<T> render, Func<T, DepthFunction> getDepthFunc)
		{
			return input =>
			{
				var oldCap = GL.IsEnabled (EnableCap.DepthTest);
				var oldFunction = GetState (GetPName.DepthFunc);
				var oldMask = GetState (GetPName.DepthWritemask);

				GL.Enable (EnableCap.DepthTest);
				GL.DepthMask (true);
				GL.DepthFunc (getDepthFunc (input));
				var result = render (input);
				GL.DepthMask (oldMask != 0);
				GL.DepthFunc ((DepthFunction)oldFunction);
				if (!oldCap)
					GL.Disable (EnableCap.DepthTest);
				return result;
			};
		}

		public static Reaction<T> DepthTest<T> (this Reaction<T> render,
			DepthFunction depthFunction = DepthFunction.Less)
		{
			return render.DepthTest (i => depthFunction);
		}

		public static Reaction<T> Blending<T> (this Reaction<T> render,
			Func<T, Tuple<BlendingFactorSrc, BlendingFactorDest>> getParams)
		{
			return input =>
			{
				var oldCap = GL.IsEnabled (EnableCap.Blend);
				var oldSource = GetState (GetPName.BlendSrc);
				var oldDest = GetState (GetPName.BlendDst);

				GL.Enable (EnableCap.Blend);
				var pars = getParams (input);
				GL.BlendFunc (pars.Item1, pars.Item2);
				var result = render (input);
				GL.BlendFunc ((BlendingFactorSrc)oldSource, (BlendingFactorDest)oldDest);
				if (!oldCap)
					GL.Disable (EnableCap.Blend);
				return result;
			};
		}

		public static Reaction<T> Blending<T> (this Reaction<T> render,
			BlendingFactorSrc source = BlendingFactorSrc.SrcAlpha,
			BlendingFactorDest destination = BlendingFactorDest.OneMinusSrcAlpha)
		{
			return render.Blending (i => Tuple.Create (source, destination));
		}

		public static Reaction<T> DrawBuffer<T> (this Reaction<T> render, Func<T, DrawBufferMode> getMode)
		{
			return input =>
			{
				var oldMode = GetState (GetPName.DrawBuffer);
				GL.DrawBuffer (getMode (input));
				var result = render (input);
				GL.DrawBuffer ((DrawBufferMode)oldMode);
				return result;
			};
		}

		public static Reaction<T> DrawBuffer<T> (this Reaction<T> render, DrawBufferMode mode)
		{
			return render.DrawBuffer (i => mode);
		}

		public static Reaction<T> Program<T> (this Reaction<T> render, Func<T, GLProgram> getProgram)
		{
			return input =>
			{
				using (getProgram (input).Scope ())
					return render (input);
			};
		}

		public static Reaction<T> Program<T> (this Reaction<T> render, GLProgram program)
		{
			return render.Program (i => program);
		}

		public static Reaction<T> Texture<T> (this Reaction<T> render, Func<T, Texture> getTexture)
		{
			return input =>
			{
				using (getTexture (input).Scope ())
					return render (input);
			};
		}

		public static Reaction<T> Texture<T> (this Reaction<T> render, Texture texture)
		{
			return render.Texture (i => texture);
		}

		public static Reaction<T> Framebuffer<T> (this Reaction<T> render, Framebuffer framebuffer)
		{
			return input =>
			{
				framebuffer.Use ();
				var result = render (input);
				framebuffer.Release ();
				return result;
			};
		}

		public static Reaction<T> FramebufferTexture<T> (this Reaction<T> render,
			Func<T, Tuple<Framebuffer, FramebufferAttachment, Texture>> getFbTexture)
		{
			return input =>
			{
				bool result;
				var fbTex = getFbTexture (input);
				fbTex.Item1.Use ();
				fbTex.Item1.AddTexture (fbTex.Item2, fbTex.Item3);
				result = render (input);
				fbTex.Item1.RemoveTexture (fbTex.Item2, fbTex.Item3);
				fbTex.Item1.Release ();
				return result;
			};
		}

		public static Reaction<T> BindSamplers<T> (this Reaction<T> render,
			Func<T, IDictionary<Sampler, Texture>> getBindings)
		{
			return input =>
			{
				var bindings = getBindings (input);
				Sampler.Bind (bindings);
				var result = render (input);
				Sampler.Unbind (bindings);
				return result;
			};
		}

		public static Reaction<T> BindSamplers<T> (this Reaction<T> render, IDictionary<Sampler, Texture> bindings)
		{
			return render.BindSamplers (i => bindings);
		}

		public static Reaction<T> Viewport<T> (this Reaction<T> render, Func<T, Vec2i> getSize)
		{
			return input =>
			{
				var oldSize = new int[4];
				GL.GetInteger (GetPName.Viewport, oldSize);
				var size = getSize (input);
				GL.Viewport (0, 0, size.X, size.Y);
				var result = render (input);
				GL.Viewport (oldSize[0], oldSize[1], oldSize[2], oldSize[3]);
				return result;
			};
		}

		public static Reaction<T> Viewport<T> (this Reaction<T> render, Vec2i size)
		{
			return render.Viewport (i => size);
		}

		public static Reaction<T> Viewport<T> (this Reaction<T> render, GameWindow window)
		{
			return render.Viewport (i => new Vec2i (window.ClientSize.Width, window.ClientSize.Height));
		}

		public static Reaction<T> SwapBuffers<T> (this Reaction<T> render, Func<T, GameWindow> getWindow)
		{
			return input =>
			{
				var result = render (input);
				getWindow (input).SwapBuffers ();
				return result;
			};
		}

		public static Reaction<T> SwapBuffers<T> (this Reaction<T> render, GameWindow window)
		{
			return render.SwapBuffers (i => window);
		}

		public static Reaction<T> Clear<T> (Vec4 clearColor, ClearBufferMask mask)
		{
			return React.By<T> (input =>
			{
				GL.ClearColor (clearColor.X, clearColor.Y, clearColor.Z, clearColor.W);
				GL.Clear (mask);
			});
		}
	}
}