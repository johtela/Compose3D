namespace ComposeFX.Graphics.GLTypes
{
	using Compiler;
    using Maths;
	using Textures;
    using OpenTK.Graphics.OpenGL4;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
	using System.Diagnostics;

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
			{ typeof(Mat4), (u, o) => GL.UniformMatrix4 (u, 1, false, Mat.ToArray<Mat4, float> ((Mat4)o)) },
			{ typeof(Sampler1D), (u, o) => GL.Uniform1 (u, ((Sampler)o)._texUnit) },
			{ typeof(Sampler1DArray), (u, o) => GL.Uniform1 (u, ((Sampler)o)._texUnit) },
			{ typeof(Sampler1DShadow), (u, o) => GL.Uniform1 (u, ((Sampler)o)._texUnit) },
			{ typeof(Sampler2D), (u, o) => GL.Uniform1 (u, ((Sampler)o)._texUnit) },
			{ typeof(Sampler2DArray), (u, o) => GL.Uniform1 (u, ((Sampler)o)._texUnit) },
			{ typeof(Sampler2DShadow), (u, o) => GL.Uniform1 (u, ((Sampler)o)._texUnit) },
			{ typeof(Sampler3D), (u, o) => GL.Uniform1 (u, ((Sampler)o)._texUnit) },
			{ typeof(SamplerCube), (u, o) => GL.Uniform1 (u, ((Sampler)o)._texUnit) }
		};

        private int _glUniform;
		private string _name;
        private Tuple<GLStructField, int>[] _mappings;
        private T _value;

        public Uniform (GLProgram program, FieldInfo field)
		{
            if (field.FieldType.GetGenericTypeDefinition () != typeof (Uniform<>))
                throw new ArgumentException ("Field must be of Uniform<> generic type.");
            var type = field.FieldType.GetGenericArguments() [0];
            if (type != typeof (T))
                throw new ArgumentException ("Field type is different from uniform type.");
            if (type.IsArray)
                _mappings = (from elem in type.GetGLArrayElements (field.Name, field.ExpectFixedArrayAttribute ().Length)
                             select Tuple.Create (elem, GetUniformLocation (program, elem.Name)))
                            .ToArray ();
            else
                CreateUniform (program, field.Name, type);
		}

        public Uniform (GLProgram program, string name)
		{
            CreateUniform (program, name, typeof (T));
		}

        private void CreateUniform (GLProgram program, string name, Type type)
        {
            if (type.IsGLStruct ())
                _mappings = (from field in type.GetGLStructFields (name + ".")
                             select Tuple.Create (field, GetUniformLocation (program, field.Name)))
                            .ToArray ();
            else
                _glUniform = GetUniformLocation (program, name);
			_name = name;
        }

        private static int GetUniformLocation (GLProgram program, string name)
        {
            var loc = GL.GetUniformLocation (program._glProgram, name);
			if (loc < 0)
				Trace.TraceWarning (string.Format ("Creating uniform '{0}' which is not found in program. " +
					" Probably optimized away by OpenGL.", name));			
            return loc;
        }
		
		private static bool UniformInitialized (string name, int unif)
		{
			if (unif < 0)
			{
				Trace.TraceWarning (string.Format ("Assigning value to uniform '{0}' which is not found " +
					"in the program. Probably optimized away by OpenGL.", name));			
				return false;
			}
			return true;
		}

        public static Uniform<T> operator & (Uniform<T> uniform, T value)
		{
            try
            {
                var type = typeof (T);
				if (type.IsGLStruct () || type.IsArray)
                    foreach (var map in uniform._mappings)
                    {
                        var field = map.Item1;
                        var glUnif = map.Item2;
						if (UniformInitialized (field.Name, glUnif))
                    		_setters[field.Type] (glUnif, field.Getter (value));
                    }
                else
					if (UniformInitialized (uniform._name, uniform._glUniform))
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
