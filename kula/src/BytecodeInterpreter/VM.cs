using Kula.ASTInterpreter.Runtime;
using Kula.BytecodeCompiler.Compiler;
using Kula.BytecodeInterpreter.Runtime;
using Kula.Runtime;
using Kula.Utilities;

namespace Kula.BytecodeInterpreter;

class VM
{
    private CompiledFile compiledFile = null!;
    private readonly Context globals;
    private Context context;
    private readonly Stack<Stack<object?>> vmStack;
    private readonly Stack<(int, int, Context)> callStack;
    private int ip, fp;

    public VM() : this(200) { }

    public VM(int maxDepth)
    {
        this.globals = new();
        this.context = globals;
        this.vmStack = new();
        this.callStack = new();

        foreach (var kv in StandardLibrary.global_functions) {
            globals.Define(kv.Key, kv.Value);
        }

        foreach (var kv in StandardLibrary.global_protos) {
            globals.Define(kv.Key, kv.Value);
        }

        CoreFunctions();
    }

    private void CoreFunctions()
    {
        globals.Define("input", new NativeFunction(0, (_, args) => Console.ReadLine()));
        globals.Define("println", new NativeFunction(-1, (_, args) => {
            List<string> items = new();
            foreach (object? item in args) {
                items.Add(StandardLibrary.Stringify(item));
            }
            Console.WriteLine(string.Join(' ', items));
            return null;
        }));
    }

