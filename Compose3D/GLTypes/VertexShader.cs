namespace Compose3D.GLTypes
{
    using System;
    using System.Linq.Expressions;
    using OpenTK.Graphics.OpenGL;

    public class VertexShader<V, U, F> : Shader where V : struct 
    {
        public VertexShader (Expression<Func<F>> shader)
            : base (ShaderType.VertexShader, GetSource (shader))
        {
            Console.WriteLine (GetSource (shader));
        }

        private static string GetSource (Expression<Func<F>> shader)
        {
            return string.Format (
@"#version 330

{0}

{1}

{2}

void main ()
{3}", 
                DeclareVariables (typeof (V), "in"),
                DeclareVariables (typeof (F), "out"),
                DeclareUniforms (typeof (U)),
                ExprToGLSL ((shader as LambdaExpression).Body, typeof (V), typeof (U), typeof (F)));
        }
    }

    public static class VertexShader
    {
        public static VertexShader<V, U, F> Create<V, U, F> (V vertex, U uniforms, Expression<Func<F>> shader)
            where V : struct
        {
            return new VertexShader<V, U, F> (shader);
        }
    }
}
