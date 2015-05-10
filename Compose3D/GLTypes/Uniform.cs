namespace Compose3D.GLTypes
{
    using Arithmetics;
    using OpenTK.Graphics.OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

	public class Uniform<T>
	{
		private static Dictionary<Type, Action<int, object>> _setters = new Dictionary<Type, Action<int, object>> ()
		{
			{ typeof(int), (u, o) => GL.Uniform1 (u, (int)o) },
			{ typeof(uint), (u, o) => GL.Uniform1 (u, (uint)o) },
			{ typeof(float), (u, o) => GL.Uniform1 (u, (float)o) },
			{ typeof(double), (u, o) => GL.Uniform1 (u, (double)o) },
			{ typeof(int[]), (u, o) => { var a = (int[])o; GL.Uniform1 (u, a.Length, a); }},
			{ typeof(uint[]), (u, o) => { var a = (uint[])o; GL.Uniform1 (u, a.Length, a); }},
			{ typeof(float[]), (u, o) => { var a = (float[])o; GL.Uniform1 (u, a.Length, a); }},
			{ typeof(double[]), (u, o) => { var a = (double[])o; GL.Uniform1 (u, a.Length, a); }},
			{ typeof(Vec3), (u, o) => GL.Uniform3 (u, 1, Vec.ToArray<Vec3, float> ((Vec3)o)) },
			{ typeof(Vec4), (u, o) => GL.Uniform4 (u, 1, Vec.ToArray<Vec4, float> ((Vec4)o)) },
			{ typeof(Mat3), (u, o) => GL.UniformMatrix3 (u, 1, false, Mat.ToArray<Mat3, float> ((Mat3)o)) },
			{ typeof(Mat4), (u, o) => GL.UniformMatrix4 (u, 1, false, Mat.ToArray<Mat4, float> ((Mat4)o)) }
		};

        private int _glUniform;
        private Tuple<GLStructField, int>[] _mappings;
        private T _value;

        public Uniform (Program program, FieldInfo field)
		{
            if (field.FieldType.GetGenericTypeDefinition () != typeof (Uniform<>))
                throw new ArgumentException ("Field must be of Uniform<> generic type.");
            var type = field.FieldType.GetGenericArguments() [0];
            if (type != typeof (T))
                throw new ArgumentException ("Field type is different from uniform type.");
            if (type.IsArray)
                _mappings = (from elem in type.GetGLArrayElements (field.Name, field.ExpectGLArrayAttribute ().Length)
                             select Tuple.Create (elem, GetUniformLocation (program, elem.Name)))
                            .ToArray ();
            else
                CreateUniform (program, field.Name, type);
		}

        public Uniform (Program program, string name)
		{
            CreateUniform (program, name, typeof (T));
		}

        private void CreateUniform (Program program, string name, Type type)
        {
            if (type.IsGLStruct ())
                _mappings = (from field in type.GetGLStructFields (name + ".")
                             select Tuple.Create (field, GetUniformLocation (program, field.Name)))
                            .ToArray ();
            else
                _glUniform = GetUniformLocation (program, name);
        }

        private static int GetUniformLocation (Program program, string name)
        {
            var loc = GL.GetUniformLocation (program._glProgram, name);
            if (loc < 0)
                throw new GLError (string.Format ("Uniform '{0}' was not found in program", name));
            return loc;
        }

        public static Uniform<T> operator & (Uniform<T> uniform, T value)
		{
            try
            {
                var type = typeof (T);
                if (type.IsGLStruct ())
                    foreach (var map in uniform._mappings)
                    {
                        var field = map.Item1;
                        var glUnif = map.Item2;
                        _setters[field.Type] (glUnif, field.Getter (value));
                    }
                else
                    _setters[type] (uniform._glUniform, (object)value);
                uniform._value = value;
                return uniform;
            }
            catch (KeyNotFoundException)
            {
                throw new GLError ("Incompatible uniform type: " + typeof (T).Name);
            }
		}

        [GLUnaryOperator ("{0}")]
        public static T operator ! (Uniform<T> uniform)
        {
            return uniform._value;
        }
	}
}
