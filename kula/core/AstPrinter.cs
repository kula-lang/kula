using Kula.Core.Ast;

namespace Kula.Core;

class AstPrinter : Stmt.Visitor<string>, Expr.Visitor<string> {
    public string Print(Stmt stmt) {
        return print(stmt);
    }

    string print(Expr expr) {
        return expr.Accept(this);
    }

    string print(Stmt stmt) {
        return stmt.Accept(this);
    }

    string Expr.Visitor<string>.VisitGet(Expr.Get expr) {
        return $"(get {print(expr.container)} {print(expr.key)})";
    }

    string Expr.Visitor<string>.VisitAssign(Expr.Assign expr) {
        return $"(assign {expr.@operator.lexeme} {print(expr.left)} {print(expr.right)})";
    }

    string Expr.Visitor<string>.VisitBinary(Expr.Binary expr) {
        return $"({expr.@operator.lexeme} {print(expr.left)} {print(expr.right)})";
    }

    string Stmt.Visitor<string>.VisitBlock(Stmt.Block stmt) {
        List<string> items = new List<string>();
        foreach (Stmt istmt in stmt.statements) {
            items.Add(print(istmt));
        }
        return $"(block {string.Join(' ', items)})";
    }

    string Expr.Visitor<string>.VisitCall(Expr.Call expr) {
        List<string> items = new List<string>();
        foreach (Expr iexpr in expr.arguments) {
            items.Add(iexpr.Accept(this));
        }
        return $"({print(expr.callee)} {string.Join(' ', items)})";
    }

    string Stmt.Visitor<string>.VisitExpression(Stmt.Expression stmt) {
        return stmt.expression.Accept(this);
    }

    string Expr.Visitor<string>.VisitFunction(Expr.Function expr) {
        List<string> parameters = new List<string>();
        foreach (Token token in expr.parameters) {
            parameters.Add(token.lexeme);
        }

        List<string> block = new List<string>();
        foreach (Stmt statement in expr.body) {
            block.Add(print(statement));
        }
        return $"(lambda ({string.Join(' ', parameters)}) {string.Join(' ', block)})";
    }

    string Stmt.Visitor<string>.VisitIf(Stmt.If stmt) {
        return
            $"(if {print(stmt.condition)} {print(stmt.thenBranch)}"
            + (stmt.elseBranch is null ? "" : (" " + print(stmt.elseBranch)))
            + ")";
    }

    string Expr.Visitor<string>.VisitLiteral(Expr.Literal expr) {
        if (expr.value is string str_value) {
            return $"\"{str_value}\"";
        }
        else {
            return expr.value?.ToString() ?? "null";
        }
    }

    string Expr.Visitor<string>.VisitLogical(Expr.Logical expr) {
        return $"({expr.@operator.lexeme} {print(expr.left)} {print(expr.right)})";
    }

    string Stmt.Visitor<string>.VisitReturn(Stmt.Return stmt) {
        return stmt.value is null ? "(return)" : $"(return {print(stmt.value)})";
    }

    string Expr.Visitor<string>.VisitUnary(Expr.Unary expr) {
        return $"({expr.@operator.lexeme} {print(expr.right)})";
    }

    string Expr.Visitor<string>.VisitVariable(Expr.Variable expr) {
        return expr.name.lexeme;
    }

    // string Stmt.Visitor<string>.VisitWhile(Stmt.While stmt) {
    //     return $"(while ({print(stmt.condition)}) {print(stmt.branch)})";
    // }

    string Stmt.Visitor<string>.VisitFor(Stmt.For stmt) {
        string initializer = stmt.initializer is null ? "" : print(stmt.initializer);
        string condition = stmt.condition is null ? "" : print(stmt.condition);
        string increment = stmt.increment is null ? "" : print(stmt.increment);

        return $"(for ({initializer}) ({condition}) {print(stmt.body)}) ({increment})";
    }

    string Stmt.Visitor<string>.VisitPrint(Stmt.Print stmt) {
        List<string> items = new List<string>();
        foreach (Expr iexpr in stmt.items) {
            items.Add(print(iexpr));
        }
        return $"(print {string.Join(' ', items)})";
    }

    string Stmt.Visitor<string>.VisitBreak(Stmt.Break stmt) {
        return "(break!)";
    }

    string Stmt.Visitor<string>.VisitContinue(Stmt.Continue stmt) {
        return "(continue!)";
    }

    string Stmt.Visitor<string>.VisitVoid(Stmt.Void stmt) {
        return "(void)";
    }
}