using Kula.ASTCompiler.Lexer;
using Kula.ASTCompiler.Parser;

namespace Kula.BytecodeCompiler.Compiler;

class Compiler : Stmt.IVisitor<int>, Expr.IVisitor<int>
{
    public static readonly Compiler Instance = new();

    private readonly Dictionary<string, int> symbols = new();
    private readonly List<object?> literals = new();
    private readonly (Dictionary<double, int>, Dictionary<string, int>) literalMap = new(new(), new());
    private readonly List<Instruction> instructions = new();
    private readonly List<(List<ushort>, List<Instruction>)> functions = new();
    private readonly Stack<List<Instruction>> instructionsStack = new();
    private readonly Stack<(List<int>, List<int>)> forStack = new();

    private Compiler() { }

    private void Clear()
    {
        symbols.Clear();
        literals.Clear();
        instructions.Clear();
        functions.Clear();
        instructionsStack.Clear();
        forStack.Clear();
        literalMap.Item1.Clear();
        literalMap.Item2.Clear();

        instructionsStack.Push(instructions);
        literals.Add(false);
        literals.Add(true);
        literals.Add(null);
    }

    public CompiledFile Compile(List<Stmt> stmts)
    {
        Clear();
        foreach (Stmt stmt in stmts) {
            stmt.Accept(this);
        }

        return new CompiledFile(new(symbols), new(literals), new(instructions), new(functions));
    }

    // Utils
    private int New(OpCode opCode, int constant)
    {
        instructionsStack.Peek().Add(new(opCode, constant));
        return instructionsStack.Peek().Count - 1;
    }

    private int Pos
    {
        get => instructionsStack.Peek().Count;
    }

    private ushort SaveSymbol(string lexeme)
    {
        if (!symbols.ContainsKey(lexeme)) {
            int id = symbols.Count;
            symbols.Add(lexeme, id);
            return (ushort)id;
        }
        return (ushort)symbols[lexeme];
    }

    private int SaveLiteral(object? literal)
    {
        if (literal is null) {
            return 2;
        }
        else if (literal is bool lbool) {
            return lbool ? 1 : 0;
        }
        else if (literal is double ldouble) {
            if (literalMap.Item1.ContainsKey(ldouble)) {
                return literalMap.Item1[ldouble];
            }
            int id = literals.Count;
            literalMap.Item1[ldouble] = id;
            literals.Add(literal);
            return id;
        }
        else if (literal is string lstring) {
            if (literalMap.Item2.ContainsKey(lstring)) {
                return literalMap.Item2[lstring];
            }
            int id = literals.Count;
            literalMap.Item2[lstring] = id;
            literals.Add(literal);
            return id;
        }
        else {
            int id = literals.Count;
            literals.Add(literal);
            return id;
        }
    }

