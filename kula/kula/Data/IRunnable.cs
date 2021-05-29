using kula.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kula.Data
{
    interface IRunnable
    {
        List<LexToken> TokenStream { get; }
        List<KvmNode> NodeStream { get; }
        IRuntime FatherRuntime { get; set; }

        List<Type> ArgTypes { get; }
        List<string> ArgNames { get; }
        Type ReturnType { get; set; }
    }
}
