using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Compose3D.Maths;
using LinqCheck;

namespace ComposeTester
{
    public class PerformanceTests
    {
        [Test]
        public void TestMatrix ()
        {
            var mat1 = Matrix4.CreateRotationX (30);
            var mat2 = Matrix4.CreateRotationY (40);
            var mat3 = Matrix4.CreateRotationZ (50);
            var mat4 = Matrix4.CreateScale (100, 100, 100);
            var mat5 = Matrix4.CreateTranslation (1000, 1000, 1000);
            Matrix4 res = new Matrix4 ();
            for (int i = 0; i < 1000000; i++)
            {
                res = mat1 * mat2 * mat3 * mat4 * mat5;
            }
			Console.WriteLine (res);
        }

        [Test]
        public void TestMat ()
        {
            var mat1 = Mat.RotationX<Mat4> (30);
            var mat2 = Mat.RotationY<Mat4> (40);
            var mat3 = Mat.RotationZ<Mat4> (50);
            var mat4 = Mat.Scaling<Mat4> (100, 100, 100);
            var mat5 = Mat.Translation<Mat4> (1000, 1000, 1000);
            Mat4 res = new Mat4 ();
            for (int i = 0; i < 1000000; i++)
            {
                res = mat1 * mat2 * mat3 * mat4 * mat5;
            }
			Console.WriteLine (res);
        }
    }
}
