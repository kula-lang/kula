using Kula.Core.Compiler;
using Kula.Core.Runtime;

namespace Kula.Core.VM;

class VM
{
    private CompiledFile compiledFile = null!;
    private KulaEngine kulaEngine = null!;
    private readonly Runtime.Environment globals;
    private Runtime.Environment environment;
    private readonly Stack<Stack<object?>> vmStack;
    private readonly Stack<(int, int, Runtime.Environment)> callStack;
    private int ip, fp;

    public VM() : this(200) { }

    public VM(int maxDepth)
    {
        this.globals = new();
        this.environment = globals;
        this.vmStack = new();
        this.callStack = new();

        foreach (KeyValuePair<string, NativeFunction> kv in StandardLibrary.global_functions) {
            globals.Define(kv.Key, kv.Value);
        }
    }

    public void Interpret(KulaEngine kulaEngine, CompiledFile compiledFile)
    {
        this.compiledFile = compiledFile;
        this.kulaEngine = kulaEngine;
        this.vmStack.Clear();
        this.vmStack.Push(new());
        this.callStack.Clear();
        ip = 0;
        fp = -1;

        while (fp >= 0 ? (ip < compiledFile.functions[fp].Item2.Count) : (ip < compiledFile.instructions.Count)) {
            Instruction ins;
            if (fp >= 0) {
                ins = compiledFile.functions[fp].Item2[ip];
                Console.WriteLine($"Do in f{fp}:\t{compiledFile.InstructionToString(ins)}");
            }
            else {
                ins = compiledFile.instructions[ip];
                Console.WriteLine($"Do:\t{compiledFile.InstructionToString(ins)}");
            }
            Run(ins);
            ip++;

            if (fp >= 0) {
                if (ip >= compiledFile.functions[fp].Item2.Count) {
                    (ip, fp, environment) = callStack.Pop();
                    ip++;
                }
            }
            else {
                if (ip >= compiledFile.instructions.Count) {
                    break;
                }
            }
        }
    }

