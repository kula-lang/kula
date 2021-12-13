using Kula.Core;
using Kula.Data.Type;
using System.Collections.Generic;
using System.Text;

namespace Kula.Data.Function
{
    class Lambda
    {
        public List<LexToken> TokenStream { get; }
        public List<ByteCode> CodeStream { get; }
        public List<(string, IType)> ArgList { get; }
        public IType ReturnType { get; set; }

        public Lambda(List<LexToken> tokenStream)
        {
            this.TokenStream = tokenStream;

            this.ArgList = new List<(string, IType)>();
            this.CodeStream = new List<ByteCode>();
        }

        private string @string = null;

        public override string ToString()
        {
            if (ReturnType == null)
                return "Uncompiled.";
            if (@string == null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("func(");
                for (int i = 0; i < ArgList.Count; ++i)
                {
                    if (i != 0)
                        sb.Append(',');
                    sb.Append(ArgList[i].Item2.ToString());
                }
                sb.Append("):");
                sb.Append(ReturnType.ToString());
                @string = sb.ToString();
            }
            return @string;
        }
    }
}
