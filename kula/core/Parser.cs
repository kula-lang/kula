using Kula.Core.Ast;

namespace Kula.Core;

class Parser
{
    private KulaEngine? kula;
    private List<Token>? tokens;

    private int current;

    public static Parser Instance = new Parser();

    private Stmt Declaration()
    {
        if (Match(TokenType.IMPORT)) {
            return Import();
        }
        else if (Match(TokenType.FUNC)) {
            return FunctionDeclaration();
        }
        else if (Match(TokenType.CLASS)) {
            return ClassDeclaration();
        }
        return Statement();
    }

    private Stmt.Sugar Import()
    {
        Token lbrace = Consume(TokenType.LEFT_BRACE, "Expect '{' before 'import' block.");
        List<Stmt> list = new List<Stmt>();
        if (!Check(TokenType.RIGHT_BRACE)) {
            do {
                Token file_path = Consume(TokenType.STRING, "Expect module name.");
                list.Add(
                    new Stmt.Expression(
                        new Expr.Call(
                            new Expr.Variable(Token.MakeTemp("load")),
                            new List<Expr>() { new Expr.Literal(file_path.literial) },
                            lbrace)));
            }
            while (Match(TokenType.COMMA));
        }
        Consume(TokenType.RIGHT_BRACE, "Expect '}' after 'import' block.");
        return new Stmt.Sugar(list);
    }

    private Stmt Statement()
    {
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
        else if (Match(TokenType.BREAK)) {
            Consume(TokenType.SEMICOLON, "Expect ';' after 'break'.");
            return new Stmt.Break();
        }
        else if (Match(TokenType.CONTINUE)) {
            Consume(TokenType.SEMICOLON, "Expect ';' after 'continue'.");
            return new Stmt.Continue();
        }
        else {
            return ExpressionStatement();
        }
    }

    private Stmt IfStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' before 'if' condition.");
        Expr condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after 'if' condition.");

        Stmt then_branch = Statement();
        Stmt? else_branch = null;
        if (Match(TokenType.ELSE)) {
            else_branch = Statement();
        }

