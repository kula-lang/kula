using Kula.Core;

namespace Kula.Data.Function
{
    class Func
    {
        public Lambda Lambda { get; }
        public FuncRuntime Runtime { get; }
        public Func(Lambda func, FuncRuntime fatherRuntime)
        {
            this.Lambda = func;
            this.Runtime = fatherRuntime;
        }

        public override string ToString()
        {
            return Lambda.ToString();
        }
    }
}