    private void Run(Instruction i)
    {
        object? top;
        switch (i.Op) {
            case OpCode.LOADC:
                vmStack.Peek().Push(compiledFile.literalList[i.Constant]);
                break;
            case OpCode.LOAD:
                vmStack.Peek().Push(environment.Get(compiledFile.variableArray[i.Constant]));
                break;
            case OpCode.DECL:
                top = vmStack.Peek().Peek();
                environment.Define(compiledFile.variableArray[i.Constant], top);
                break;
            case OpCode.ASGN:
                top = vmStack.Peek().Peek();
                environment.Assign(compiledFile.variableArray[i.Constant], top);
                break;
            case OpCode.POP:
                top = vmStack.Peek().Pop();
                break;
            case OpCode.FUNC:
                vmStack.Peek().Push(new FunctionObject(i.Constant, environment));
                break;
            case OpCode.RET:
                top = vmStack.Peek().Pop();
                vmStack.Pop().Clear();
                vmStack.Peek().Push(top);
                (ip, fp, environment) = callStack.Pop();
                break;
            case OpCode.BLKST:
                environment = new(environment);
                break;
            case OpCode.BLKEND:
                environment = environment.Unenclose();
                break;
            case OpCode.GET: {
                    object? key = vmStack.Peek().Pop();
                    object? container = vmStack.Peek().Pop();
                    object? value = EvalGet(container, key);
                    vmStack.Peek().Push(value);
                    if (value is FunctionObject fo) {
                        fo.Bind(container);
                    }
                    break;
                }
            case OpCode.SET: {
                    object? value = vmStack.Peek().Pop();
                    object? key = vmStack.Peek().Pop();
                    object? container = vmStack.Peek().Pop();
                    if (container is Container.Object dict && key is string key_string) {
                        dict.Set(key_string, value);
                    }
                    else if (container is Container.Array array && key is int key_int) {
                        array.Set(key_int, value);
                    }
                    else {
                        throw new Exception();
                    }
                    break;
                }
            case OpCode.ADD: {
                    object? v2 = vmStack.Peek().Pop();
                    object? v1 = vmStack.Peek().Pop();
                    if (v1 is double v1_double && v2 is double v2_double) {
                        vmStack.Peek().Push(v1_double + v2_double);
                    }
                    else if (v1 is string v1_string && v2 is string v2_string) {
                        vmStack.Peek().Push(v1_string + v2_string);
                    }
                    else {
                        throw new Exception();
                    }
                    break;
                }
            case OpCode.SUB: {
                    object? v2 = vmStack.Peek().Pop();
                    object? v1 = vmStack.Peek().Pop();
                    if (v1 is double v1_double && v2 is double v2_double) {
                        vmStack.Peek().Push(v1_double - v2_double);
                    }
                    else {
                        throw new Exception();
                    }
                    break;
                }
            case OpCode.MUL: {
                    object? v2 = vmStack.Peek().Pop();
                    object? v1 = vmStack.Peek().Pop();
                    if (v1 is double v1_double && v2 is double v2_double) {
                        vmStack.Peek().Push(v1_double * v2_double);
                    }
                    else {
                        throw new Exception();
                    }
                    break;
                }
            case OpCode.DIV: {
                    object? v2 = vmStack.Peek().Pop();
                    object? v1 = vmStack.Peek().Pop();
                    if (v1 is double v1_double && v2 is double v2_double) {
                        vmStack.Peek().Push(v1_double / v2_double);
                    }
                    else {
                        throw new Exception();
                    }
                    break;
                }
            case OpCode.MOD: {
                    object? v2 = vmStack.Peek().Pop();
                    object? v1 = vmStack.Peek().Pop();
                    if (v1 is double v1_double && v2 is double v2_double) {
                        vmStack.Peek().Push((double)((int)v1_double % (int)v2_double));
                    }
                    else {
                        throw new Exception();
                    }
                    break;
                }
            case OpCode.GT: {
                    object? v2 = vmStack.Peek().Pop();
                    object? v1 = vmStack.Peek().Pop();
                    if (v1 is double v1_double && v2 is double v2_double) {
                        vmStack.Peek().Push(v1_double > v2_double);
                    }
                    else {
                        throw new Exception();
                    }
                    break;
                }
            case OpCode.GE: {
                    object? v2 = vmStack.Peek().Pop();
                    object? v1 = vmStack.Peek().Pop();
                    if (v1 is double v1_double && v2 is double v2_double) {
                        vmStack.Peek().Push(v1_double >= v2_double);
                    }
                    else {
                        throw new Exception();
                    }
                    break;
                }
            case OpCode.LT: {
                    object? v2 = vmStack.Peek().Pop();
                    object? v1 = vmStack.Peek().Pop();
                    if (v1 is double v1_double && v2 is double v2_double) {
                        vmStack.Peek().Push(v1_double < v2_double);
                    }
                    else {
                        throw new Exception();
                    }
                    break;
                }
            case OpCode.LE: {
                    object? v2 = vmStack.Peek().Pop();
                    object? v1 = vmStack.Peek().Pop();
                    if (v1 is double v1_double && v2 is double v2_double) {
                        vmStack.Peek().Push(v1_double <= v2_double);
                    }
                    else {
                        throw new Exception();
                    }
                    break;
                }
            case OpCode.EQ: {
                    object? v2 = vmStack.Peek().Pop();
                    object? v1 = vmStack.Peek().Pop();
                    vmStack.Peek().Push(Equals(v1, v2));
                    break;
                }
            case OpCode.NEQ: {
                    object? v2 = vmStack.Peek().Pop();
                    object? v1 = vmStack.Peek().Pop();
                    vmStack.Peek().Push(!Equals(v1, v2));
                    break;
                }
            case OpCode.NEG: {
                    top = vmStack.Peek().Pop();
                    if (top is double v_double) {
                        vmStack.Peek().Push(-v_double);
                    }
                    else {
                        throw new Exception();
                    }
                    break;
                }
            case OpCode.NOT: {
                    top = vmStack.Peek().Pop();
                    vmStack.Peek().Push(!StandardLibrary.Booleanify(top));
                    break;
                }
            case OpCode.CALL: {
                    int argc = i.Constant;
                    object?[] argv = new object?[argc];
                    for (int c = argc - 1; c >= 0; --c) {
                        argv[c] = vmStack.Peek().Pop();
                    }
                    object? function = vmStack.Peek().Pop();

                    if (function is NativeFunction nf) {
                        vmStack.Peek().Push(nf.Call(new(argv)));
                    }
                    else if (function is FunctionObject fo) {
                        CalcFunctionObject(fo, argv);
                    }
                    break;
                }
            case OpCode.PRINT: {
                    object?[] ls = new object[i.Constant];
                    for (int t = i.Constant - 1; t >= 0; --t) {
                        ls[t] = vmStack.Peek().Pop();
                    }
                    kulaEngine.Print(string.Join(' ', ls));
                    break;
                }
            case OpCode.JMP:
                ip = i.Constant - 1;
                break;
            case OpCode.JMPT:
                if (StandardLibrary.Booleanify(vmStack.Peek().Pop())) {
                    ip = i.Constant - 1;
                }
                break;
            case OpCode.JMPF:
                if (!StandardLibrary.Booleanify(vmStack.Peek().Pop())) {
                    ip = i.Constant - 1;
                }
                break;
            default:
                Console.WriteLine($"Unsupported Instruction '{i.Op}'.");
                break;

        }
    }

    private void CalcFunctionObject(FunctionObject fo, object?[] argv)
    {
        callStack.Push((ip, fp, environment));
        ip = -1;
        fp = fo.Fp;
        var function = this.compiledFile.functions[fo.Fp];
        if (argv.Length != function.Item1.Count) {
            throw new Exception();
        }

        this.environment = new(fo.Parent);
        vmStack.Push(new());
        for (int i = 0; i < argv.Length; ++i) {
            int v_index = function.Item1[i];
            string v_name = this.compiledFile.variableArray[v_index];
            this.environment.Define(v_name, argv[i]);
        }
        this.environment.Define("self", fo);
        this.environment.Define("this", fo.CallSite);
    }

    private object? EvalGet(object? container, object? key)
    {
        if (container is Container.Object dict) {
            if (key is string key_string) {
                return dict.Get(key_string);
            }
            throw new Exception("Index of 'Dict' can only be 'String'.");
        }
        else if (container is Container.Array array) {
            if (key is double key_double) {
                return array.Get(key_double);
            }
            else if (key is string key_string) {
                return StandardLibrary.array_proto.Get(key_string);
            }
            throw new Exception("Index of 'Array' can only be 'Number'.");
        }
        else if (container is string string_proto) {
            if (key is string key_string) {
                return StandardLibrary.string_proto.Get(key_string);
            }
        }
        else if (container is double number_proto) {
            if (key is string key_string) {
                return StandardLibrary.number_proto.Get(key_string);
            }
        }
        else if (container is ICallable function_proto) {
            if (key is string key_string) {
                return StandardLibrary.function_proto.Get(key_string);
            }
        }
        throw new Exception("What do you want to get?");
    }
}