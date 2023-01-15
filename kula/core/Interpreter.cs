using Kula.Core.Ast;
using Kula.Core.Runtime;
using Kula.Core.Container;

namespace Kula.Core;

class Interpreter : Expr.Visitor<System.Object?>, Stmt.Visitor<int> {
    internal readonly Runtime.Environment globals;
    internal Runtime.Environment environment;
    private KulaEngine? kula;

    public static readonly Interpreter Instance = new Interpreter();
    private readonly Dictionary<Expr.Function, Function> functionDict = new Dictionary<Expr.Function, Function>();

    private Interpreter() {
        this.globals = new Runtime.Environment();
        this.environment = this.globals;

        foreach (KeyValuePair<string, NativeFunction> kv in StandardLibrary.global_functions) {
            globals.Define(Token.MakeTemp(kv.Key), kv.Value);
        }
        globals.Define(Token.MakeTemp("input"), new NativeFunction(0, (_, args) => kula!.Input()));
        globals.Define(Token.MakeTemp("println"), new NativeFunction(-1, (_, args) => {
            List<string> items = new List<string>();
            foreach (object? item in args) {
                items.Add(StandardLibrary.Stringify(item));
            }
            kula!.Print(string.Join(' ', items));
            return null;
        }));
        globals.Define(Token.MakeTemp("eval"), new NativeFunction(1, (_, args) => {
            string source = StandardLibrary.Assert<string>(args[0]);
            kula!.Run(source);
            return null;
        }));
        globals.Define(Token.MakeTemp("load"), new NativeFunction(1, (_, args) => {
            string path = StandardLibrary.Assert<string>(args[0]);
            kula!.Run(new FileInfo(path));
            return null;
        }));

        globals.Define(Token.MakeTemp("__string_proto__"), StandardLibrary.string_proto);
        globals.Define(Token.MakeTemp("__array_proto__"), StandardLibrary.array_proto);
        globals.Define(Token.MakeTemp("__number_proto__"), StandardLibrary.number_proto);
        globals.Define(Token.MakeTemp("__object_proto__"), StandardLibrary.object_proto);
    }

    public void Interpret(KulaEngine kula, List<Stmt> stmts) {
        this.kula = kula;
        try {
            ExecuteBlock(stmts, environment);
        }
        catch (RuntimeError runtimeError) {
            kula!.RuntimeError(runtimeError);
        }
    }

    private object? Evaluate(Expr expr) {
        return expr.Accept(this);
    }

    private void Execute(Stmt stmt) {
        stmt.Accept(this);
    }

    object? Expr.Visitor<object?>.VisitAssign(Expr.Assign expr) {
        object? value = Evaluate(expr.right);

        if (expr.left is Expr.Variable variable) {
            switch (expr.@operator.type) {
                case TokenType.COLON_EQUAL:
                    environment.Define(variable.name, value);
                    break;
                case TokenType.EQUAL:
                    environment.Assign(variable.name, value);
                    break;
            }
        }
        else if (expr.left is Expr.Get get) {
            object? container = Evaluate(get.container);
            object? key = Evaluate(get.key);

            if (container is Container.Object container_dict) {
                if (key is string key_string) {
                    container_dict.Set(key_string, value);
                }
                else {
                    throw new RuntimeError(get.@operator, "Index of 'Dict' can only be 'String'.");
                }
            }
            else if (container is Container.Array container_array) {
                if (key is double key_double) {
                    container_array.Set(key_double, value);
                }
                else {
                    throw new RuntimeError(get.@operator, "Index of 'Array' can only be 'Number'.");
                }
            }
            else {
                throw new RuntimeError(get.@operator, "Only 'Object' have properties when set.");
            }
        }

        return value;
    }

