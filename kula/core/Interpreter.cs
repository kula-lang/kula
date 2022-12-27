using Kula.Core.Ast;
using Kula.Core.Runtime;

namespace Kula.Core;

class Interpreter : Expr.Visitor<Object?>, Stmt.Visitor<int> {
    internal readonly Runtime.Environment globals;
    internal Runtime.Environment environment;
    private KulaEngine? kula;

    public static readonly Interpreter Instance = new Interpreter();

    public void Interpret(KulaEngine kula, List<Stmt> stmts) {
        this.kula = kula;
        try {
            ExecuteBlock(stmts, environment);
        }
        catch (RuntimeError runtimeError) {
            kula!.RuntimeError(runtimeError);
        }
    }

    private Interpreter() {
        this.globals = new Runtime.Environment();
        this.environment = this.globals;
    }

    private object? Evaluate(Expr expr) {
        return expr.Accept(this);
    }

    private void Execute(Stmt stmt) {
        stmt.Accept(this);
    }

    private string Stringify(object? @object) {
        if (@object is bool object_bool) {
            return object_bool.ToString().ToLower();
        }
        else if (@object is double object_double) {
            return object_double.ToString();
        }

        return @object!.ToString() ?? "null";
    }

    private bool IsTruly(object? @object) {
        if (@object is null) {
            return false;
        }
        if (@object is bool object_bool) {
            return object_bool;
        }

        return true;
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
            // TODO
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
        ExecuteBlock(stmt.statements, new Runtime.Environment());
        return 0;
    }

    internal void ExecuteBlock(List<Stmt> statements, Runtime.Environment environment) {
        Runtime.Environment previous = this.environment;

        this.environment = environment;
        foreach (Stmt statement in statements) {
            Execute(statement);
        }

        this.environment = previous;
    }

    object? Expr.Visitor<object?>.VisitCall(Expr.Call expr) {
        object? callee = Evaluate(expr.callee);
        if (callee is ICallable function) {
            if (function.Arity != expr.arguments.Count) {
                throw new RuntimeError(
                    new Token(TokenType.ARROW, "=>", null, -1), 
                    $"Need {function.Arity} argument(s) but {expr.arguments.Count} is given.");
            }

            List<object?> arguments = new List<object?>();
            foreach (Expr argument in expr.arguments) {
                arguments.Add(Evaluate(argument));
            }

            return function.Call(arguments);
        }
        else {
            throw new RuntimeError(new Token(TokenType.ARROW, "=>", null, -1), "Can only call functions.");
        }
    }

    int Stmt.Visitor<int>.VisitExpression(Stmt.Expression stmt) {
        Evaluate(stmt.expression);
        return 0;
    }

    object? Expr.Visitor<object?>.VisitFunction(Expr.Function expr) {
        return new Function(expr, this);
    }

    object? Expr.Visitor<object?>.VisitGet(Expr.Get expr) {
        // TODO
        throw new NotImplementedException();
    }

    int Stmt.Visitor<int>.VisitIf(Stmt.If stmt) {
        if (IsTruly(Evaluate(stmt.condition))) {
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

        if ((expr.@operator.type == TokenType.OR) == IsTruly(left)) {
            return left;
        }

        return Evaluate(expr.right);
    }

    int Stmt.Visitor<int>.VisitPrint(Stmt.Print stmt) {
        List<string> items = new List<string>();

        foreach (Expr iexpr in stmt.items) {
            items.Add(Stringify(Evaluate(iexpr)));
        }

        kula!.Print(string.Join(' ', items));
        return 0;
    }

    int Stmt.Visitor<int>.VisitReturn(Stmt.Return stmt) {
        throw new Return(stmt.value is null ? null : Evaluate(stmt.value));
    }

    int Stmt.Visitor<int>.VisitTypeDefine(Stmt.TypeDefine stmt) {
        return 0;
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
                return !IsTruly(value);
        }
        
        throw new RuntimeError(@operator, "Undefined Operator.");
    }

    object? Expr.Visitor<object?>.VisitUnary(Expr.Unary expr) {
        return EvalUnary(expr.@operator, expr.right);
    }

    object? Expr.Visitor<object?>.VisitVariable(Expr.Variable expr) {
        return environment.Get(expr.name);
    }

    int Stmt.Visitor<int>.VisitWhile(Stmt.While stmt) {
        while (IsTruly(Evaluate(stmt.condition))) {
            Execute(stmt.branch);
        }
        return 0;
    }

    internal class Return : Exception {
        public readonly object? value;
        public Return(object? value) {
            this.value = value;
        }
    }
}