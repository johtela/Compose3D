using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Compose3D.GLSL
{
    public abstract class Vec4<TComp, TImpl> : Vec3<TComp, TImpl>
        where TImpl : IEquatable<TImpl>
    {
        public abstract TComp W { get; set; }

        public Vec2<TComp, TImpl> XW
        {
            get
            {
                var result = (Vec2<TComp, TImpl>)Clone ();
                result.Y = W;
                return result;
            }
            set
            {
                X = value.X;
                W = value.Y;
            }
        }

        public Vec2<TComp, TImpl> YW
        {
            get
            {
                var result = (Vec2<TComp, TImpl>)Clone ();
                result.X = Y;
                result.Y = W;
                return result;
            }
            set
            {
                Y = value.X;
                W = value.Y;
            }
        }

        public Vec2<TComp, TImpl> ZW
        {
            get
            {
                var result = (Vec2<TComp, TImpl>)Clone ();
                result.X = Z;
                result.Y = W;
                return result;
            }
            set
            {
                Z = value.X;
                W = value.Y;
            }
        }

        public Vec2<TComp, TImpl> WX
        {
            get
            {
                var result = (Vec2<TComp, TImpl>)Clone ();
                result.X = W;
                result.Y = X;
                return result;
            }
            set
            {
                W = value.X;
                X = value.Y;
            }
        }

        public Vec2<TComp, TImpl> WY
        {
            get
            {
                var result = (Vec2<TComp, TImpl>)Clone ();
                result.X = W;
                result.Y = Y;
                return result;
            }
            set
            {
                W = value.X;
                Y = value.Y;
            }
        }

        public Vec2<TComp, TImpl> WZ
        {
            get
            {
                var result = (Vec2<TComp, TImpl>)Clone ();
                result.X = W;
                result.Y = Z;
                return result;
            }
            set
            {
                W = value.X;
                Z = value.Y;
            }
        }

        public Vec3<TComp, TImpl> XYW
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.Z = W;
                return result;
            }
            set
            {
                X = value.X;
                Y = value.Y;
                W = value.Z;
            }
        }

        public Vec3<TComp, TImpl> XZW
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.Y = Z;
                result.Z = W;
                return result;
            }
            set
            {
                X = value.X;
                Z = value.Y;
                W = value.Z;
            }
        }

        public Vec3<TComp, TImpl> XWY
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.Y = W;
                result.Z = Y;
                return result;
            }
            set
            {
                X = value.X;
                W = value.Y;
                Y = value.Z;
            }
        }

        public Vec3<TComp, TImpl> XWZ
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.Y = W;
                result.Z = Z;
                return result;
            }
            set
            {
                X = value.X;
                W = value.Y;
                Z = value.Z;
            }
        }

        public Vec3<TComp, TImpl> YXW
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.X = Y;
                result.Y = X;
                result.Z = W;
                return result;
            }
            set
            {
                Y = value.X;
                X = value.Y;
                W = value.Z;
            }
        }

        public Vec3<TComp, TImpl> YZW
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.X = Y;
                result.Y = Z;
                result.Z = W;
                return result;
            }
            set
            {
                Y = value.X;
                Z = value.Y;
                W = value.Z;
            }
        }

        public Vec3<TComp, TImpl> YWX
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.X = Y;
                result.Y = W;
                result.Z = X;
                return result;
            }
            set
            {
                Y = value.X;
                W = value.Y;
                X = value.Z;
            }
        }

        public Vec3<TComp, TImpl> YWZ
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.X = Y;
                result.Y = W;
                return result;
            }
            set
            {
                Y = value.X;
                W = value.Y;
                Z = value.Z;
            }
        }

        public Vec3<TComp, TImpl> ZXW
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.X = Z;
                result.Y = X;
                result.Z = W;
                return result;
            }
            set
            {
                Z = value.X;
                X = value.Y;
                W = value.Z;
            }
        }

        public Vec3<TComp, TImpl> ZYW
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.X = Z;
                result.Z = W;
                return result;
            }
            set
            {
                Z = value.X;
                Y = value.Y;
                W = value.Z;
            }
        }

        public Vec3<TComp, TImpl> ZWX
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.X = Z;
                result.Y = W;
                result.Z = X;
                return result;
            }
            set
            {
                Z = value.X;
                W = value.Y;
                X = value.Z;
            }
        }

        public Vec3<TComp, TImpl> ZWY
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.X = Z;
                result.Y = W;
                result.Z = Y;
                return result;
            }
            set
            {
                Z = value.X;
                W = value.Y;
                Y = value.Z;
            }
        }

        public Vec3<TComp, TImpl> WXY
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.X = W;
                result.Y = X;
                result.Z = Y;
                return result;
            }
            set
            {
                W = value.X;
                X = value.Y;
                Y = value.Z;
            }
        }

        public Vec3<TComp, TImpl> WXZ
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.X = W;
                result.Y = X;
                return result;
            }
            set
            {
                W = value.X;
                X = value.Y;
                Z = value.Z;
            }
        }

        public Vec3<TComp, TImpl> WYX
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.X = W;
                result.Z = X;
                return result;
            }
            set
            {
                W = value.X;
                Y = value.Y;
                X = value.Z;
            }
        }

        public Vec3<TComp, TImpl> WYZ
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.X = W;
               return result;
            }
            set
            {
                W = value.X;
                Y = value.Y;
                Z = value.Z;
            }
        }

        public Vec3<TComp, TImpl> WZX
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.X = W;
                result.Y = Z;
                result.Z = X;
                return result;
            }
            set
            {
                W = value.X;
                Z = value.Y;
                X = value.Z;
            }
        }

        public Vec3<TComp, TImpl> WZY
        {
            get
            {
                var result = (Vec3<TComp, TImpl>)Clone ();
                result.X = W;
                result.Y = Z;
                result.Z = Y;
                return result;
            }
            set
            {
                W = value.X;
                Z = value.Y;
                Y = value.Z;
            }
        }

        public Vec4<TComp, TImpl> XYWZ
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.Z = W;
                result.W = Z;
                return result;
            }
            set
            {
                X = value.X;
                Y = value.Y;
                W = value.Z;
                Z = value.W;
            }
        }

        public Vec4<TComp, TImpl> XZYW
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.Y = Z;
                result.Z = Y;
                return result;
            }
            set
            {
                X = value.X;
                Z = value.Y;
                Y = value.Z;
                W = value.W;
            }
        }

        public Vec4<TComp, TImpl> XZWY
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.Y = Z;
                result.Z = W;
                result.W = Y;
                return result;
            }
            set
            {
                X = value.X;
                Z = value.Y;
                W = value.Z;
                Y = value.W;
            }
        }

        
        public Vec4<TComp, TImpl> XWYZ
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.Y = W;
                result.Z = Y;
                result.W = Z;
                return result;
            }
            set
            {
                X = value.X;
                W = value.Y;
                Y = value.Z;
                Z = value.W;
            }
        }

        public Vec4<TComp, TImpl> XWZY
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.Y = W;
                result.W = Y;
                return result;
            }
            set
            {
                X = value.X;
                W = value.Y;
                Z = value.Z;
                Y = value.W;
            }
        }

        public Vec4<TComp, TImpl> YXZW
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.X = Y;
                result.Y = X;
                return result;
            }
            set
            {
                Y = value.X;
                X = value.Y;
                Z = value.Z;
                W = value.W;
            }
        }

        public Vec4<TComp, TImpl> YXWZ
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.X = Y;
                result.Y = X;
                result.Z = W;
                result.W = Z;
                return result;
            }
            set
            {
                Y = value.X;
                X = value.Y;
                W = value.Z;
                Z = value.W;
            }
        }

        public Vec4<TComp, TImpl> YZXW
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
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
                W = value.W;
            }
        }

        public Vec4<TComp, TImpl> YZWX
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.X = Y;
                result.Y = Z;
                result.Z = W;
                result.W = X;
                return result;
            }
            set
            {
                Y = value.X;
                Z = value.Y;
                W = value.Z;
                X = value.W;
            }
        }

        public Vec4<TComp, TImpl> YWXZ
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.X = Y;
                result.Y = W;
                result.Z = X;
                result.W = Z;
                return result;
            }
            set
            {
                Y = value.X;
                W = value.Y;
                X = value.Z;
                Z = value.W;
            }
        }

        public Vec4<TComp, TImpl> YWZX
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.X = Y;
                result.Y = W;
                result.W = X;
                return result;
            }
            set
            {
                Y = value.X;
                W = value.Y;
                Z = value.Z;
                X = value.W;
            }
        }

        public Vec4<TComp, TImpl> ZXYW
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
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
                W = value.W;
            }
        }

        public Vec4<TComp, TImpl> ZXWY
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.X = Z;
                result.Y = X;
                result.Z = W;
                result.W = Y;
                return result;
            }
            set
            {
                Z = value.X;
                X = value.Y;
                W = value.Z;
                Y = value.W;
            }
        }

        public Vec4<TComp, TImpl> ZYXW
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.X = Z;
                result.Z = X;
                return result;
            }
            set
            {
                Z = value.X;
                Y = value.Y;
                X = value.Z;
                W = value.W;
            }
        }

        public Vec4<TComp, TImpl> ZYWX
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.X = Z;
                result.Z = W;
                result.W = X;
                return result;
            }
            set
            {
                Z = value.X;
                Y = value.Y;
                W = value.Z;
                X = value.W;
            }
        }

        public Vec4<TComp, TImpl> ZWXY
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.X = Z;
                result.Y = W;
                result.Z = X;
                result.W = Y;
                return result;
            }
            set
            {
                Z = value.X;
                W = value.Y;
                X = value.Z;
                Y = value.W;
            }
        }

        public Vec4<TComp, TImpl> ZWYX
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.X = Z;
                result.Y = W;
                result.Z = Y;
                result.W = X;
                return result;
            }
            set
            {
                Z = value.X;
                W = value.Y;
                Y = value.Z;
                X = value.W;
            }
        }

        public Vec4<TComp, TImpl> WXYZ
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.X = W;
                result.Y = X;
                result.Z = Y;
                result.W = Z;
                return result;
            }
            set
            {
                W = value.X;
                X = value.Y;
                Y = value.Z;
                Z = value.W;
            }
        }

        public Vec4<TComp, TImpl> WXZY
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.X = W;
                result.Y = X;
                result.W = Y;
                return result;
            }
            set
            {
                W = value.X;
                X = value.Y;
                Z = value.Z;
                Y = value.W;
            }
        }

        public Vec4<TComp, TImpl> WYXZ
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.X = W;
                result.Z = X;
                result.W = Z;
                return result;
            }
            set
            {
                W = value.X;
                Y = value.Y;
                X = value.Z;
                Z = value.W;
            }
        }

        public Vec4<TComp, TImpl> WYZX
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.X = W;
                result.W = X;
                return result;
            }
            set
            {
                W = value.X;
                Y = value.Y;
                Z = value.Z;
                X = value.W;
            }
        }

        public Vec4<TComp, TImpl> WZXY
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.X = W;
                result.Y = Z;
                result.Z = X;
                result.W = Y;
                return result;
            }
            set
            {
                W = value.X;
                Z = value.Y;
                X = value.Z;
                Y = value.W;
            }
        }

        public Vec4<TComp, TImpl> WZYX
        {
            get
            {
                var result = (Vec4<TComp, TImpl>)Clone ();
                result.X = W;
                result.Y = Z;
                result.Z = Y;
                result.W = X;
                return result;
            }
            set
            {
                W = value.X;
                Z = value.Y;
                Y = value.Z;
                X = value.W;
            }
        }

        public override TComp this[int index]
        {
            get { return index == 3 ? W : base[index]; }
            set
            {
                if (index == 3) W = value;
                else base[index] = value;
            }
        }
    }
}