    public void Interpret(KulaEngine kulaEngine, CompiledFile compiledFile)
    {
        this.compiledFile = compiledFile;
        this.vmStack.Clear();
        this.vmStack.Push(new());
        this.callStack.Clear();
        ip = 0;
        fp = -1;

        while (fp >= 0 ? (ip < compiledFile.functions[fp].Item2.Count) : (ip < compiledFile.instructions.Count)) {
            Instruction ins;
            if (fp >= 0) {
                ins = compiledFile.functions[fp].Item2[ip];
#if DEBUG_MODE
                Console.WriteLine($"\t[Do in f{fp}]:\t{compiledFile.InstructionToString(ins)}");
#endif
            }
            else {
                ins = compiledFile.instructions[ip];
#if DEBUG_MODE
                Console.WriteLine($"\t[Do]:\t{compiledFile.InstructionToString(ins)}");
#endif
            }
            Run(ins);
#if DEBUG_MODE
            PrintStack();
#endif
            ip++;

            if (fp >= 0) {
                if (ip >= compiledFile.functions[fp].Item2.Count) {
                    vmStack.Pop().Clear();
                    vmStack.Peek().Push(null);
                    (ip, fp, context) = callStack.Pop();
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
                vmStack.Peek().Push(compiledFile.literals[i.Value]);
                break;
            case OpCode.LOAD: {
                    try {
                        var v = context.Get(compiledFile.symbolArray[i.Value]);
                        vmStack.Peek().Push(v);
                    }
                    catch (InterpreterInnerException rie) {
                        throw new VMException(i, rie.Message);
                    }
                    break;
                }
            case OpCode.DECL:
                top = vmStack.Peek().Peek();
                context.Define(compiledFile.symbolArray[i.Value], top);
                break;
            case OpCode.ASGN:
                top = vmStack.Peek().Peek();
                context.Assign(compiledFile.symbolArray[i.Value], top);
                break;
            case OpCode.POP:
                vmStack.Peek().Pop();
                break;
            case OpCode.DUP:
                vmStack.Peek().Push(vmStack.Peek().Peek());
                break;
            case OpCode.FUNC:
                vmStack.Peek().Push(new VMFunction(i.Value, context, compiledFile.functions[i.Value].Item1.Count));
                break;
            case OpCode.RET: {
                    top = null;
                    vmStack.Pop().Clear();
                    vmStack.Peek().Push(top);
                    (ip, fp, context) = callStack.Pop();
                    break;
                }
            case OpCode.RETV: {
                    top = vmStack.Peek().Pop();
                    vmStack.Pop().Clear();
                    vmStack.Peek().Push(top);
                    (ip, fp, context) = callStack.Pop();
                    break;
                }
            case OpCode.ENVST:
                context = new(context);
                break;
            case OpCode.ENVED:
                context = context.Unenclose();
                break;
            case OpCode.GET: {
                    object? key = vmStack.Peek().Pop();
                    object? container = vmStack.Peek().Pop();
                    object? value = EvalGet(container, key, i);
                    vmStack.Peek().Push(value);
                    break;
                }
            case OpCode.GETWT: {
                    object? key = vmStack.Peek().Pop();
                    object? container = vmStack.Peek().Pop();
                    object? value = EvalGet(container, key, i);
                    vmStack.Peek().Push(container);
                    vmStack.Peek().Push(value);
                    break;
                }
            case OpCode.SET: {
                    object? value = vmStack.Peek().Pop();
                    object? key = vmStack.Peek().Pop();
                    object? container = vmStack.Peek().Pop();
                    if (container is KulaObject dict && key is string key_string) {
                        dict.Set(key_string, value);
                    }
                    else if (container is KulaArray array && key is int key_int) {
                        array.Set(key_int, value);
                    }
                    else {
                        throw new VMException(i, $"Cannot set key '{key}' to container '{container}'.");
                    }
                    vmStack.Peek().Push(value);
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
                        throw new VMException(i, "Operands must be 2 numbers or 2 strings.");
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
                        throw new VMException(i, "Operands must be 2 numbers.");
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
                        throw new VMException(i, "Operands must be 2 numbers.");
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
                        throw new VMException(i, "Operands must be 2 numbers.");
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
                        throw new VMException(i, "Operands must be 2 numbers.");
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
                        throw new VMException(i, "Operands must be 2 numbers.");
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
                        throw new VMException(i, "Operands must be 2 numbers.");
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
                        throw new VMException(i, "Operands must be 2 numbers.");
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
                        throw new VMException(i, "Operands must be 2 numbers.");
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
                        throw new VMException(i, "Operands must number.");
                    }
                    break;
                }
            case OpCode.NOT: {
                    top = vmStack.Peek().Pop();
                    vmStack.Peek().Push(!StandardLibrary.Booleanify(top));
                    break;
                }
            case OpCode.CALL: {
                    int argc = i.Value;
                    object?[] argv = new object?[argc];
                    for (int c = argc - 1; c >= 0; --c) {
                        argv[c] = vmStack.Peek().Pop();
                    }
                    object? function = vmStack.Peek().Pop();

                    if (function is NativeFunction nf) {
                        vmStack.Peek().Push(nf.Call(new(argv)));
                    }
                    else if (function is VMFunction fo) {
                        CalcFunctionObject(fo, argv);
                    }
                    else if (function is KulaObject obj && obj.Get("__func__") is VMFunction fo_obj) {
                        CalcFunctionObject(fo_obj, argv);
                    }
                    else {
                        throw new VMException(i, "Can only call functions.");
                    }
                    break;
                }
            case OpCode.CALWT: {
                    int argc = i.Value;
                    object?[] argv = new object?[argc];
                    for (int c = argc - 1; c >= 0; --c) {
                        argv[c] = vmStack.Peek().Pop();
                    }
                    object? function = vmStack.Peek().Pop();
                    object? container = vmStack.Peek().Pop();

                    if (function is NativeFunction nf) {
                        nf.Bind(container);
                        vmStack.Peek().Push(nf.Call(new(argv)));
                    }
                    else if (function is VMFunction fo) {
                        fo.Bind(container);
                        CalcFunctionObject(fo, argv);
                    }
                    else if (function is KulaObject obj && obj.Get("__func__") is VMFunction fo_obj) {
                        fo_obj.Bind(container);
                        CalcFunctionObject(fo_obj, argv);
                    }
                    else {
                        throw new VMException(i, "Can only call functions.");
                    }
                    break;
                }
            case OpCode.PRINT: {
                    string[] ls = new string[i.Value];
                    for (int t = i.Value - 1; t >= 0; --t) {
                        ls[t] = StandardLibrary.Stringify(vmStack.Peek().Pop());
                    }
                    Console.WriteLine(string.Join(' ', ls));
                    break;
                }
            case OpCode.JMP:
                ip = i.Value - 1;
                break;
            case OpCode.JMPT:
                if (StandardLibrary.Booleanify(vmStack.Peek().Pop())) {
                    ip = i.Value - 1;
                }
                break;
            case OpCode.JMPF:
                if (!StandardLibrary.Booleanify(vmStack.Peek().Pop())) {
                    ip = i.Value - 1;
                }
                break;
            default:
#if DEBUG_MODE
                Console.WriteLine($"Unsupported Instruction '{i.Op}'.");
#endif
                break;

        }
    }

    private void CalcFunctionObject(VMFunction vmf, object?[] argv)
    {
        callStack.Push((ip, fp, context));
        ip = -1;
        fp = vmf.Fp;
        var function = this.compiledFile.functions[vmf.Fp];
        if (argv.Length != function.Item1.Count) {
            throw new Exception();
        }

        this.context = new(vmf.Parent);
        vmStack.Push(new());
        for (int i = 0; i < argv.Length; ++i) {
            int v_index = function.Item1[i];
            string v_name = this.compiledFile.symbolArray[v_index];
            this.context.Define(v_name, argv[i]);
        }
        this.context.Define("self", vmf);
        if (vmf.CallSite != null) {
            this.context.Define("this", vmf.CallSite);
            vmf.Bind(null);
        }
    }

    private static object? EvalGet(object? container, object? key, Instruction ins)
    {
        if (container is KulaObject dict) {
            if (key is string key_string) {
                return dict.Get(key_string);
            }
            throw new VMException(ins, "Index of 'Object' can only be 'String'.");
        }
        else if (container is KulaArray array) {
            if (key is double key_double) {
                return array.Get(key_double);
            }
            else if (key is string key_string) {
                return StandardLibrary.array_proto.Get(key_string);
            }
            throw new VMException(ins, "Index of 'Array' can only be 'Number'.");
        }
        else if (container is string) {
            if (key is string key_string) {
                return StandardLibrary.string_proto.Get(key_string);
            }
        }
        else if (container is double && key is string key_string) {
            return StandardLibrary.number_proto.Get(key_string);
        }
        else if (container is ICallable && key is string key_string2) {
            return StandardLibrary.function_proto.Get(key_string2);
        }
        throw new VMException(ins, "What do you want to get?");
    }

    private void PrintStack()
    {
        List<string> ls = new();
        foreach (var stack in vmStack) {
            List<string> strings = new();
            foreach (var value in stack) {
                strings.Add(StandardLibrary.Stringify(value));
            }
            ls.Add($"[{string.Join(",", strings)}]");
        }
        Console.WriteLine($"Stack: [{string.Join(" ", ls)}]");
    }
}