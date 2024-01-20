using Kula.ASTCompiler.Lexer;

namespace Kula.ASTCompiler.Parser;

abstract class Expr
{
    public interface IVisitor<R>
    {
        R VisitCall(Call expr);
        R VisitGet(Get expr);
        R VisitLiteral(Literal expr);
        R VisitUnary(Unary expr);
        R VisitBinary(Binary expr);
        R VisitLogical(Logical expr);
        R VisitVariable(Variable expr);
        R VisitAssign(Assign expr);
        R VisitFunction(Function expr);
    }
    public abstract R Accept<R>(IVisitor<R> visitor);

    public class Call : Expr
    {
        public readonly Expr callee;
        public readonly List<Expr> arguments;
        public readonly Token paren;

        public Call(Expr callee, List<Expr> arguments, Token paren)
        {
            this.callee = callee;
            this.arguments = arguments;
            this.paren = paren;
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitCall(this);
        }
    }

    /// <summary>
    /// Get Value by Key From Container (Object|Array)
    /// </summary>
    public class Get : Expr
    {
        public readonly Token @operator;
        public readonly Expr container;
        public readonly Expr key;

        public Get(Expr container, Expr key, Token @operator)
        {
            this.container = container;
            this.key = key;
            this.@operator = @operator;
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitGet(this);
        }
    }

    public class Literal : Expr
    {
        public readonly object? value;

        public Literal(object? value)
        {
            this.value = value;
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitLiteral(this);
        }
    }

    public class Unary : Expr
    {
        public readonly Token @operator;
        public readonly Expr right;

        public Unary(Token @operator, Expr right)
        {
            this.@operator = @operator;
            this.right = right;
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitUnary(this);
        }
    }

    public class Binary : Expr
    {
        public readonly Token @operator;
        public readonly Expr left, right;

        public Binary(Token @operator, Expr left, Expr right)
        {
            this.@operator = @operator;
            this.left = left;
            this.right = right;
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitBinary(this);
        }
    }

    public class Logical : Expr
    {
        public readonly Token @operator;
        public readonly Expr left, right;

        public Logical(Token @operator, Expr left, Expr right)
        {
            this.@operator = @operator;
            this.left = left;
            this.right = right;
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitLogical(this);
        }
    }

    public class Variable : Expr
    {
        public readonly Token name;

        public Variable(Token name)
        {
            this.name = name;
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitVariable(this);
        }
    }

    public class Assign : Expr
    {
        public readonly Expr left;
        public readonly Token @operator;
        public readonly Expr right;

        public Assign(Expr left, Token @operator, Expr right)
        {
            this.left = left;
            this.@operator = @operator;
            this.right = right;
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitAssign(this);
        }
    }

    public class Function : Expr
    {
        public readonly List<Token> parameters;
        public readonly List<Stmt> body;

        public Function(List<Token> parameters, List<Stmt> body)
        {
            this.parameters = parameters;
            this.body = body;
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitFunction(this);
        }
    }
}