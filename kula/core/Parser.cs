using Kula.Core.Ast;

namespace Kula.Core;

class Parser {
    private KulaEngine? kula;
    private List<Token>? tokens;

    private int current;

    public static Parser Instance = new Parser();

    private Stmt Declaration() {
        if (Match(TokenType.FUNC)) {
            return FunctionDeclaration();
        }
        else if (Match(TokenType.TYPE)) {
            return TypeDefine();
        }
        return Statement();
    }

    private Stmt Statement() {
        if (Match(TokenType.IF)) {
            return IfStatement();
        }
        else if (Match(TokenType.WHILE)) {
            return WhileStatement();
        }
        else if (Match(TokenType.FOR)) {
            return ForStatement();
        }
        else if (Match(TokenType.LEFT_BRACE)) {
            return new Stmt.Block(Block());
        }
        else if (Match(TokenType.RETURN)) {
            return ReturnStatement();
        }
        else if (Match(TokenType.PRINT)) {
            return PrintStatement();
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
        Consume(TokenType.LEFT_PAREN, "Expect '(' before while condition.");
        Expr condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect '(' after while condition.");

        Stmt branch = Statement();
        return new Stmt.While(condition, branch);
    }

    private Stmt ForStatement() {
        Consume(TokenType.LEFT_PAREN, "Expect '(' before for condition.");
        Stmt? initializer;
        if (Match(TokenType.SEMICOLON)) {
            initializer = null;
        }
        else {
            initializer = ExpressionStatement();
        }

        Expr? condition = null;
        if (!Check(TokenType.SEMICOLON)) {
            condition = Expression();
        }
        Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

        Expr? increment = null;
        if (!Check(TokenType.RIGHT_PAREN)) {
            increment = Expression();
        }
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after for condition.");

        Stmt body = Statement();

        if (increment != null) {
            body = new Stmt.Block(
                new List<Stmt>() {
                    body,
                    new Stmt.Expression(increment) });
        }

        Expr final_condition = condition ?? new Expr.Literal(true);

        Stmt loop = new Stmt.While(final_condition, body);
        if (initializer != null) {
            loop = new Stmt.Block(
                new List<Stmt>() { initializer, loop }
            );
        }

        return loop;
    }

    private Stmt ReturnStatement() {
        Expr? value = null;
        if (!Check(TokenType.SEMICOLON)) {
            value = Expression();
        }
        Consume(TokenType.SEMICOLON, "Expect ';' after return.");
        return new Stmt.Return(value);
    }

    private Stmt PrintStatement() {
        List<Expr> items = new List<Expr>();
        do {
            items.Add(Expression());
        }
        while (Match(TokenType.COMMA));
        Consume(TokenType.SEMICOLON, "Expect ';' after print.");

        return new Stmt.Print(items);
    }

    private Stmt ExpressionStatement() {
        Expr expr = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after expression.");

        return new Stmt.Expression(expr);
    }

    private Stmt FunctionDeclaration() {
        Token func_name = Consume(TokenType.IDENTIFIER, "Expect identifier in function declaration.");
        Expr function = LambdaDeclaration();

        return new Stmt.Expression(
            new Expr.Assign(
                new Expr.Variable(func_name),
                Token.MakeTemp(TokenType.COLON_EQUAL, ":="),
                function));
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
            else if (Match(TokenType.DOT)) {
                expr = DotGet(expr);
            }
            else if (Match(TokenType.LEFT_SQUARE)) {
                expr = SquareGet(expr);
            }
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

    private Expr DotGet(Expr dict) {
        Token key = Consume(TokenType.IDENTIFIER, "Expect identifier after dot.");
        return new Expr.Get(dict, new Expr.Literal(key.lexeme));
    }

    private Expr SquareGet(Expr dict) {
        Expr key = Expression();
        Consume(TokenType.RIGHT_SQUARE, "Expect ']' after dict key.");

        return new Expr.Get(dict, key);
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
        // Lambda Declaration
        else if (Match(TokenType.FUNC)) {
            return LambdaDeclaration();
        }

        throw Error(Peek(), "Expect expression.");
    }

    private Expr LambdaDeclaration() {
        Consume(TokenType.LEFT_PAREN, "Expect '(' before function parameters.");

        List<(Token, Ast.Type)> parameters = new List<(Token, Ast.Type)>();
        if (!Check(TokenType.RIGHT_PAREN)) {
            do {
                Token param_name = Consume(TokenType.IDENTIFIER, "Expect parameters name.");
                Ast.Type param_type;
                if (Match(TokenType.COLON)) {
                    param_type = Type();
                }
                else {
                    param_type = new Ast.Type.Literal(
                        Token.MakeTemp("Any"));
                }

                parameters.Add((param_name, param_type));
            }
            while (Match(TokenType.COMMA));
        }
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");

        Token return_type;
        if (Match(TokenType.ARROW)) {
            return_type = Consume(TokenType.IDENTIFIER, "Expect return type name.");
        }
        else {
            return_type = Token.MakeTemp("None");
        }

        Consume(TokenType.LEFT_BRACE, "Expect '{' before function body.");
        List<Stmt> body = Block();

        return new Expr.Function(parameters, return_type, body);
    }

    private List<Stmt> Block() {
        List<Stmt> statements = new List<Stmt>();

        while (!Check(TokenType.RIGHT_BRACE) && !IsEnd()) {
            statements.Add(Declaration());
        }

        Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    private Stmt TypeDefine() {
        Token name = Consume(TokenType.IDENTIFIER, "Expect type name.");
        Consume(TokenType.LEFT_BRACE, "Expect '{' before type defination.");

        List<(Token, Ast.Type)> interfaces = new List<(Token, Ast.Type)>();
        if (!Check(TokenType.RIGHT_BRACE)) {
            do {
                Token item_name = Consume(TokenType.IDENTIFIER, "Expect item name in type defination.");
                Consume(TokenType.COLON, "Expect ':' in type defination item.");
                Ast.Type item_type = Type();
            }
            while (Match(TokenType.COMMA));
        }

        Consume(TokenType.RIGHT_BRACE, "Expect '}' after type defination.");

        return new Stmt.TypeDefine(name, interfaces);
    }

    private Ast.Type Type() {
        if (Match(TokenType.LESS)) {
            return TypeExpression();
        }
        Token name = Consume(TokenType.IDENTIFIER, "Expect type name.");
        return new Ast.Type.Literal(name);
    }

    private Ast.Type TypeExpression() {
        List<Ast.Type> parameters = new List<Ast.Type>();
        if (!Check(TokenType.COLON)) {
            do {
                parameters.Add(Type());
            }
            while (Match(TokenType.COMMA));
        }

        Consume(TokenType.COLON, "Expect ':' before return type.");
        
        Ast.Type return_type = Type();
        Consume(TokenType.GREATER, "Expect '>' after type expression.");

        return new Ast.Type.TypeExpr(parameters, return_type);
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