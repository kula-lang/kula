using Kula.Core.Ast;

namespace Kula.Core;

class Resolver : Stmt.Visitor<int>, Expr.Visitor<int>
{
    class Environment
    {
        private readonly Environment enclosing;
        private readonly Dictionary<string, bool> variables;

        public Environment()
        {
            enclosing = this;
            variables = new Dictionary<string, bool>();
        }

        public Environment(Environment env)
        {
            this.enclosing = env;
            variables = new Dictionary<string, bool>();
        }

        public bool Has(string name)
        {
            if (variables.ContainsKey(name)) {
                return true;
            }
            if (enclosing != this) {
                return variables.ContainsKey(name);
            }
            return false;
        }

        public bool Use(string name)
        {
            if (Has(name)) {
                variables[name] = false;
                return true;
            }
            else {
                return false;
            }
        }

        public bool Declare(string name)
        {
            if (variables.ContainsKey(name)) {
                return false;
            }
            variables[name] = true;
            return true;
        }
    }

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

    private Resolver.Environment environment = new Resolver.Environment();
    private int inFunction, inFor;

    public void Resolve(KulaEngine kula, List<Stmt> stmts)
    {
        this.kula = kula;
        environment = new Environment();
        inFunction = 0;
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

    int Expr.Visitor<int>.VisitAssign(Expr.Assign expr)
    {
        if (expr.left is Expr.Variable left_expr) {
            if (expr.@operator.type == TokenType.COLON_EQUAL) {
                if (environment.Declare(left_expr.name.lexeme) == false) {
                    throw Error(left_expr.name, $"Illegal declaration '{left_expr.name.lexeme}'.");
                }
            }
            else if (expr.@operator.type == TokenType.EQUAL) {
                if (environment.Has(left_expr.name.lexeme) == false) {
                    // throw Error(left_expr.name, $"Use Variable '{left_expr.name.lexeme}' before declaration.");
                }
            }
        }
        else if (expr.left is Expr.Get left_get) {
            left_get.Accept(this);
        }
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
        Environment previous = this.environment;

        this.environment = new Environment(previous);
        foreach (Stmt si in stmt.statements) {
            si.Accept(this);
        }

        this.environment = previous;

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

        Environment previous = environment;
        environment = new Environment(previous);

        if (stmt.increment is not null) {
            stmt.increment.Accept(this);
        }
        if (stmt.condition is not null) {
            stmt.condition.Accept(this);
        }
        stmt.body.Accept(this);

        environment = previous;

        inFor--;
        return 0;
    }

    int Expr.Visitor<int>.VisitFunction(Expr.Function expr)
    {
        inFunction++;

        Environment previous = environment;
        environment = new Environment(previous);

        foreach (Token param in expr.parameters) {
            environment.Declare(param.lexeme);
        }
        foreach (Stmt stmt in expr.body) {
            stmt.Accept(this);
        }

        inFunction--;
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
        if (inFunction <= 0) {
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
        if (environment.Use(expr.name.lexeme) == false) {
            // throw Error(expr.name, $"Undefined variable '{expr.name.lexeme}'.");
        }
        return 0;
    }
}