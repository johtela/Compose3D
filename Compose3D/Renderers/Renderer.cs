namespace Compose3D.Renderers
{
	using System;
	using System.Linq;
	using OpenTK.Graphics.OpenGL;

	public delegate void Renderer ();
	
	public static class Render
	{ 
		public static Renderer Do (Action action)
		{
			return new Renderer (action);
		}
		
		public static Renderer Culling (this Renderer render, CullFaceMode mode = CullFaceMode.Back, 
			FrontFaceDirection frontFace = FrontFaceDirection.Cw)
		{
			GL.Enable (EnableCap.CullFace);
			GL.CullFace (mode);
			GL.FrontFace (frontFace);
			render ();
			GL.Disable (EnableCap.CullFace);
		}
		
		public static Renderer DepthTest (this Renderer render, DepthFunction depthFunction = DepthFunction.Less)
		{
			GL.Enable (EnableCap.DepthTest);
			GL.DepthMask (true);
			GL.DepthFunc (depthFunction);
			render ();
			GL.Disable (EnableCap.DepthTest);
		}
		
		public static Renderer Blending (this Renderer render, 
			BlendingFactorSrc source = BlendingFactorSrc.Src1Alpha,
			BlendingFactorDest destination = BlendingFactorDest.OneMinusSrc1Alpha)
		{
			GL.Enable (EnableCap.Blend);
			GL.BlendFunc (source, destination);
			render ();
			GL.Disable (EnableCap.Blend);
		}
	}
}
