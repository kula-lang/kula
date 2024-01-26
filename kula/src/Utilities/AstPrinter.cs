using Kula.ASTCompiler.Lexer;
using Kula.ASTCompiler.Parser;

namespace Kula.Utilities;

class AstPrinter : Stmt.IVisitor<string>, Expr.IVisitor<string>
{
    public string Print(Stmt stmt)
    {
        return PStmt(stmt);
    }

    string PExpr(Expr expr)
    {
        return expr.Accept(this);
    }

    string PStmt(Stmt stmt)
    {
        return stmt.Accept(this);
    }

    string Expr.IVisitor<string>.VisitGet(Expr.Get expr)
    {
        return $"(get {PExpr(expr.container)} {PExpr(expr.key)})";
    }

    string Expr.IVisitor<string>.VisitAssign(Expr.Assign expr)
    {
        return $"(assign {expr.@operator.lexeme} {PExpr(expr.left)} {PExpr(expr.right)})";
    }

    string Expr.IVisitor<string>.VisitBinary(Expr.Binary expr)
    {
        return $"({expr.@operator.lexeme} {PExpr(expr.left)} {PExpr(expr.right)})";
    }

    string Stmt.IVisitor<string>.VisitBlock(Stmt.Block stmt)
    {
        List<string> items = new();
        foreach (Stmt istmt in stmt.statements) {
            items.Add(PStmt(istmt));
        }
        return $"(block {string.Join(' ', items)})";
    }

    string Expr.IVisitor<string>.VisitCall(Expr.Call expr)
    {
        List<string> items = new();
        foreach (Expr iexpr in expr.arguments) {
            items.Add(iexpr.Accept(this));
        }
        return $"({PExpr(expr.callee)} {string.Join(' ', items)})";
    }

    string Stmt.IVisitor<string>.VisitExpression(Stmt.Expression stmt)
    {
        return stmt.expression.Accept(this);
    }

    string Expr.IVisitor<string>.VisitFunction(Expr.Function expr)
    {
        List<string> parameters = new();
        foreach (Token token in expr.parameters) {
            parameters.Add(token.lexeme);
        }

        List<string> block = new();
        foreach (Stmt statement in expr.body) {
            block.Add(PStmt(statement));
        }
        return $"(lambda ({string.Join(' ', parameters)}) {string.Join(' ', block)})";
    }

    string Stmt.IVisitor<string>.VisitIf(Stmt.If stmt)
    {
        return
            $"(if {PExpr(stmt.condition)} {PStmt(stmt.thenBranch)}"
            + (stmt.elseBranch is null ? "" : (" " + PStmt(stmt.elseBranch)))
            + ")";
    }

    string Expr.IVisitor<string>.VisitLiteral(Expr.Literal expr)
    {
        if (expr.value is string str_value) {
            return $"\"{str_value}\"";
        }
        else {
            return expr.value?.ToString() ?? "null";
        }
    }

    string Expr.IVisitor<string>.VisitLogical(Expr.Logical expr)
    {
        return $"({expr.@operator.lexeme} {PExpr(expr.left)} {PExpr(expr.right)})";
    }

    string Stmt.IVisitor<string>.VisitReturn(Stmt.Return stmt)
    {
        return stmt.value is null ? "(return)" : $"(return {PExpr(stmt.value)})";
    }

    string Expr.IVisitor<string>.VisitUnary(Expr.Unary expr)
    {
        return $"({expr.@operator.lexeme} {PExpr(expr.right)})";
    }

    string Expr.IVisitor<string>.VisitVariable(Expr.Variable expr)
    {
        return expr.name.lexeme;
    }

    // string Stmt.Visitor<string>.VisitWhile(Stmt.While stmt) {
    //     return $"(while ({print(stmt.condition)}) {print(stmt.branch)})";
    // }

    string Stmt.IVisitor<string>.VisitFor(Stmt.For stmt)
    {
        string initializer = stmt.initializer is null ? "" : PStmt(stmt.initializer);
        string condition = stmt.condition is null ? "" : PExpr(stmt.condition);
        string increment = stmt.increment is null ? "" : PExpr(stmt.increment);

        return $"(for ({initializer}) ({condition}) {PStmt(stmt.body)}) ({increment})";
    }

    string Stmt.IVisitor<string>.VisitPrint(Stmt.Print stmt)
    {
        List<string> items = new();
        foreach (Expr iexpr in stmt.items) {
            items.Add(PExpr(iexpr));
        }
        return $"(print {string.Join(' ', items)})";
    }

    string Stmt.IVisitor<string>.VisitBreak(Stmt.Break stmt)
    {
        return "(break!)";
    }

    string Stmt.IVisitor<string>.VisitContinue(Stmt.Continue stmt)
    {
        return "(continue!)";
    }

    string Stmt.IVisitor<string>.VisitImport(Stmt.Import stmt)
    {
        List<string> items = new();
        foreach (Token tk in stmt.modules) {
            items.Add(tk.lexeme);
        }
        return $"(import {string.Join(' ', items)})";
    }
}