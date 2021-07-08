using Kula.Core;

namespace Kula.Data
{
    class FuncWithEnv
    {
        public Func Func { get; }
        public FuncRuntime Runtime { get; }
        public FuncWithEnv(Func func, FuncRuntime fatherRuntime)
        {
            this.Func = func;
            this.Runtime = fatherRuntime;
        }

        public override string ToString()
        {
            return Func.ToString();
        }
    }
}