    private object EvalBinary(Token @operator, object? left, object? right) {
        if (@operator.type == TokenType.PLUS) {
            if (left is string left_string && right is string right_string) {
                return left_string + right_string;
            }
            if (left is double left_double && right is double right_double) {
                return left_double + right_double;
            }
            else {
                throw new RuntimeError(@operator, "Operands must be 2 numbers or 2 strings.");
            }
        }
        else if (@operator.type == TokenType.EQUAL_EQUAL) {
            return object.Equals(left, right);
        }
        else if (@operator.type == TokenType.BANG_EQUAL) {
            return !object.Equals(left, right);
        }
        else {
            if (left is double left_double && right is double right_double) {
                switch (@operator.type) {
                    case TokenType.MINUS:
                        return left_double - right_double;
                    case TokenType.STAR:
                        return left_double * right_double;
                    case TokenType.SLASH:
                        return left_double / right_double;
                    case TokenType.GREATER:
                        return left_double > right_double;
                    case TokenType.LESS:
                        return left_double < right_double;
                    case TokenType.GREATER_EQUAL:
                        return left_double >= right_double;
                    case TokenType.LESS_EQUAL:
                        return left_double <= right_double;
                    default:
                        throw new RuntimeError(@operator, "Undefined Operator.");
                }
            }
            else {
                throw new RuntimeError(@operator, "Operands must be numbers.");
            }
        }
    }

    object? Expr.Visitor<object?>.VisitBinary(Expr.Binary expr) {
        return EvalBinary(expr.@operator, Evaluate(expr.left), Evaluate(expr.right));
    }

    int Stmt.Visitor<int>.VisitBlock(Stmt.Block stmt) {
        ExecuteBlock(stmt.statements, new Runtime.Environment(environment));
        return 0;
    }

    internal void ExecuteBlock(List<Stmt> statements, Runtime.Environment environment) {
        Runtime.Environment previous = this.environment;

        try {
            this.environment = environment;
            foreach (Stmt statement in statements) {
                Execute(statement);
            }
        }
        finally {
            this.environment = previous;
        }
    }

    object? Expr.Visitor<object?>.VisitCall(Expr.Call expr) {
        object? callee;

        // 'this' binding
        if (expr.callee is Expr.Get expr_get) {
            EvalGet(expr_get, out object? container, out object? key, out object? value);
            if (value is ICallable value_function) {
                value_function.Bind(container);
            }
            callee = value;
        }
        else {
            callee = Evaluate(expr.callee);
        }

        // __func__
        while (callee is Container.Object functor) {
            callee = functor.Get("__func__");
        }

        if (callee is ICallable function) {
            if (function.Arity >= 0 && function.Arity != expr.arguments.Count) {
                throw new RuntimeError($"Need {function.Arity} argument(s) but {expr.arguments.Count} is given.");
            }

            List<object?> arguments = new List<object?>();
            foreach (Expr argument in expr.arguments) {
                arguments.Add(Evaluate(argument));
            }

            return function.Call(arguments);
        }
        else {
            throw new RuntimeError("Can only call functions.");
        }
    }

    int Stmt.Visitor<int>.VisitExpression(Stmt.Expression stmt) {
        Evaluate(stmt.expression);
        return 0;
    }

    object? Expr.Visitor<object?>.VisitFunction(Expr.Function expr) {
        if (!functionDict.ContainsKey(expr)) {
            functionDict[expr] = new Function(expr, this, environment);
        }
        return functionDict[expr];
    }

    object? Expr.Visitor<object?>.VisitGet(Expr.Get expr) {
        // TODO
        EvalGet(expr, out object? container, out object? key, out object? value);
        return value;
    }

    void EvalGet(Expr.Get expr, out object? container, out object? key, out object? value) {
        container = Evaluate(expr.container);
        key = Evaluate(expr.key);

        if (container is Container.Object dict) {
            if (key is string key_string) {
                value = dict.Get(key_string);
                return;
            }
            throw new RuntimeError(expr.@operator, "Index of 'Dict' can only be 'String'.");
        }
        else if (container is Container.Array array) {
            if (key is Double key_double) {
                value = array.Get(key_double);
                return;
            }
            else if (key is string key_string) {
                value = StandardLibrary.array_proto.Get(key_string);
                return;
            }
            throw new RuntimeError(expr.@operator, "Index of 'Array' can only be 'Number'.");
        }
        else if (container is String string_proto) {
            if (key is string key_string) {
                value = StandardLibrary.string_proto.Get(key_string);
                return;
            }
        }
        else if (container is double number_proto) {
            if (key is string key_string) {
                value = StandardLibrary.number_proto.Get(key_string);
                return;
            }
        }
        throw new RuntimeError(expr.@operator, "Only 'Object' have properties when get.");
    }

