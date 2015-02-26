using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Compose3D.GLSL;
using LinqCheck;

namespace ComposeTester
{
    public class PerformanceTests
    {
        [Test]
        public void TestMatrix ()
        {
            for (int i = 0; i < 1000000; i++)
            {
                var mat1 = Matrix4.CreateRotationX (30);
                var mat2 = Matrix4.CreateRotationY (40);
                var mat3 = Matrix4.CreateRotationZ (50);
                var mat4 = Matrix4.CreateScale (100, 100, 100);
                var mat5 = Matrix4.CreateTranslation (1000, 1000, 1000);
                var res = mat1 * mat2 * mat3 * mat4 * mat5;
            }
        }

        [Test]
        public void TestMat ()
        {
            for (int i = 0; i < 1000000; i++)
            {
                var mat1 = Matf.RotationX<Mat4> (30);
                var mat2 = Matf.RotationY<Mat4> (40);
                var mat3 = Matf.RotationZ<Mat4> (50);
                var mat4 = Matf.Scaling<Mat4> (100, 100, 100);
                var mat5 = Matf.Translation<Mat4> (1000, 1000, 1000);
                var res = mat1 * mat2 * mat3 * mat4 * mat5;
            }
        }
    }
}
