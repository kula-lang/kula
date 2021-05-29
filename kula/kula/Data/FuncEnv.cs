using kula.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kula.Data
{
    class FuncEnv
    {
        private Func func;
        private FuncRuntime fatherRuntime;

        public Func Func { get => func; }
        public FuncRuntime Runtime { get => fatherRuntime; }
        public FuncEnv(Func func, FuncRuntime fatherRuntime)
        {
            this.func = func;
            this.fatherRuntime = fatherRuntime;
        }

        public override string ToString()
        {
            return "{lambda-env}";
        }
    }
}
