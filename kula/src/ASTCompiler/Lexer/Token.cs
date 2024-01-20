namespace Kula.ASTCompiler.Lexer;

struct Token
{
    public readonly TokenType type;
    public readonly string lexeme;
    public readonly object? literial;
    public readonly (int, int, TokenFile) position;

    static (int, int, TokenFile) lastPosition;

    public Token(TokenType type, string lexeme, object? literial, int line, int column, TokenFile tfile)
    {
        this.type = type;
        this.lexeme = lexeme;
        this.literial = literial;
        position = (line, column, tfile);
        lastPosition = position;
    }

    public static Token MakeTemp(TokenType type, string lexeme)
    {
        return new Token(type, lexeme, null, -1, -1, lastPosition.Item3);
    }

    public static Token Fake(Token baseToken, TokenType newType)
    {
        return new Token(newType,
                         baseToken.lexeme,
                         baseToken.literial,
                         baseToken.position.Item1,
                         baseToken.position.Item2,
                         baseToken.position.Item3);
    }

    public override string ToString()
    {
        return
            $"[ ln {position.Item1.ToString().PadRight(4)}, col {position.Item2.ToString().PadRight(4)}: {type.ToString().PadRight(12)} ] => [ {lexeme.PadRight(12)} ]"
            + (literial is null ? "" : $" => [ {literial} ]");
    }
}

enum TokenType
{
    // Single-Character Tokens.
    LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE, LEFT_SQUARE, RIGHT_SQUARE,
    COMMA, DOT, MINUS, MODULUS, PLUS, SEMICOLON, SLASH, STAR,

    PLUS_EQUAL, MINUS_EQUAL, STAR_EQUAL, SLASH_EQUAL, MODULUS_EQUAL,

    // One or Two Character Tokens.
    BANG, BANG_EQUAL, EQUAL, EQUAL_EQUAL,
    GREATER, LESS, GREATER_EQUAL, LESS_EQUAL,
    COLON_EQUAL, COLON,
    ARROW,

    // Literials.
    IDENTIFIER, STRING, NUMBER,

    // Keyword
    AND, BREAK, CLASS, CONTINUE, ELSE, FALSE, FUNC, FOR, IF,
    IMPORT, NULL, OR, PRINT, RETURN, TRUE, WHILE,

    // EOF
    EOF
}