    int Expr.IVisitor<int>.VisitAssign(Expr.Assign expr)
    {
        Expr left = expr.left;
        Expr right = expr.right;
        if (left is Expr.Variable variable) {
            int variable_id = SaveSymbol(variable.name.lexeme);
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
        New(OpCode.ENVST, 0);
        foreach (Stmt stmt1 in stmt.statements) {
            stmt1.Accept(this);
        }
        New(OpCode.ENVED, 0);
        return 0;
    }

    int Stmt.IVisitor<int>.VisitBreak(Stmt.Break stmt)
    {
        int b = New(OpCode.JMP, 0);
        forStack.Peek().Item1.Add(b);
        return 0;
    }

    int Expr.IVisitor<int>.VisitCall(Expr.Call expr)
    {
        // expr.callee.Accept(this);
        if (expr.callee is Expr.Get expr_get) {
            expr_get.container.Accept(this);
            expr_get.key.Accept(this);
            New(OpCode.GETWT, 0);
        }
        else {
            expr.callee.Accept(this);
        }
        foreach (var arg in expr.arguments) {
            arg.Accept(this);
        }
        if (expr.callee is Expr.Get) {
            New(OpCode.CALWT, expr.arguments.Count);
        }
        else {
            New(OpCode.CALL, expr.arguments.Count);
        }
        return 0;
    }

    int Stmt.IVisitor<int>.VisitContinue(Stmt.Continue stmt)
    {
        int c = New(OpCode.JMP, 0);
        forStack.Peek().Item2.Add(c);
        return 0;
    }

    int Stmt.IVisitor<int>.VisitExpression(Stmt.Expression stmt)
    {
        stmt.expression.Accept(this);
        New(OpCode.POP, 0);
        return 0;
    }

    int Stmt.IVisitor<int>.VisitFor(Stmt.For stmt)
    {
        New(OpCode.ENVST, 0);
        stmt.initializer?.Accept(this);
        int for_condition = Pos;
        if (stmt.condition is null) {
            New(OpCode.LOADC, SaveLiteral(true));
        }
        else {
            stmt.condition?.Accept(this);
        }
        int if_not_jump = New(OpCode.JMPF, 0);
        forStack.Push((new(), new()));
        stmt.body.Accept(this);
        if (stmt.increment is not null) {
            stmt.increment.Accept(this);
            New(OpCode.POP, 0);
        }
        New(OpCode.JMP, for_condition);
        int end_loop = Pos;
        instructionsStack.Peek()[if_not_jump] = new(OpCode.JMPF, end_loop);
        (List<int> list_break, List<int> list_continue) = forStack.Pop();
        foreach (int pos in list_break) {
            // ins.Constant = end_loop;
            instructionsStack.Peek()[pos] = new(OpCode.JMP, end_loop);
        }
        foreach (int pos in list_continue) {
            // ins.Constant = for_condition;
            instructionsStack.Peek()[pos] = new(OpCode.JMP, for_condition);
        }
        New(OpCode.ENVED, 0);
        return 0;
    }

    int Expr.IVisitor<int>.VisitFunction(Expr.Function expr)
    {
        int func_ins = New(OpCode.FUNC, -1);
        List<Instruction> instructions = new();
        List<ushort> parameters = new();
        foreach (var parameter in expr.parameters) {
            parameters.Add(SaveSymbol(parameter.lexeme));
        }
        instructionsStack.Push(instructions);
        foreach (var stmt in expr.body) {
            stmt.Accept(this);
        }
        // func_ins.Constant = functions.Count;
        functions.Add((parameters, instructionsStack.Pop()));
        instructionsStack.Peek()[func_ins] = new(OpCode.FUNC, functions.Count - 1);
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
        int if_not_jump = New(OpCode.JMPF, 0);
        int if_branch = Pos;
        stmt.thenBranch.Accept(this);
        int if_branch_end = New(OpCode.JMP, 0);
        int else_branch = Pos;
        stmt.elseBranch?.Accept(this);
        int end = Pos;
        // if_not_jump.Constant = else_branch;
        instructionsStack.Peek()[if_not_jump] = new(OpCode.JMPF, else_branch);
        // if_branch_end.Constant = end;
        instructionsStack.Peek()[if_branch_end] = new(OpCode.JMP, end);
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
            New(OpCode.DUP, 0);
            int instr1 = New(OpCode.JMPF, 0);
            expr.right.Accept(this);
            int instr2 = New(OpCode.JMP, 0);
            // instr1.Constant = Pos;
            // instr2.Constant = Pos;
            instructionsStack.Peek()[instr1] = new(OpCode.JMPF, Pos);
            instructionsStack.Peek()[instr2] = new(OpCode.JMP, Pos);
        }
        else if (expr.@operator.type == TokenType.OR) {
            expr.left.Accept(this);
            New(OpCode.DUP, 0);
            int instr1 = New(OpCode.JMPT, 0);
            expr.right.Accept(this);
            int instr2 = New(OpCode.JMP, 0);
            // instr1.Constant = Pos;
            // instr2.Constant = Pos;
            instructionsStack.Peek()[instr1] = new(OpCode.JMPT, Pos);
            instructionsStack.Peek()[instr2] = new(OpCode.JMP, Pos);
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
            New(OpCode.RETV, 0);
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
        New(OpCode.LOAD, SaveSymbol(expr.name.lexeme));
        return 0;
    }
}
