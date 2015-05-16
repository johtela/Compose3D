namespace Compose3D.GLTypes
{
    using OpenTK.Graphics.OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

	public class Shader
	{
		internal int _glShader;

		public Shader (int glShader)
		{
			_glShader = glShader;
		}

		public Shader (ShaderType type, string source)
		{
			_glShader = GL.CreateShader (type);
			GL.ShaderSource (_glShader, source);
			GL.CompileShader (_glShader);
			var log = GL.GetShaderInfoLog (_glShader);
			if (log.ToUpper ().Contains ("ERROR"))
				throw new GLError (string.Format ("Shader compilation error:\n{0}", log));
		}

        public static Shader FromFile (ShaderType type, string path)
        {
            return new Shader (type, File.ReadAllText (path));
        }

        public static Shader Create<T> (ShaderType type, IQueryable<T> shader)
        {
            var source = ShaderBuilder.Execute (shader);
            Console.WriteLine(source);
            return new Shader (type, source);
        }
    }
}
