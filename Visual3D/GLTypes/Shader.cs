namespace Visual3D.GLTypes
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;
	using System.IO;

	public class Shader
	{
		internal int _glShader;

		public Shader (int glShader)
		{
			_glShader = glShader;
		}

		public Shader (ShaderType type, string path)
		{
			var src = File.ReadAllText (path);
			_glShader = GL.CreateShader (type);
			GL.ShaderSource (_glShader, src);
			GL.CompileShader (_glShader);
			var log = GL.GetShaderInfoLog (_glShader);
			if (log.Contains ("ERROR"))
				throw new GLError (string.Format ("Shader compilation error in '{0}':\n{1}", path, log));
		}
	}
}
