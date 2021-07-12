using System;
using System.Collections.Generic;

using Kula.Util;
using Kula.Core;
using System.Text;

namespace Kula.Data
{
    
    class Func
    {
        public List<LexToken> TokenStream { get; }
        public List<VMNode> NodeStream { get; }
        public List<Type> ArgTypes { get; }
        public List<string> ArgNames { get; }
        public Type ReturnType { get; set; }

        public Func(List<LexToken> tokenStream)
        {
            this.TokenStream = tokenStream;

            this.ArgTypes = new List<Type>();
            this.ArgNames = new List<string>();
            this.NodeStream = new List<VMNode>();
        }

        private string @string = null;

        public override string ToString()
        {
            if (@string == null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("func(");
                for (int i = 0; i < ArgTypes.Count; ++i)
                {
                    if (i != 0)
                    {
                        sb.Append(',');
                    }
                    sb.Append(ArgTypes[i].KTypeToString());
                }
                sb.Append("):");
                sb.Append(ReturnType.KTypeToString());
                @string = sb.ToString();
            }
            return @string;
        }
    }
}
