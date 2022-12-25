using Kula.Core.Ast;

namespace Kula.Core;

class Parser {
    private KulaEngine? kula;
    private List<Token>? tokens;

    private int current;

    public static Parser Instance = new Parser(); 

    private Stmt Declaration() {
        // if (Match(TokenType.FUNC)) {
        // }
        return Statement();
    }

    private Stmt Statement() {
        if (Match(TokenType.IF)) {
            return IfStatement();
        }
        else if (Match(TokenType.WHILE)) {
            return WhileStatement();
        }
        else if (Match(TokenType.LEFT_BRACE)) {
            return new Stmt.Block(Block());
        }
        else if (Match(TokenType.RETURN)) {
            return ReturnStatement();
        }
        return ExpressionStatement();
    }

    private Stmt IfStatement() {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after if.");
        Expr condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");

        Stmt then_branch = Statement();
        Stmt? else_branch = null;
        if (Match(TokenType.ELSE)) {
            else_branch = Statement();
        }

        return new Stmt.If(condition, then_branch, else_branch);
    }

    private Stmt WhileStatement() {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after while.");
        Expr condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect '(' after while.");

        Stmt branch = Statement();
        return new Stmt.While(condition, branch);
    }

    private Stmt ReturnStatement() {
        Expr? value = null;
        if (!Check(TokenType.SEMICOLON)) {
            value = Expression();
        }
        Consume(TokenType.SEMICOLON, "Expect ';' after return.");
        return new Stmt.Return(value);
    }

    private Stmt ExpressionStatement() {
        Expr expr = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
        
        return new Stmt.Expression(expr);
    }

    private Expr Expression() {
        return Assignment();
    }

    private Expr Assignment() {
        Expr expr = LogicOr();

        if (Match(TokenType.EQUAL, TokenType.COLON_EQUAL)) {
            Token @operator = Previous();
            Expr right = Assignment();

            return new Expr.Assign(expr, @operator, right);

            throw Error(@operator, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr LogicOr() {
        Expr expr = LogicAnd();

        while (Match(TokenType.OR)) {
            Token @operator = Previous();
            Expr right = LogicAnd();
            expr = new Expr.Logical(@operator, expr, right);
        }

        return expr;
    }

    private Expr LogicAnd() {
        Expr expr = Equality();

        while (Match(TokenType.AND)) {
            Token @operator = Previous();
            Expr right = Equality();
            expr = new Expr.Logical(@operator, expr, right);
        }

        return expr;
    }

    private Expr Equality() {
        Expr expr = Comparison();

        while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL)) {
            Token @operator = Previous();
            Expr right = Comparison();
            expr = new Expr.Binary(@operator, expr, right);
        }

        return expr;
    }

    private Expr Comparison() {
        Expr expr = Term();

        while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL)) {
            Token @operator = Previous();
            Expr right = Term();
            expr = new Expr.Binary(@operator, expr, right);
        }

