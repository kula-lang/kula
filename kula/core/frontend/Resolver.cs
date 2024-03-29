using Kula.Core.Ast;

namespace Kula.Core;

class Resolver : Stmt.Visitor<int>, Expr.Visitor<int>
{
    private ResolveError Error(Token token, string errmsg)
    {
        kula!.ResolveError(token, errmsg);
        return ResolveError.Instance;
    }

    public class ResolveError : Exception
    {
        public static readonly ResolveError Instance = new ResolveError();
    }

    private KulaEngine? kula;
    public static readonly Resolver Instance = new Resolver();

    private int inFor;
    private Stack<int> inFunction = new Stack<int>();

    public void Resolve(KulaEngine kula, List<Stmt> stmts)
    {
        lock (this) {
            this.kula = kula;
            inFunction.Clear();//inFunction = new Stack<int>();
            inFor = 0;

            foreach (Stmt stmt in stmts) {
                try {
                    stmt.Accept(this);
                }
                catch (ResolveError) {
                    continue;
                }
            }
        }
    }

    int Expr.Visitor<int>.VisitAssign(Expr.Assign expr)
    {
        if (expr.left is Expr.Variable left_variable) {
            if (expr.@operator.type == TokenType.COLON_EQUAL) {
            }
            else if (expr.@operator.type == TokenType.EQUAL) {
            }
        }
        else if (expr.left is Expr.Get left_get) {
            left_get.Accept(this);
        }
        else {
            throw Error(expr.@operator, "Illegal assignment.");
        }

        expr.right.Accept(this);
        return 0;
    }

    int Expr.Visitor<int>.VisitBinary(Expr.Binary expr)
    {
        expr.left.Accept(this);
        expr.right.Accept(this);
        return 0;
    }

    int Stmt.Visitor<int>.VisitBlock(Stmt.Block stmt)
    {
        foreach (Stmt si in stmt.statements) {
            si.Accept(this);
        }
        return 0;
    }

    int Stmt.Visitor<int>.VisitBreak(Stmt.Break stmt)
    {
        if (inFor <= 0) {
            throw Error(stmt.keyword, "Illegal 'break'.");
        }
        return 0;
    }

    int Expr.Visitor<int>.VisitCall(Expr.Call expr)
    {
        foreach (var ei in expr.arguments) {
            ei.Accept(this);
        }
        return 0;
    }

    int Stmt.Visitor<int>.VisitContinue(Stmt.Continue stmt)
    {
        if (inFor <= 0) {
            throw Error(stmt.keyword, "Illegal 'continue'.");
        }
        return 0;
    }

    int Stmt.Visitor<int>.VisitExpression(Stmt.Expression stmt)
    {
        stmt.expression.Accept(this);
        return 0;
    }

    int Stmt.Visitor<int>.VisitFor(Stmt.For stmt)
    {
        inFor++;

        if (stmt.increment is not null) {
            stmt.increment.Accept(this);
        }
        if (stmt.condition is not null) {
            stmt.condition.Accept(this);
        }
        stmt.body.Accept(this);

        inFor--;
        return 0;
    }

    int Expr.Visitor<int>.VisitFunction(Expr.Function expr)
    {
        inFunction.Push(inFor);
        inFor = 0;

        foreach (Token param in expr.parameters) {
        }
        foreach (Stmt stmt in expr.body) {
            stmt.Accept(this);
        }

        inFor = inFunction.Pop();
        return 0;
    }

    int Expr.Visitor<int>.VisitGet(Expr.Get expr)
    {
        expr.container.Accept(this);
        expr.key.Accept(this);
        return 0;
    }

    int Stmt.Visitor<int>.VisitIf(Stmt.If stmt)
    {
        stmt.condition.Accept(this);
        stmt.thenBranch.Accept(this);
        if (stmt.elseBranch is not null) {
            stmt.elseBranch.Accept(this);
        }
        return 0;
    }

    int Stmt.Visitor<int>.VisitImport(Stmt.Import stmt)
    {
        return 0;
    }

    int Expr.Visitor<int>.VisitLiteral(Expr.Literal expr)
    {
        return 0;
    }

    int Expr.Visitor<int>.VisitLogical(Expr.Logical expr)
    {
        expr.left.Accept(this);
        expr.right.Accept(this);
        return 0;
    }

    int Stmt.Visitor<int>.VisitPrint(Stmt.Print stmt)
    {
        foreach (Expr expr in stmt.items) {
            expr.Accept(this);
        }
        return 0;
    }

    int Stmt.Visitor<int>.VisitReturn(Stmt.Return stmt)
    {
        if (inFunction.Count <= 0) {
            throw Error(stmt.keyword, "Illegal 'return'.");
        }
        if (stmt.value is not null) {
            stmt.value.Accept(this);
        }
        return 0;
    }

    int Expr.Visitor<int>.VisitUnary(Expr.Unary expr)
    {
        expr.right.Accept(this);
        return 0;
    }

    int Expr.Visitor<int>.VisitVariable(Expr.Variable expr)
    {
        return 0;
    }
}