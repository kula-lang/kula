//using kula.Core;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace kula.Data
//{
//    class Main : IRunnable
//    {
//        private readonly List<LexToken> tokenStream;
//        private readonly List<KvmNode> nodeStream;
//        public List<LexToken> TokenStream => tokenStream;
//        public List<KvmNode> NodeStream => nodeStream;
//        public IRuntime FatherRuntime { get => null; set => _ = value; }
//        public List<Type> ArgTypes { get => throw new NotImplementedException(); }
//        public List<string> ArgNames { get => throw new NotImplementedException(); }
//        public Type ReturnType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

//        public Main(List<LexToken> tokenStream)
//        {
//            this.tokenStream = tokenStream;
//            this.nodeStream = new List<KvmNode>();
//        }
//    }
//}