        return expr;
    }

    private Expr Term() {
        Expr expr = Factor();

        while (Match(TokenType.PLUS, TokenType.MINUS)) {
            Token @operator = Previous();
            Expr right = Factor();
            expr = new Expr.Binary(@operator, expr, right);
        }

        return expr;
    }

    private Expr Factor() {
        Expr expr = Unary();

        while (Match(TokenType.STAR, TokenType.SLASH)) {
            Token @operator = Previous();
            Expr right = Unary();
            expr = new Expr.Binary(@operator, expr, right);
        }

        return expr;
    }

    private Expr Unary() {
        if (Match(TokenType.BANG, TokenType.MINUS)) {
            Token @operator = Previous();
            Expr right = Unary();
            return new Expr.Unary(@operator, right);
        }
        else {
            return Call();
        }
    }

    private Expr Call() {
        Expr expr = Primary();

        for (; ; ) {
            if (Match(TokenType.LEFT_PAREN)) {
                expr = FinishCall(expr);
            }
            // else if (Match(TokenType.DOT)) {
            //     expr = Dot(expr);
            // }
            // else if (Match(TokenType.LEFT_SQUARE)) {
            //     expr = Get();
            // }
            else {
                break;
            }
        }
        return expr;
    }

    private Expr FinishCall(Expr callee) {
        List<Expr> arguments = new List<Expr>();

        if (!Check(TokenType.RIGHT_PAREN)) {
            do {
                arguments.Add(Expression());
            }
            while (Match(TokenType.COMMA));
        }
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

        return new Expr.Call(callee, arguments);
    }

    private Expr Primary() {
        if (Match(TokenType.FALSE)) {
            return new Expr.Literal(false);
        }
        else if (Match(TokenType.TRUE)) {
            return new Expr.Literal(true);
        }
        else if (Match(TokenType.NULL)) {
            return new Expr.Literal(null);
        }
        else if (Match(TokenType.NUMBER, TokenType.STRING)) {
            return new Expr.Literal(Previous().literial);
        }
        else if (Match(TokenType.IDENTIFIER)) {
            return new Expr.Variable(Previous());
        }
        else if (Match(TokenType.LEFT_PAREN)) {
            Expr expr = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return expr;
        }
        // Function Declaration
        else if (Match(TokenType.FUNC)) {
            return FunctionDeclaration();
        }

        throw Error(Peek(), "Expect expression.");
    }

    private Expr FunctionDeclaration() {
        Consume(TokenType.LEFT_PAREN, "Expect ')' before function parameters.");

        List<(Token, Token)> parameters = new List<(Token, Token)>();
        if (!Check(TokenType.RIGHT_PAREN)) {
            do {
                Token param_name = Consume(TokenType.IDENTIFIER, "Expect parameters name.");
                Consume(TokenType.COLON, "Expect ':' between parameters name and type.");
                Token param_type = Consume(TokenType.IDENTIFIER, "Expect parameters type.");

                parameters.Add((param_name, param_type));
            }
            while (Match(TokenType.COMMA));
        }
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");

        Consume(TokenType.LEFT_BRACE, "Expect '{' before function body.");
        List<Stmt> body = Block();

        return new Expr.Function(parameters, body);
    }

    private List<Stmt> Block() {
        List<Stmt> statements = new List<Stmt>();

        while (!Check(TokenType.RIGHT_BRACE) && !IsEnd()) {
            statements.Add(Declaration());
        }

        Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    public List<Stmt> Parse(KulaEngine kula, List<Token> tokens) {
        this.kula = kula;
        this.tokens = tokens;

        List<Stmt> statements = new List<Stmt>();
        while (!IsEnd()) {
            try {
                statements.Add(Declaration());
            }
            catch (ParseError) {
                Synchronize();
            }
        }

        return statements;
    }

    private void Synchronize() {
        Advance();

        while (!IsEnd()) {
            if (Previous().type == TokenType.SEMICOLON) return;
            switch (Peek().type) {
                case TokenType.CLASS:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.RETURN:
                case TokenType.PRINT:
                    break;
            }

            Advance();
        }
    }

    private Token Consume(TokenType type, string errmsg) {
        if (Check(type)) {
            return Advance();
        }
        throw Error(Peek(), errmsg);
    }

    private ParseError Error(Token token, string errmsg) {
        kula!.Error(token, errmsg);
        return ParseError.Instance;
    }

    private Token Previous() {
        return tokens![current - 1];
    }

    private Token Peek() {
        return tokens![current];
    }

    private bool IsEnd() {
        return Peek().type == TokenType.EOF;
    }

    private bool Check(TokenType type) {
        return !IsEnd() && Peek().type == type;
    }

    private Token Advance() {
        if (!IsEnd()) {
            ++current;
        }
        return Previous();
    }

    private bool Match(TokenType type) {
        if (Check(type)) {
            Advance();
            return true;
        }
        return false;
    }

    private bool Match(params TokenType[] types) {
        foreach (TokenType type in types) {
            if (Check(type)) {
                Advance();
                return true;
            }
        }
        return false;
    }

    public class ParseError : Exception {
        public static ParseError Instance = new ParseError();
    }
}