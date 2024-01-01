namespace Kula.Core.Compiler;

enum OpCode : byte
{
    __START__ = 0x80,
    // Load & Store
    LOADC, LOAD, DECL, ASGN, POP,
    // Function
    FUNC, RET,
    // Env
    BLKST, BLKEND,
    // Container
    GET, SET,
    // Arithmetic
    ADD, SUB, MUL, DIV, MOD, NEG, NOT,
    // Comparison
    EQ, NEQ, LT, LE, GT, GE,
    // Control Flow
    JMP, JMPT, JMPF, CALL,
    // Output
    PRINT
}

class Instruction
{
    internal OpCode Op { get; set; }
    internal int Constant { get; set; }

    public Instruction(OpCode op, int constant)
    {
        this.Op = op;
        this.Constant = constant;
    }

    public static int ReadConstant(BinaryReader br, OpCode op)
    {
        switch (CodeSize(op)) {
            case sizeof(int):
                return br.ReadInt32();
            case sizeof(short):
                return br.ReadInt16();
            case sizeof(byte):
                return br.ReadByte();
            default:
                return 0;
        }
    }

    private static int CodeSize(OpCode op)
    {
        switch (op) {
            case OpCode.LOADC:
            case OpCode.LOAD:
            case OpCode.DECL:
            case OpCode.ASGN:
            case OpCode.JMP:
            case OpCode.JMPF:
            case OpCode.JMPT:
                return sizeof(int);
            case OpCode.FUNC:
            case OpCode.RET:
                return sizeof(byte);
            default:
                return 0;
        }
    }
}
