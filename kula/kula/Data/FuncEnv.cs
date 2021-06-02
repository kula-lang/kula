using kula.Core;

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
            return "{func-env}";
        }
    }
}
