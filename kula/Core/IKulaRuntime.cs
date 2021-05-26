using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kula.Core
{
    interface IKulaRuntime
    {
        void Run();
        IKulaRuntime Father { get; }
        Dictionary<string, object> VarDict { get; }
    }
}
