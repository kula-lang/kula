namespace Kula.Core.Ast;

abstract class Stmt
{
    public interface Visitor<R>
    {
        R VisitBreak(Break stmt);
        R VisitContinue(Continue stmt);
        R VisitExpression(Expression stmt);
        R VisitReturn(Return stmt);
        R VisitPrint(Print stmt);
        R VisitBlock(Block stmt);
        R VisitFor(For stmt);
        R VisitIf(If stmt);
    }
    public abstract R Accept<R>(Visitor<R> visitor);

    public class Break : Stmt
    {
        public Break() { }

        public override R Accept<R>(Visitor<R> visitor)
        {
            return visitor.VisitBreak(this);
        }
    }

    public class Continue : Stmt
    {
        public Continue() { }

        public override R Accept<R>(Visitor<R> visitor)
        {
            return visitor.VisitContinue(this);
        }
    }

    public class Expression : Stmt
    {
        public readonly Expr expression;

        public Expression(Expr expression)
        {
            this.expression = expression;
        }

        public override R Accept<R>(Visitor<R> visitor)
        {
            return visitor.VisitExpression(this);
        }
    }

    public class Print : Stmt
    {
        public readonly List<Expr> items;

        public Print(List<Expr> items)
        {
            this.items = items;
        }

        public override R Accept<R>(Visitor<R> visitor)
        {
            return visitor.VisitPrint(this);
        }
    }

    public class Return : Stmt
    {
        public readonly Expr? value;

        public Return(Expr? value)
        {
            this.value = value;
        }

        public override R Accept<R>(Visitor<R> visitor)
        {
            return visitor.VisitReturn(this);
        }
    }

    public class Block : Stmt
    {
        public readonly List<Stmt> statements;

        public Block(List<Stmt> statements)
        {
            this.statements = statements;
        }

        public override R Accept<R>(Visitor<R> visitor)
        {
            return visitor.VisitBlock(this);
        }
    }

    // public class While : Stmt {
    //     public readonly Expr condition;
    //     public readonly Stmt branch;

    //     public While(Expr condition, Stmt branch) {
    //         this.condition = condition;
    //         this.branch = branch;
    //     }

    //     public override R Accept<R>(Visitor<R> visitor) {
    //         return visitor.VisitWhile(this);
    //     }
    // }

    public class For : Stmt
    {
        public readonly Stmt? initializer;
        public readonly Expr? condition;
        public readonly Expr? increment;
        public readonly Stmt body;

        public For(Stmt? initializer, Expr? condition, Expr? increment, Stmt body)
        {
            this.initializer = initializer;
            this.condition = condition;
            this.increment = increment;
            this.body = body;
        }

        public override R Accept<R>(Visitor<R> visitor)
        {
            return visitor.VisitFor(this);
        }
    }

    public class If : Stmt
    {
        public readonly Expr condition;
        public readonly Stmt thenBranch;
        public readonly Stmt? elseBranch;

        public If(Expr condition, Stmt thenBranch, Stmt? elseBranch)
        {
            this.condition = condition;
            this.thenBranch = thenBranch;
            this.elseBranch = elseBranch;
        }

        public override R Accept<R>(Visitor<R> visitor)
        {
            return visitor.VisitIf(this);
        }
    }
}