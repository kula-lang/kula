using System;
using System.Collections.Generic;
using System.Text;

using Kula.Util;
using Kula.Core;
using Kula.Data.Type;

namespace Kula.Data.Function
{
    class Lambda
    {
        public List<LexToken> TokenStream { get; }
        public List<VMNode> NodeStream { get; }
        public List<IType> ArgTypes { get; }
        public List<string> ArgNames { get; }
        public IType ReturnType { get; set; }

        public Lambda(List<LexToken> tokenStream)
        {
            this.TokenStream = tokenStream;

            this.ArgTypes = new List<IType>();
            this.ArgNames = new List<string>();
            this.NodeStream = new List<VMNode>();
        }

        private string @string = null;

        public override string ToString()
        {
            /*
            if (@string == null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("func(");
                for (int i = 0; i < ArgTypes.Count; ++i)
                {
                    if (i != 0)
                        sb.Append(',');
                    sb.Append(ArgTypes[i].ToString());
                }
                sb.Append("):");
                sb.Append(ReturnType.ToString());
                @string = sb.ToString();
            }
            return @string;
            */
            return "tMp";
        }
    }
}
