using Kula.Core.Ast;

namespace Kula.Core.Compiler;

class Compiler : Stmt.IVisitor<int>, Expr.IVisitor<int>
{
    public static readonly Compiler Instance = new();

    private readonly Dictionary<string, int> variableDict = new();
    private readonly List<object?> literalList = new();
    private readonly List<Instruction> instructions = new();
    private readonly List<(List<int>, List<Instruction>)> functions = new();
    private readonly Stack<List<Instruction>> instructionsStack = new();
    private readonly Stack<(List<Instruction>, List<Instruction>)> forStack = new();

    private Compiler() { }

    private void Clear()
    {
        variableDict.Clear();
        literalList.Clear();
        instructions.Clear();
        functions.Clear();
        instructionsStack.Clear();
        forStack.Clear();

        instructionsStack.Push(instructions);
        literalList.Add(false);
        literalList.Add(true);
    }

    public CompiledFile Compile(List<Stmt> stmts)
    {
        Clear();
        foreach (Stmt stmt in stmts) {
            stmt.Accept(this);
        }

        return new CompiledFile(new(variableDict), new(literalList), new(instructions), new(functions));
    }

    // Utils
    private Instruction New(OpCode opCode, int constant)
    {
        Instruction instr = new(opCode, constant);
        instructionsStack.Peek().Add(instr);
        return instr;
    }

    private int Pos
    {
        get => instructionsStack.Peek().Count;
    }

    private int SaveVariable(string lexeme)
    {
        if (!variableDict.ContainsKey(lexeme)) {
            int id = variableDict.Count;
            variableDict.Add(lexeme, id);
            return id;
        }
        return variableDict[lexeme];
    }

    private int SaveLiteral(object? literal)
    {
        if (literal is bool literal_bool) {
            return literal_bool ? 1 : 0;
        }
        int id = literalList.Count;
        literalList.Add(literal);
        return id;
    }

    int Expr.IVisitor<int>.VisitAssign(Expr.Assign expr)
    {
        Expr left = expr.left;
        Expr right = expr.right;
        if (left is Expr.Variable variable) {
            int variable_id = SaveVariable(variable.name.lexeme);
            switch (expr.@operator.type) {
                case TokenType.COLON_EQUAL:
                    expr.right.Accept(this);
                    New(OpCode.DECL, variable_id);
                    break;
                case TokenType.EQUAL:
                    expr.right.Accept(this);
                    New(OpCode.ASGN, variable_id);
                    break;
            }
        }
        else if (left is Expr.Get left_get) {
            left_get.container.Accept(this);
            left_get.key.Accept(this);
            right.Accept(this);
            New(OpCode.SET, 0);
        }
        return 0;
    }

    int Expr.IVisitor<int>.VisitBinary(Expr.Binary expr)
    {
        expr.left.Accept(this);
        expr.right.Accept(this);
        OpCode opCode = expr.@operator.type switch {
            TokenType.PLUS => OpCode.ADD,
            TokenType.MINUS => OpCode.SUB,
            TokenType.STAR => OpCode.MUL,
            TokenType.SLASH => OpCode.DIV,
            TokenType.MODULUS => OpCode.MOD,
            TokenType.BANG_EQUAL => OpCode.NEQ,
            TokenType.EQUAL_EQUAL => OpCode.EQ,
            TokenType.GREATER => OpCode.GT,
            TokenType.LESS => OpCode.LT,
            TokenType.GREATER_EQUAL => OpCode.GE,
            TokenType.LESS_EQUAL => OpCode.LE,
            _ => throw new CompileError(),
        };
        New(opCode, 0);
        return 0;
    }

    int Stmt.IVisitor<int>.VisitBlock(Stmt.Block stmt)
    {
        New(OpCode.BLKST, 0);
        foreach (Stmt stmt1 in stmt.statements) {
            stmt1.Accept(this);
        }
        New(OpCode.BLKEND, 0);
        return 0;
    }

    int Stmt.IVisitor<int>.VisitBreak(Stmt.Break stmt)
    {
        Instruction b = New(OpCode.JMP, 0);
        forStack.Peek().Item1.Add(b);
        return 0;
    }

    int Expr.IVisitor<int>.VisitCall(Expr.Call expr)
    {
        expr.callee.Accept(this);
        foreach (var arg in expr.arguments) {
            arg.Accept(this);
        }
        New(OpCode.CALL, expr.arguments.Count);
        return 0;
    }

    int Stmt.IVisitor<int>.VisitContinue(Stmt.Continue stmt)
    {
        Instruction c = New(OpCode.JMP, 0);
        forStack.Peek().Item2.Add(c);
        return 0;
    }

