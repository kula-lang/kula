using Kula.Data.Function;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kula.Data.Type
{
    public class FuncType : IType
    {
        public IType ReturnType { get; private set; }

        public IList<IType> TypeList { get; }

        public FuncType(IType returnType, IList<IType> typeList)
        {
            ReturnType = returnType;
            TypeList = typeList;
        }

        public bool Check(object o)
        {
            if (o is Func o_func)
            {
                Lambda lambda = o_func.Lambda;
                if (ReturnType.CheckType(lambda.ReturnType))
                {
                    if (lambda.ArgList.Count == TypeList.Count)
                    {
                        int len = TypeList.Count;
                        bool flag = true;
                        for (int i=0; i<len && flag; ++i)
                        {
                            flag = TypeList[i].CheckType(lambda.ArgList[i].Item2);
                        }
                        return flag;
                    }
                }
                return false;
            }
            else
                return false;
        }

        public bool CheckType(IType type)
        {
            if (type is FuncType o_func)
            {
                if (o_func.ReturnType.CheckType(ReturnType) && o_func.TypeList.Count == TypeList.Count)
                {
                    int len = TypeList.Count;
                    bool flag = true;
                    for (int i = 0; i < len && flag; ++i)
                    {
                        flag = o_func.TypeList[i].CheckType(TypeList[i]);
                    }
                    return flag;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = ReturnType.GetHashCode();
            foreach (IType type in TypeList)
            {
                hash = hash * 17 + type.GetHashCode();
            }
            return hash;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("func(");
            for (int i=0; i<TypeList.Count; ++i)
            {
                sb.Append(TypeList[i].ToString());
                if (i != 0)
                    sb.Append(',');
            }
            sb.Append("):");
            sb.Append(ReturnType.ToString());
            return sb.ToString();
        }
    }
}
