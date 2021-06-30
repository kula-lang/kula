using Kula.Core;

namespace Kula.Data
{
    class FuncEnv
    {
        private readonly Func func;
        private readonly FuncRuntime fatherRuntime;


        public Func Func { get => func; }
        public FuncRuntime Runtime { get => fatherRuntime; }
        public FuncEnv(Func func, FuncRuntime fatherRuntime)
        {
            this.func = func;
            this.fatherRuntime = fatherRuntime;
        }

        public override string ToString()
        {
            return func.ToString();
        }
    }
}