    int Stmt.IVisitor<int>.VisitExpression(Stmt.Expression stmt)
    {
        stmt.expression.Accept(this);
        return 0;
    }

    int Stmt.IVisitor<int>.VisitFor(Stmt.For stmt)
    {
        New(OpCode.BLKST, 0);
        stmt.initializer?.Accept(this);
        int for_condition = Pos;
        if (stmt.condition is null) {
            New(OpCode.LOADC, SaveLiteral(true));
        }
        else {
            stmt.condition?.Accept(this);
        }
        Instruction if_not_jump = New(OpCode.JMPF, 0);
        forStack.Push((new(), new()));
        stmt.body.Accept(this);
        stmt.increment?.Accept(this);
        New(OpCode.JMP, for_condition);
        int end_loop = Pos;
        if_not_jump.Constant = end_loop;
        (List<Instruction> list_break, List<Instruction> list_continue) = forStack.Pop();
        foreach (Instruction ins in list_break) {
            ins.Constant = end_loop;
        }
        foreach (Instruction ins in list_continue) {
            ins.Constant = for_condition;
        }
        New(OpCode.BLKEND, 0);
        return 0;
    }

    int Expr.IVisitor<int>.VisitFunction(Expr.Function expr)
    {
        var func_ins = New(OpCode.FUNC, -1);
        List<Instruction> instructions = new();
        List<int> parameters = new();
        foreach (var parameter in expr.parameters) {
            parameters.Add(SaveVariable(parameter.lexeme));
        }
        instructionsStack.Push(instructions);
        foreach (var stmt in expr.body) {
            stmt.Accept(this);
        }
        func_ins.Constant = functions.Count;
        functions.Add((parameters, instructionsStack.Pop()));
        return 0;
    }

    int Expr.IVisitor<int>.VisitGet(Expr.Get expr)
    {
        expr.container.Accept(this);
        expr.key.Accept(this);
        New(OpCode.GET, 0);
        return 0;
    }

    int Stmt.IVisitor<int>.VisitIf(Stmt.If stmt)
    {
        stmt.condition.Accept(this);
        Instruction if_not_jump = New(OpCode.JMPF, 0);
        int if_branch = Pos;
        stmt.thenBranch.Accept(this);
        Instruction if_branch_end = New(OpCode.JMP, 0);
        int else_branch = Pos;
        stmt.elseBranch?.Accept(this);
        int end = Pos;
        if_not_jump.Constant = else_branch;
        if_branch_end.Constant = end;
        return 0;
    }

    int Stmt.IVisitor<int>.VisitImport(Stmt.Import stmt)
    {
        return 0;
    }

    int Expr.IVisitor<int>.VisitLiteral(Expr.Literal expr)
    {
        int id = SaveLiteral(expr.value);
        New(OpCode.LOADC, id);
        return 0;
    }

    int Expr.IVisitor<int>.VisitLogical(Expr.Logical expr)
    {
        if (expr.@operator.type == TokenType.AND) {
            expr.left.Accept(this);
            Instruction instr1 = New(OpCode.JMPF, 0);
            expr.right.Accept(this);
            Instruction instr2 = New(OpCode.JMP, 0);
            instr1.Constant = Pos;
            New(OpCode.LOADC, 0);
            instr2.Constant = Pos;
        }
        else if (expr.@operator.type == TokenType.OR) {
            expr.left.Accept(this);
            Instruction instr1 = New(OpCode.JMPT, 0);
            expr.right.Accept(this);
            Instruction instr2 = New(OpCode.JMP, 0);
            instr1.Constant = Pos;
            New(OpCode.LOADC, 1);
            instr2.Constant = Pos;
        }
        return 0;
    }

    int Stmt.IVisitor<int>.VisitPrint(Stmt.Print stmt)
    {
        foreach (Expr expr in stmt.items) {
            expr.Accept(this);
        }
        New(OpCode.PRINT, stmt.items.Count);
        return 0;
    }

    int Stmt.IVisitor<int>.VisitReturn(Stmt.Return stmt)
    {
        if (stmt.value is null) {
            New(OpCode.RET, 0);
        }
        else {
            stmt.value.Accept(this);
            New(OpCode.RET, 1);
        }
        return 0;
    }

    int Expr.IVisitor<int>.VisitUnary(Expr.Unary expr)
    {
        expr.right.Accept(this);
        OpCode opCode = expr.@operator.type switch {
            TokenType.MINUS => OpCode.NEG,
            TokenType.BANG => OpCode.NOT,
            _ => throw new CompileError(),
        };
        New(opCode, 0);
        return 0;
    }

    int Expr.IVisitor<int>.VisitVariable(Expr.Variable expr)
    {
        New(OpCode.LOAD, SaveVariable(expr.name.lexeme));
        return 0;
    }
}
