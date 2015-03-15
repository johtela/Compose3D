namespace Compose3D.GLTypes
{
    using System;
    using System.Linq.Expressions;
    using OpenTK.Graphics.OpenGL;

    public class FragmentShader<F, U, B> : Shader
    {
        public FragmentShader (Expression<Func<B>> shader)
            : base (ShaderType.FragmentShader, GetSource (shader))
        {
            Console.WriteLine (GetSource (shader));
        }

        private static string GetSource (Expression<Func<B>> shader)
        {
            return string.Format (
@"#version 330

{0}

{1}

{2}

void main ()
{3}", 
                DeclareVariables (typeof (F), "in"),
                DeclareVariables (typeof (B), "out"),
                DeclareUniforms (typeof (U)),
                ExprToGLSL ((shader as LambdaExpression).Body, typeof (F), typeof (U), typeof (B)));
        }
    }

    public static class FragmentShader
    {
        public static FragmentShader<F, U, B> Create<F, U, B> (F fragment, U uniforms, Expression<Func<B>> shader)
        {
            return new FragmentShader<F, U, B> (shader);
        }
    }
}
