using System;
using System.Collections.Generic;

namespace Kula.Data.Type
{
    public class RawType : IType
    {
        private readonly System.Type type;
        private readonly string @string;

        public static IDictionary<string, IType> TypeDict = new Dictionary<string, IType>();
        public static IDictionary<IType, string> InvertTypeDict = new Dictionary<IType, string>();

        public static readonly RawType None = new RawType(typeof(void), "None");
        public static readonly RawType Any = new RawType(typeof(object), "Any");
        public static readonly RawType Num = new RawType(typeof(float), "Num");
        public static readonly RawType Str = new RawType(typeof(string), "Str");
        public static readonly RawType SharpFunc = new RawType(typeof(Function.SharpFunc), "SharpFunc");
        public static readonly RawType Func = new RawType(typeof(Function.Func), "Func");
        public static readonly RawType Array = new RawType(typeof(Container.Array), "Array");
        public static readonly RawType Map = new RawType(typeof(Container.Map), "Map");


        private RawType(System.Type type, string name)
        {
            this.type = type;
            this.@string = name;
            TypeDict[name] = this;
            InvertTypeDict[this] = name;
        }

        public bool Check(object o)
        {
            if (type == null) return false;
            return type == typeof(object) || o.GetType() == type;
        }


        public override string ToString() => @string;
    }
}
