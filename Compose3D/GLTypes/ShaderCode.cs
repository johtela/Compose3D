namespace Compose3D.GLTypes
{
    using System;
    using System.Linq.Expressions;
    using OpenTK.Graphics.OpenGL;

    public class ShaderCode<T>
    {
        Expression<Func<T>> _code;

        public ShaderCode (Expression<Func<T>> code)
        {
            _code = code;
        }

        public string Compile (Type[] types)
        {
            return Shader.ExprToGLSL ((_code as LambdaExpression).Body, types);
        }

        public static T operator ! (ShaderCode<T> shaderCode)
        {
            return default (T);
        }
    }
}
