using Kula.Core.Compiler;

namespace Kula.Core.VM;

class RuntimeError : Exception
{
    public readonly Instruction instruction;

    public RuntimeError(Instruction instruction, string msg) : base(msg)
    {
        this.instruction = instruction;
    }
}