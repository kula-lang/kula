using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kula.Core
{
    interface IRuntime
    {
        void Run();
        IRuntime Father { get; }
        Dictionary<string, object> VarDict { get; }
    }
}
