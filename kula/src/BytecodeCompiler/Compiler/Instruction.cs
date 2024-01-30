namespace Kula.BytecodeCompiler.Compiler;

enum OpCode : byte
{
    __MEM__ = 0x00,
    // Load & Store
    LOADC, LOAD, DECL, ASGN, POP, DUP,
    // Control Flow
    JMP, JMPT, JMPF, CALL, CALWT,
    // Function
    FUNC, RET,
    // Env
    ENVST, ENVED,
    // Container
    GET, SET, GETWT,

    __CALC__ = 0x40,
    // Arithmetic
    ADD, SUB, MUL, DIV, MOD, NEG, NOT,
    // Comparison
    EQ, NEQ, LT, LE, GT, GE,
    // Output
    PRINT
}

struct Instruction
{
    internal OpCode Op { get; set; }
    internal int Value { get; set; }

    public Instruction(OpCode op, int value)
    {
        this.Op = op;
        this.Value = value;
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
                bw.Write((uint)ins.Value);
                break;
            case sizeof(ushort):
                bw.Write((ushort)ins.Value);
                break;
            case sizeof(byte):
                bw.Write((byte)ins.Value);
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
            case OpCode.CALL:
            case OpCode.CALWT:
                return sizeof(byte);
            default:
                return 0;
        }
    }

    public override string ToString()
    {
        return $"{{{Op},{Value}}}";
    }
}
