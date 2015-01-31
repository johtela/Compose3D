using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Compose3D.GLSL
{
    public abstract class Vec2<TComp, TImpl> : Vec<TComp, TImpl>
        where TImpl : IEquatable<TImpl>
    {
        public abstract TComp X { get; set; }
        public abstract TComp Y { get; set; }

        public Vec2<TComp, TImpl> YX
        {
            get 
            { 
                var result = (Vec2<TComp, TImpl>)Clone ();
                result.X = Y;
                result.Y = X;
                return result;
            }
            set
            {
                X = value.Y;
                Y = value.X;
            }
        }

        public virtual TComp this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return X;
                    case 1: return Y;
                    default: throw new IndexOutOfRangeException ("No component at index " + index);
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        return;
                    case 1:
                        Y = value;
                        return;
                    default: 
                        throw new IndexOutOfRangeException ("No component at index " + index);
                }
            }
        }
    }
}
