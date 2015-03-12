namespace Compose3D.GLTypes
{
    using System;
    using System.Linq.Expressions;
    using OpenTK.Graphics.OpenGL;

    public class VertexShader<V, U, F> : Shader where V : struct 
    {
        public VertexShader (Expression<Func<V, U, F>> shader) : base (ShaderType.VertexShader, GetSource (shader))
        {
            Console.WriteLine (GetSource (shader));
        }

        private static string GetSource (Expression<Func<V, U, F>> shader)
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
}
