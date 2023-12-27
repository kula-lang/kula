using System.Text;
using Kula.Core.Ast;

namespace Kula.Core.Compiler;

class CompiledFile
{
    static class TypeCode
    {
        public static readonly byte NONE = 0x80;
        public static readonly byte BOOL = 0x81;
        public static readonly byte DOUBLE = 0x82;
        public static readonly byte STRING = 0x83;
    }


    private static readonly ushort MAGIC_NUMBER = 0x1AC5;
    private static readonly byte SEPARATOR = 0x8f;
    internal readonly Dictionary<string, int> variableDict;
    internal readonly List<object?> literalList;
    internal readonly List<Instruction> instructions;
    internal readonly List<(List<int>, List<Instruction>)> functions = new();

    public CompiledFile(Dictionary<string, int> variableDict, List<object?> literalList, List<Instruction> instructions, List<(List<int>, List<Instruction>)> functions)
    {
        this.variableDict = variableDict;
        this.literalList = literalList;
        this.instructions = instructions;
        this.functions = functions;
    }

    public CompiledFile()
    {
        this.variableDict = new();
        this.literalList = new();
        this.instructions = new();
        this.functions = new();
    }

    public void Write(BinaryWriter bw)
    {
        // Prepare
        string[] variables = new string[variableDict.Count];
        foreach (var kv in variableDict) {
            variables[kv.Value] = kv.Key;
        }

        // Magic Number
        bw.Write(MAGIC_NUMBER);

        // Variables
        foreach (string variable in variables) {
            bw.Write((byte)variable.Length);
            bw.Write(variable.ToCharArray());
        }
        bw.Write(SEPARATOR);

        // Literal
        foreach (object? literal in literalList) {
            if (literal is string literal_string) {
                bw.Write(TypeCode.STRING);
                bw.Write(literal_string.Length);
                bw.Write(literal_string.ToCharArray());
            }
            else if (literal is double literal_double) {
                bw.Write(TypeCode.DOUBLE);
                bw.Write(literal_double);
            }
            else if (literal is bool) {
            }
            else if (literal is null) {
                bw.Write(TypeCode.NONE);
            }
            else {
                throw new CompileError($"{literal?.GetType()}");
            }
        }
        bw.Write(SEPARATOR);

        // ByteCode

        // Instructionss
        foreach (Instruction ins in this.instructions) {
            bw.Write((byte)ins.Op);
            bw.Write(ins.Constant);
        }
        bw.Write(SEPARATOR);

        // Functions
        bw.Write(functions.Count);
        foreach ((List<int> parameters, List<Instruction> instructions) in functions) {
            bw.Write((byte)parameters.Count);
            foreach (int parameter in parameters) {
                bw.Write(parameter);
            }

            foreach (Instruction ins in instructions) {
                bw.Write((byte)ins.Op);
                bw.Write(ins.Constant);
            }
            bw.Write(SEPARATOR);
        }
    }

    public void Read(BinaryReader br)
    {
        this.variableDict.Clear();
        this.literalList.Clear();
        this.instructions.Clear();
        this.functions.Clear();
        literalList.Add(false);
        literalList.Add(true);

        // Magic Number
        ushort magic_number = br.ReadUInt16();
        if (magic_number != MAGIC_NUMBER) {
            throw new CompileError();
        }

        byte byte_buffer;
        // Variables
        while ((byte_buffer = br.ReadByte()) != SEPARATOR) {
            byte var_size = byte_buffer;
            char[] chars = br.ReadChars(var_size);
            variableDict[new string(chars)] = variableDict.Count;
        }

        // Literal
        while ((byte_buffer = br.ReadByte()) != SEPARATOR) {
            byte literal_type = byte_buffer;
            if (literal_type == TypeCode.STRING) {
                int size = br.ReadInt32();
                char[] chars = br.ReadChars(size);
                literalList.Add(new string(chars));
            }
            else if (literal_type == TypeCode.DOUBLE) {
                double val = br.ReadDouble();
                literalList.Add(val);
            }
            else if (literal_type == TypeCode.BOOL) {
            }
            else if (literal_type == TypeCode.NONE) {
                literalList.Add(null);
            }
            else {
                throw new CompileError();
            }
        }

        // ByteCode
        while ((byte_buffer = br.ReadByte()) != SEPARATOR) {
            byte opCode = byte_buffer;
            int constant = br.ReadInt32();
            instructions.Add(new Instruction((OpCode)opCode, constant));
        }

        // Functions
        int functions_count = br.ReadInt32();
        for (int i = 0; i < functions_count; ++i) {
            byte size = br.ReadByte();
            List<int> parameters = new();
            List<Instruction> instructions = new();
            for (int j = 0; j < size; ++j) {
                parameters.Add(br.ReadInt32());
            }
            while ((byte_buffer = br.ReadByte()) != SEPARATOR) {
                byte opCode = byte_buffer;
                int constant = br.ReadInt32();
                instructions.Add(new Instruction((OpCode)opCode, constant));
            }
            functions.Add((parameters, instructions));
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new();

        sb.AppendLine("==== Variable ====");
        int index;
        index = 0;
        foreach (var kv in this.variableDict) {
            sb.AppendLine($"\t{index++}\t{kv.Key}");
        }

        sb.AppendLine("==== Literal ====");
        index = 0;
        foreach (var literal in this.literalList) {
            sb.AppendLine($"\t{index++}\t{literal}");
        }

        sb.AppendLine("==== Instructions ====");
        index = 0;
        foreach (var ins in this.instructions) {
            sb.AppendLine($"\t{index++}\t{ins.Op}\t{ins.Constant}");
        }

        sb.AppendLine("==== Functions ====");
        int f_index = 0;
        foreach (var function in this.functions) {
            sb.AppendLine($"== F {f_index} ==");
            index = 0;
            foreach (var vv in function.Item1) {
                sb.AppendLine($"\t{index++}\t{vv}");
            }
            sb.AppendLine($"== I {f_index} ==");
            index = 0;
            foreach (var ins in function.Item2) {
                sb.AppendLine($"\t{index++}\t{ins.Op}\t{ins.Constant}");
            }
            ++f_index;
        }

        return sb.ToString();
    }
}