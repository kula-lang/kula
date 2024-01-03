namespace Kula.Core.Compiler;

enum OpCode : byte
{
    __MEM__ = 64,
    // Load & Store
    LOADC, LOAD, DECL, ASGN, POP, DUP,
    // Control Flow
    JMP, JMPT, JMPF, CALL,
    // Function
    FUNC, RET,
    // Env
    BLKST, BLKEND,
    // Container
    GET, SET,

    __CALC__ = 96,
    // Arithmetic
    ADD, SUB, MUL, DIV, MOD, NEG, NOT,
    // Comparison
    EQ, NEQ, LT, LE, GT, GE,
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

    public static void WriteInstruction(BinaryWriter bw, Instruction ins)
    {
        bw.Write((byte)ins.Op);
        switch (CodeSize(ins.Op)) {
            case sizeof(uint):
                bw.Write((uint)ins.Constant);
                break;
            case sizeof(ushort):
                bw.Write((ushort)ins.Constant);
                break;
            case sizeof(byte):
                bw.Write((byte)ins.Constant);
                break;
            default:
                break;
        }
    }

    private static int CodeSize(OpCode op)
    {
        switch (op) {
            case OpCode.LOADC:
            case OpCode.LOAD:
            case OpCode.DECL:
            case OpCode.ASGN:
                return sizeof(ushort);
            case OpCode.JMP:
            case OpCode.JMPF:
            case OpCode.JMPT:
                return sizeof(ushort);
            case OpCode.FUNC:
            case OpCode.RET:
            case OpCode.PRINT:
                return sizeof(byte);
            default:
                return 0;
        }
    }
}
