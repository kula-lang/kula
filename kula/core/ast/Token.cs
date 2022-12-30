namespace Kula.Core.Ast;

struct Token {
    public readonly TokenType type;
    public readonly string lexeme;
    public readonly object? literial;
    public readonly int line;

    public Token(TokenType type, string lexeme, object? literial, int line) {
        this.type = type;
        this.lexeme = lexeme;
        this.literial = literial;
        this.line = line;
    }

    public static Token MakeTemp(string lexeme) {
        return new Token(TokenType.IDENTIFIER, lexeme, null, -1);
    }

    public static Token MakeTemp(TokenType type, string lexeme) {
        return new Token(type, lexeme, null, -1);
    }

    public override string ToString() {
        return
            $"[ line {line.ToString().PadRight(4)}: {type.ToString().PadRight(12)} ] => [ {lexeme.PadRight(12)} ]"
            + (literial is null ? "" : $" => [ {literial} ]");
    }
}

enum TokenType {
    // Single-Character Tokens.
    LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE, LEFT_SQUARE, RIGHT_SQUARE,
    COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,

    // One or Two Character Tokens.
    BANG, BANG_EQUAL, EQUAL, EQUAL_EQUAL,
    GREATER, LESS, GREATER_EQUAL, LESS_EQUAL,
    COLON_EQUAL, COLON,
    ARROW,

    // Literials.
    IDENTIFIER, STRING, NUMBER,

    // Keyword
    AND, BREAK, CLASS, ELSE, FALSE, FUNC, FOR, IF,
    NULL, OR, PRINT, RETURN, TRUE, WHILE,

    // EOF
    EOF
}