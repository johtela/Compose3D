namespace Compose3D.GLTypes
{
    using System;
    using System.Linq.Expressions;

    public class VertexShader<V, U, F> : Shader where V : struct 
    {
        public VertexShader (Expression<Func<V, U, F>> shader) : base (1)
        {

        }


    }
}
