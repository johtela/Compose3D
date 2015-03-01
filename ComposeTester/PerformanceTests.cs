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
            var mat1 = Matrix4.CreateRotationX (30);
            var mat2 = Matrix4.CreateRotationY (40);
            var mat3 = Matrix4.CreateRotationZ (50);
            var mat4 = Matrix4.CreateScale (100, 100, 100);
            var mat5 = Matrix4.CreateTranslation (1000, 1000, 1000);
            for (int i = 0; i < 1000000; i++)
            {
                var res = mat1 * mat2 * mat3 * mat4 * mat5;
            }
        }

        public Compose3D.Arithmetics.Mat4 Convert(Mat4 mat)
        {
            return new Compose3D.Arithmetics.Mat4 (
                mat[0, 0], mat[0, 1], mat[0, 2], mat[0,3],
                mat[1, 0], mat[1, 1], mat[1, 2], mat[1,3],
                mat[2, 0], mat[2, 1], mat[2, 2], mat[2,3],
                mat[3, 0], mat[3, 1], mat[3, 2], mat[3,3]);
        }

        [Test]
        public void TestMat ()
        {
            var mat1 = Convert(Matf.RotationX<Mat4> (30));
            var mat2 = Convert(Matf.RotationY<Mat4> (40));
            var mat3 = Convert(Matf.RotationZ<Mat4> (50));
            var mat4 = Convert(Matf.Scaling<Mat4> (100, 100, 100));
            var mat5 = Convert(Matf.Translation<Mat4> (1000, 1000, 1000));
            for (int i = 0; i < 1000000; i++)
            {
                var res = mat1 * mat2 * mat3 * mat4 * mat5;
            }
        }
    }
}