    int Stmt.Visitor<int>.VisitIf(Stmt.If stmt) {
        if (StandardLibrary.Booleanify(Evaluate(stmt.condition))) {
            Execute(stmt.thenBranch);
        }
        else if (stmt.elseBranch is not null) {
            Execute(stmt.elseBranch);
        }
        return 0;
    }

    object? Expr.Visitor<object?>.VisitLiteral(Expr.Literal expr) {
        return expr.value;
    }

    object? Expr.Visitor<object?>.VisitLogical(Expr.Logical expr) {
        object? left = Evaluate(expr.left);

        if ((expr.@operator.type == TokenType.OR) == StandardLibrary.Booleanify(left)) {
            return left;
        }

        return Evaluate(expr.right);
    }

    int Stmt.Visitor<int>.VisitPrint(Stmt.Print stmt) {
        List<string> items = new List<string>();

        foreach (Expr iexpr in stmt.items) {
            items.Add(StandardLibrary.Stringify(Evaluate(iexpr)));
        }

        kula!.Print(string.Join(' ', items));
        return 0;
    }

    int Stmt.Visitor<int>.VisitReturn(Stmt.Return stmt) {
        throw new Return(stmt.value is null ? null : Evaluate(stmt.value));
    }

    private object? EvalUnary(Token @operator, Expr expr) {
        object? value = Evaluate(expr);

        switch (@operator.type) {
            case TokenType.MINUS:
                if (value is double value_double) {
                    return -value_double;
                }
                throw new RuntimeError(@operator, "Operand must be a number.");
            case TokenType.BANG:
                return !StandardLibrary.Booleanify(value);
        }

        throw new RuntimeError(@operator, "Undefined Operator.");
    }

    object? Expr.Visitor<object?>.VisitUnary(Expr.Unary expr) {
        return EvalUnary(expr.@operator, expr.right);
    }

    object? Expr.Visitor<object?>.VisitVariable(Expr.Variable expr) {
        return environment.Get(expr.name);
    }

    // int Stmt.Visitor<int>.VisitWhile(Stmt.While stmt) {
    //     while (StandardLibrary.Booleanify(Evaluate(stmt.condition))) {
    //         try {
    //             Execute(stmt.branch);
    //         }
    //         catch (Break) {
    //             break;
    //         }
    //         catch (Continue) {
    //             continue;
    //         }
    //     }
    //     return 0;
    // }
    int Stmt.Visitor<int>.VisitFor(Stmt.For stmt) {
        if (stmt.initializer is not null) {
            Execute(stmt.initializer);
        }
        while (stmt.condition is null ? true : StandardLibrary.Booleanify(Evaluate(stmt.condition))) {
            try {
                Execute(stmt.body);
            }
            catch (Break) {
                break;
            }
            catch (Continue) {
                continue;
            }
            finally {
                if (stmt.increment is not null) {
                    Evaluate(stmt.increment);
                }
            }
        }
        return 0;
    }

    int Stmt.Visitor<int>.VisitVoid(Stmt.Void stmt) {
        return 0;
    }

    int Stmt.Visitor<int>.VisitBreak(Stmt.Break stmt) {
        throw new Break();
    }

    int Stmt.Visitor<int>.VisitContinue(Stmt.Continue stmt) {
        throw new Continue();
    }

    internal class Return : Exception {
        public readonly object? value;
        public Return(object? value) {
            this.value = value;
        }
    }

    internal class Break : Exception {
        public Break() { }
    }

    internal class Continue : Exception {
        public Continue() { }
    }
}