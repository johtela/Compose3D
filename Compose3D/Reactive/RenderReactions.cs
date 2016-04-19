namespace Compose3D.Reactive
{
	using System.Collections.Generic;
	using GLTypes;
	using Maths;
	using Textures;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;

	public static class Render
	{
		private static int GetState (GetPName parameter)
		{
			int value;
			GL.GetInteger (parameter, out value);
			return value;
		}

		public static Reaction<T> Culling<T> (this Reaction<T> render, CullFaceMode mode = CullFaceMode.Back,
			FrontFaceDirection frontFace = FrontFaceDirection.Cw)
		{
			return input =>
			{
				var oldCap = GL.IsEnabled (EnableCap.CullFace);
				var oldMode = GetState (GetPName.CullFaceMode);
				var oldFrontFace = GetState (GetPName.FrontFace);

				GL.Enable (EnableCap.CullFace);
				GL.CullFace (mode);
				GL.FrontFace (frontFace);
				var result = render (input);
				GL.FrontFace ((FrontFaceDirection)oldFrontFace);
				GL.CullFace ((CullFaceMode)oldMode);
				if (!oldCap)
					GL.Disable (EnableCap.CullFace);
				return result;
			};
		}

		public static Reaction<T> DepthTest<T> (this Reaction<T> render, 
			DepthFunction depthFunction = DepthFunction.Less)
		{
			return input =>
			{
				var oldCap = GL.IsEnabled (EnableCap.DepthTest);
				var oldFunction = GetState (GetPName.DepthFunc);
				var oldMask = GetState (GetPName.DepthWritemask);

				GL.Enable (EnableCap.DepthTest);
				GL.DepthMask (true);
				GL.DepthFunc (depthFunction);
				var result = render (input);
				GL.DepthMask (oldMask != 0);
				GL.DepthFunc ((DepthFunction)oldFunction);
				if (!oldCap)
					GL.Disable (EnableCap.DepthTest);
				return result;
			};
		}
		
		public static Reaction<T> Blending<T> (this Reaction<T> render, 
			BlendingFactorSrc source = BlendingFactorSrc.Src1Alpha,
			BlendingFactorDest destination = BlendingFactorDest.OneMinusSrc1Alpha)
		{
			return input =>
			{
				var oldCap = GL.IsEnabled (EnableCap.Blend);
				var oldSource = GetState (GetPName.BlendSrc);
				var oldDest = GetState (GetPName.BlendDst);

				GL.Enable (EnableCap.Blend);
				GL.BlendFunc (source, destination);
				var result = render (input);
				GL.BlendFunc ((BlendingFactorSrc)oldSource, (BlendingFactorDest)oldDest);
				if (!oldCap)
					GL.Disable (EnableCap.Blend);
				return result;
			};
		}

		public static Reaction<T> DrawBuffer<T> (this Reaction<T> render, DrawBufferMode mode)
		{
			return input =>
			{
				var oldMode = GetState (GetPName.DrawBuffer);
				GL.DrawBuffer (mode);
				var result = render (input);
				GL.DrawBuffer ((DrawBufferMode)oldMode);
				return result;
			};
		}

		public static Reaction<T> Program<T> (this Reaction<T> render, Program program)
		{
			return input =>
			{
				using (program.Scope ())
					return render (input);
			};
		}

		public static Reaction<T> Texture<T> (this Reaction<T> render, Texture texture)
		{
			return input =>
			{
				using (texture.Scope ())
					return render (input);
			};
		}

		public static Reaction<T> Framebuffer<T> (this Reaction<T> render, Framebuffer framebuffer)
		{
			return input =>
			{
				using (framebuffer.Scope ())
					return render (input);
			};
		}

		public static Reaction<T> BindSamplers<T> (this Reaction<T> render, IDictionary<Sampler, Texture> bindings)
		{
			return input =>
			{
				Sampler.Bind (bindings);
				var result = render (input);
				Sampler.Unbind (bindings);
				return result;
			};
		}

		public static Reaction<T> Viewport<T> (this Reaction<T> render, Vec2i size)
		{
			return input =>
			{
				var oldSize = new int[4];
				GL.GetInteger (GetPName.Viewport, oldSize);
				GL.Viewport (0, 0, size.X, size.Y);
				var result = render (input);
				GL.Viewport (oldSize[0], oldSize[1], oldSize[2], oldSize[3]);
				return result;
			};
		}
		
		public static Reaction<T> Viewport<T> (this Reaction<T> render, GameWindow window)
		{
			return input =>
			{
				var oldSize = new int[4];
				GL.GetInteger (GetPName.Viewport, oldSize);
				GL.Viewport (0, 0, window.ClientSize.Width, window.ClientSize.Height);
				var result = render (input);
				GL.Viewport (oldSize[0], oldSize[1], oldSize[2], oldSize[3]);
				return result;
			};
		}
	}}