using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Compose3D.GLSL
{
    public abstract class Vec3<TComp, TImpl> : Vec2<TComp, TImpl>
        where TImpl : IEquatable<TImpl>
    {
        public abstract TComp Z { get; set; }

        public Vec2<TComp, TImpl> XY
        {
            get { return (Vec2<TComp, TImpl>)Clone (); }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public Vec2<TComp, TImpl> XZ
        {
            get 
            {
                var result = (Vec2<TComp, TImpl>)Clone ();
                result.Y = Z;
                return result;
            }
            set
            {
                X = value.X;
                Z = value.Y;
            }
        }

        public Vec2<TComp, TImpl> YZ
        {
            get
            {
                var result = (Vec2<TComp, TImpl>)Clone ();
                result.X = Y;
                result.Y = Z;
                return result;
            }
            set
            {
                Y = value.X;
                Z = value.Y;
            }
        }

        public Vec2<TComp, TImpl> ZX
        {
            get
            {
                var result = (Vec2<TComp, TImpl>)Clone ();
                result.X = Z;
                result.Y = X;
                return result;
            }
            set
            {
                Z = value.X;
                X = value.Y;
            }
        }

        public Vec2<TComp, TImpl> ZY
        {
            get
            {
                var result = (Vec2<TComp, TImpl>)Clone ();
                result.X = Z;
                return result;
            }
            set
            {
                Z = value.X;
                Y = value.Y;
            }
        }

        public Vec3<TComp, TImpl> XZY
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.Y = Z;
                result.Z = Y;
                return result;
            }
            set
            {
                X = value.X;
                Z = value.Y;
                Y = value.Z;
            }
        }

        public Vec3<TComp, TImpl> YXZ
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.X = Y;
                result.Y = X;
                return result;
            }
            set
            {
                Y = value.X;
                X = value.Y;
                Z = value.Z;
            }
        }

        public Vec3<TComp, TImpl> YZX
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.X = Y;
                result.Y = Z;
                result.Z = X;
                return result;
            }
            set
            {
                Y = value.X;
                Z = value.Y;
                X = value.Z;
            }
        }

        public Vec3<TComp, TImpl> ZXY
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.X = Z;
                result.Y = X;
                result.Z = Y;
                return result;
            }
            set
            {
                Z = value.X;
                X = value.Y;
                Y = value.Z;
            }
        }

        public Vec3<TComp, TImpl> ZYX
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.X = Z;
                result.Z = X;
                return result;
            }
            set
            {
                Z = value.X;
                Y = value.Y;
                X = value.Z;
            }
        }

        public override TComp this[int index]
        {
            get { return index == 2 ? Z : base[index]; }
            set 
            { 
                if (index == 2) Z = value;
                else base[index] = value; 
            }
        }
    }
}
