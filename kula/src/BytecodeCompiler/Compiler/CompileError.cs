namespace Kula.BytecodeCompiler.Compiler;

class CompileError : Exception
{
    public CompileError() { }
    public CompileError(string msg) : base(msg) { }
}