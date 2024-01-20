using Kula.BytecodeCompiler.Compiler;

namespace Kula.BytecodeInterpreter.Runtime;

class VMException : Exception
{
    public readonly Instruction instruction;

    public VMException(Instruction instruction, string msg) : base(msg)
    {
        this.instruction = instruction;
    }
}