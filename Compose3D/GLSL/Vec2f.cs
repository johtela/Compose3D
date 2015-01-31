using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Compose3D.GLSL
{
    public class Vec2f : Vec2<float, Vector2>
    {
        public Vec2f () { }

        public Vec2f (float x, float y)
        {
            _vector = new Vector2 (x, y);
        }

        private Vec2f (Vector2 vector)
        {
            _vector = vector;
        }

        protected override Vec<float, Vector2> Neg ()
        {
            return new Vec2f (-_vector);
        }

        protected override Vec<float, Vector2> Add (Vec<float, Vector2> other)
        {
            var result = new Vec2f ();
            Vector2.Add (ref _vector, ref other._vector, out result._vector);
            return result;
        }

        protected override Vec<float, Vector2> Sub (Vec<float, Vector2> other)
        {
            var result = new Vec2f ();
            Vector2.Subtract (ref _vector, ref other._vector, out result._vector);
            return result;
        }

        protected override Vec<float, Vector2> Mul (float scalar)
        {
            var result = new Vec2f ();
            Vector2.Multiply (ref _vector, scalar, out result._vector);
            return result;
        }

        protected override Vec<float, Vector2> Mul (Vec<float, Vector2> other)
        {
            var result = new Vec2f ();
            Vector2.Multiply (ref _vector, ref other._vector, out result._vector);
            return result;
        }

        protected override Vec<float, Vector2> Div (float scalar)
        {
            var result = new Vec2f ();
            Vector2.Divide (ref _vector, scalar, out result._vector);
            return result;
        }

        public override float Dot (Vec<float, Vector2> other)
        {
            float result;
            Vector2.Dot (ref _vector, ref other._vector, out result);
            return result;
        }

        public override float Length ()
        {
            return _vector.Length;
        }

        public override float Distance (Vec<float, Vector2> other)
        {
            return Sub (other).Length ();
        }

        public override Vec<float, Vector2> Normalize ()
        {
            var result = new Vec2f(_vector);
            result._vector.Normalize ();
            return result;
        }

        public override float X 
        {
            get { return _vector.X; }
            set { _vector.X = value; }
        }

        public override float Y
        {
            get { return _vector.Y; }
            set { _vector.Y = value; }
        }

        public override object Clone ()
        {
            return new Vec2f (_vector);
        }
    }
}
