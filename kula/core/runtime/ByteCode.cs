namespace Kula.Core.Runtime;

enum OpCode : Byte
{
    CONSTANT = 0, VARIABLE, ASSIGN,
    PLUS, MINUS, STAR, SLASH,
    CALL,
    RETURN,
}

class ConstantValue
{
    public int type;
    public object? value;
}

struct ByteCode
{
    OpCode op;
    ConstantValue? value;
}
