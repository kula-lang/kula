using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using kula.DataObj;

namespace kula.Core
{
    class FuncRuntime : IRuntime
    {
        private Dictionary<string, Object> varDict = new Dictionary<string, Object>();

        public FuncRuntime()
        {

        }

        public IRuntime Father => throw new NotImplementedException();
        public Dictionary<string, object> VarDict => throw new NotImplementedException();
        public void Run()
        {
            throw new NotImplementedException();
        }
    }
}