        return new Stmt.If(condition, then_branch, else_branch);
    }

    private Stmt WhileStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' before 'while' condition.");
        Expr condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after 'while' condition.");

        Stmt branch = Statement();
        return new Stmt.For(null, condition, null, branch);
        //return new Stmt.While(condition, branch);
    }

    private Stmt ForStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' before 'for' condition.");
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
        Consume(TokenType.SEMICOLON, "Expect ';' after 'for' condition.");

        Expr? increment = null;
        if (!Check(TokenType.RIGHT_PAREN)) {
            increment = Expression();
        }
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after 'for' condition.");

        Stmt body = Statement();

        return new Stmt.For(initializer, condition, increment, body);
    }

    private Stmt ReturnStatement()
    {
        Expr? value = null;
        if (!Check(TokenType.SEMICOLON)) {
            value = Expression();
        }
        Consume(TokenType.SEMICOLON, "Expect ';' after return.");
        return new Stmt.Return(value);
    }

    private Stmt PrintStatement()
    {
        List<Expr> items = new List<Expr>();
        do {
            items.Add(Expression());
        }
        while (Match(TokenType.COMMA));
        Consume(TokenType.SEMICOLON, "Expect ';' after print.");

        return new Stmt.Print(items);
    }

    private Stmt ExpressionStatement()
    {
        Expr expr = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after expression.");

        return new Stmt.Expression(expr);
    }

    private Stmt FunctionDeclaration()
    {
        Token func_name = Consume(TokenType.IDENTIFIER, "Expect identifier in function declaration.");
        Expr function = LambdaDeclaration();

        return new Stmt.Expression(
            new Expr.Assign(
                new Expr.Variable(func_name),
                Token.MakeTemp(TokenType.COLON_EQUAL, ":="),
                function));
    }

    private Stmt ClassDeclaration()
    {
        Token class_name = Consume(TokenType.IDENTIFIER, "Expect identifier in class declaration.");

        Token? parent_name = null;
        if (Match(TokenType.COLON)) {
            parent_name = Consume(TokenType.IDENTIFIER, "Expect identifier in class extension.");
        }

        Token lbrace = Consume(TokenType.LEFT_BRACE, "Expect '{' before class declaration block.");

        List<Stmt> methods = new List<Stmt>();

        Token prototype = Token.MakeTemp("__proto__");
        Token func = Token.MakeTemp("__func__");
        Token colon_equal = Token.MakeTemp(TokenType.COLON_EQUAL, ":=");
        Token token_object = Token.MakeTemp("Object");
        Token token_this = Token.MakeTemp("this");

        methods.Add(
            new Stmt.Expression(
                new Expr.Assign(
                    new Expr.Variable(prototype),
                    colon_equal,
                    new Expr.Call(
                        new Expr.Variable(token_object),
                        new List<Expr>(),
                        Token.MakeTemp(TokenType.RIGHT_PAREN, ")")
                    )
                )
            )
        );
        while (!Match(TokenType.RIGHT_BRACE)) {
            Token method_name = Consume(TokenType.IDENTIFIER, "Expect identifier before method declaration.");
            Expr method_body = LambdaDeclaration();

            if (method_name.lexeme == "constructor") {
                method_name = Token.MakeTemp("__func__");
                Expr.Function constructor = (Expr.Function)method_body;
                constructor.body.Insert(0,
                    new Stmt.Expression(
                        new Expr.Assign(
                            new Expr.Variable(token_this),
                            colon_equal,
                            new Expr.Call(new Expr.Variable(token_object), new List<Expr>(), Token.MakeTemp(TokenType.RIGHT_PAREN, ")"))
                        )
                    )
                );
                constructor.body.Insert(1,
                    new Stmt.Expression(
                        new Expr.Assign(
                            new Expr.Get(
                                new Expr.Variable(token_this),
                                new Expr.Literal("__proto__"),
                                Token.MakeTemp(TokenType.DOT, ".")),
                            colon_equal,
                            new Expr.Variable(prototype)
                        )
                    )
                );

                if (parent_name is not null) {
                    constructor.body.Insert(2,
                    new Stmt.Expression(
                            new Expr.Assign(
                                new Expr.Get(
                                    new Expr.Variable(prototype),
                                    new Expr.Literal("__proto__"),
                                    Token.MakeTemp(TokenType.DOT, ".")),
                                colon_equal,
                                new Expr.Variable(parent_name.Value)
                            )
                        )
                    );
                }
                constructor.body.Add(new Stmt.Return(new Expr.Variable(token_this)));
            }

            methods.Add(
                new Stmt.Expression(
                    new Expr.Assign(
                        new Expr.Get(
                            new Expr.Variable(prototype),
                            new Expr.Literal(method_name.lexeme),
                            Token.MakeTemp(TokenType.DOT, ".")),
                        Token.MakeTemp(TokenType.EQUAL, "="),
                        method_body
                    )
                )
            );
        }
        methods.Add(new Stmt.Return(new Expr.Variable(prototype)));

        return new Stmt.Expression(
            new Expr.Assign(
                new Expr.Variable(class_name),
                Token.MakeTemp(TokenType.COLON_EQUAL, ":="),
                new Expr.Call(
                    new Expr.Function(new List<Token>(), methods),
                    new List<Expr>(),
                    lbrace
                )
            )
        );
    }

    private Expr Expression()
    {
        return Assignment();
    }

    private Expr Assignment()
    {
        Expr expr = LogicOr();

        if (Match(TokenType.EQUAL, TokenType.COLON_EQUAL)) {
            Token @operator = Previous();
            Expr right = Assignment();

            return new Expr.Assign(expr, @operator, right);

            throw Error(@operator, "Invalid assignment target.");
        }
        else if (Match(TokenType.PLUS_EQUAL, TokenType.MINUS_EQUAL, TokenType.STAR_EQUAL, TokenType.SLASH_EQUAL, TokenType.MODULUS_EQUAL)) {
            Token @operator = Previous();
            Expr right = Assignment();
            Token equal = Token.MakeTemp(TokenType.EQUAL, "=");

            switch (@operator.type) {
                case TokenType.PLUS_EQUAL:
                    return new Expr.Assign(expr, equal, new Expr.Binary(Token.MakeTemp(TokenType.PLUS, "+"), expr, right));
                case TokenType.MINUS_EQUAL:
                    return new Expr.Assign(expr, equal, new Expr.Binary(Token.MakeTemp(TokenType.MINUS, "-"), expr, right));
                case TokenType.STAR_EQUAL:
                    return new Expr.Assign(expr, equal, new Expr.Binary(Token.MakeTemp(TokenType.STAR, "*"), expr, right));
                case TokenType.SLASH_EQUAL:
                    return new Expr.Assign(expr, equal, new Expr.Binary(Token.MakeTemp(TokenType.SLASH, "/"), expr, right));
                case TokenType.MODULUS_EQUAL:
                    return new Expr.Assign(expr, equal, new Expr.Binary(Token.MakeTemp(TokenType.MODULUS, "%"), expr, right));
            }
            throw Error(@operator, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr LogicOr()
    {
        Expr expr = LogicAnd();

        while (Match(TokenType.OR)) {
            Token @operator = Previous();
            Expr right = LogicAnd();
            expr = new Expr.Logical(@operator, expr, right);
        }

        return expr;
    }

    private Expr LogicAnd()
    {
        Expr expr = Equality();

        while (Match(TokenType.AND)) {
            Token @operator = Previous();
            Expr right = Equality();
            expr = new Expr.Logical(@operator, expr, right);
        }

        return expr;
    }

    private Expr Equality()
    {
        Expr expr = Comparison();

        while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL)) {
            Token @operator = Previous();
            Expr right = Comparison();
            expr = new Expr.Binary(@operator, expr, right);
        }

        return expr;
    }

    private Expr Comparison()
    {
        Expr expr = Term();

        while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL)) {
            Token @operator = Previous();
            Expr right = Term();
            expr = new Expr.Binary(@operator, expr, right);
        }

        return expr;
    }

    private Expr Term()
    {
        Expr expr = Factor();

        while (Match(TokenType.PLUS, TokenType.MINUS)) {
            Token @operator = Previous();
            Expr right = Factor();
            expr = new Expr.Binary(@operator, expr, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        Expr expr = Unary();

        while (Match(TokenType.STAR, TokenType.SLASH, TokenType.MODULUS)) {
            Token @operator = Previous();
            Expr right = Unary();
            expr = new Expr.Binary(@operator, expr, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(TokenType.BANG, TokenType.MINUS)) {
            Token @operator = Previous();
            Expr right = Unary();
            return new Expr.Unary(@operator, right);
        }
        else {
            return Call();
        }
    }

    private Expr Call()
    {
        Expr expr = Primary();

        for (; ; ) {
            if (Match(TokenType.LEFT_PAREN)) {
                expr = FinishCall(expr);
            }
            else if (Match(TokenType.DOT)) {
                expr = DotGet(expr, Previous());
            }
            else if (Match(TokenType.LEFT_SQUARE)) {
                expr = SquareGet(expr, Previous());
            }
            else {
                break;
            }
        }
        return expr;
    }

    private Expr FinishCall(Expr callee)
    {
        List<Expr> arguments = new List<Expr>();

        if (!Check(TokenType.RIGHT_PAREN)) {
            do {
                arguments.Add(Expression());
            }
            while (Match(TokenType.COMMA));
        }
        Token paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

        return new Expr.Call(callee, arguments, paren);
    }

    private Expr DotGet(Expr dict, Token dot)
    {
        Token key = Consume(TokenType.IDENTIFIER, "Expect identifier after dot.");
        return new Expr.Get(dict, new Expr.Literal(key.lexeme), dot);
    }

    private Expr SquareGet(Expr dict, Token square)
    {
        Expr key = Expression();
        Consume(TokenType.RIGHT_SQUARE, "Expect ']' after dict key.");

        return new Expr.Get(dict, key, square);
    }

    private Expr Primary()
    {
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
            // () => xxx
            if (Match(TokenType.RIGHT_PAREN)) {
                return ArrowFunction(new List<Token>());
            }
            // expr
            Expr expr = Expression();
            List<Token>? parameters = null;
            // (a,b...)
            if (Match(TokenType.COMMA)) {
                parameters = new List<Token>();
                do {
                    Token param_name = Consume(TokenType.IDENTIFIER, "Expect parameter names.");
                    parameters.Add(param_name);
                }
                while (Match(TokenType.COMMA));
            }
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");

            if (Peek().type == TokenType.ARROW) {
                parameters = parameters ?? new List<Token>();
                if (expr is Expr.Variable first_param) {
                    parameters.Insert(0, first_param.name);
                }
                else {
                    throw Error(Peek(), "Expect parameter name in arrow-function.");
                }
                return ArrowFunction(parameters);
            }
            else {
                return expr;
            }
        }
        // Lambda Declaration
        else if (Match(TokenType.FUNC)) {
            return LambdaDeclaration();
        }

        throw Error(Peek(), "Expect expression.");
    }

    private Expr ArrowFunction(List<Token> parameters)
    {
        Consume(TokenType.ARROW, "Expect '=>' in arrow-function.");
        Expr return_value = Expression();
        return new Expr.Function(parameters, new List<Stmt>() { new Stmt.Return(return_value) });
    }

    private Expr LambdaDeclaration()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' before function parameters.");

        List<Token> parameters = new List<Token>();
        if (!Check(TokenType.RIGHT_PAREN)) {
            do {
                Token param_name = Consume(TokenType.IDENTIFIER, "Expect parameter names.");
                parameters.Add(param_name);
            }
            while (Match(TokenType.COMMA));
        }
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");

        Consume(TokenType.LEFT_BRACE, "Expect '{' before function body.");
        List<Stmt> body = Block();

        return new Expr.Function(parameters, body);
    }

    private List<Stmt> Block()
    {
        List<Stmt> statements = new List<Stmt>();

        while (!Check(TokenType.RIGHT_BRACE) && !IsEnd()) {
            statements.Add(Declaration());
        }

        Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    public List<Stmt> Parse(KulaEngine kula, List<Token> tokens)
    {
        this.kula = kula;
        this.tokens = tokens;
        this.current = 0;

        List<Stmt> statements = new List<Stmt>();
        while (!IsEnd()) {
            try {
                Stmt stmt = Declaration();
                if (stmt is Stmt.Sugar sugar) {
                    statements.AddRange(sugar.list);
                }
                else {
                    statements.Add(stmt);
                }
            }
            catch (ParseError) {
                Synchronize();
            }
        }

        return statements;
    }

    private void Synchronize()
    {
        Advance();

        while (!IsEnd()) {
            if (Previous().type == TokenType.SEMICOLON) return;
            if (Previous().type == TokenType.RIGHT_BRACE) return;
            switch (Peek().type) {
                case TokenType.CLASS:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.RETURN:
                case TokenType.PRINT:
                    return;
            }

            Advance();
        }
    }

    private Token Consume(TokenType type, string errmsg)
    {
        if (Check(type)) {
            return Advance();
        }
        throw Error(Peek(), errmsg);
    }

    private ParseError Error(Token token, string errmsg)
    {
        kula!.Error(token, errmsg);
        return ParseError.Instance;
    }

    private Token Previous()
    {
        return tokens![current - 1];
    }

    private Token Previous(int i)
    {
        return tokens![current - i];
    }

    private Token Peek()
    {
        return tokens![current];
    }

    private bool IsEnd()
    {
        return Peek().type == TokenType.EOF;
    }

    private bool Check(TokenType type)
    {
        return !IsEnd() && Peek().type == type;
    }

    private Token Advance()
    {
        if (!IsEnd()) {
            ++current;
        }
        return Previous();
    }

    private bool Match(TokenType type)
    {
        if (Check(type)) {
            Advance();
            return true;
        }
        return false;
    }

    private bool Match(params TokenType[] types)
    {
        foreach (TokenType type in types) {
            if (Check(type)) {
                Advance();
                return true;
            }
        }
        return false;
    }

    public class ParseError : Exception
    {
        public static ParseError Instance = new ParseError();
    }
}