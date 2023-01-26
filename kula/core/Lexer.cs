using Kula.Core.Ast;

namespace Kula.Core;

class Lexer
{
    private KulaEngine? kula;
    private string? source;
    private List<Token>? tokens;

    private int start;
    private int current;
    private int line, column;
    private TokenFile? tfile;

    private Lexer() { }

    public static Lexer Instance = new Lexer();
    private static Dictionary<string, TokenType> keywordDict = new Dictionary<string, TokenType>() {
        {"and", TokenType.AND},
        {"break", TokenType.BREAK},
        {"class", TokenType.CLASS},
        {"continue", TokenType.CONTINUE},
        {"else", TokenType.ELSE},
        {"false", TokenType.FALSE},
        {"func", TokenType.FUNC},
        {"for", TokenType.FOR},
        {"if", TokenType.IF},
        {"import", TokenType.IMPORT},
        {"null", TokenType.NULL},
        {"or", TokenType.OR},
        {"print", TokenType.PRINT},
        {"return", TokenType.RETURN},
        {"true", TokenType.TRUE},
        {"while", TokenType.WHILE}
    };

    public TokenFile Lex(KulaEngine kula, string source, string filename)
    {
        this.kula = kula;
        this.source = source;

        tokens = new List<Token>();
        tfile = new TokenFile(filename, tokens, source);
        start = 0;
        current = start;
        line = 1;
        column = 1;

        while (!IsEnd()) {
            start = current;
            ScanToken();
        }
        tokens.Add(new Token(TokenType.EOF, "", null, line, column, tfile));

        return tfile;
    }

    private void ScanToken()
    {
        char c = Advance();
        switch (c) {
            // Single Character Tokens
            case '(': AddToken(TokenType.LEFT_PAREN); break;
            case ')': AddToken(TokenType.RIGHT_PAREN); break;
            case '[': AddToken(TokenType.LEFT_SQUARE); break;
            case ']': AddToken(TokenType.RIGHT_SQUARE); break;
            case '{': AddToken(TokenType.LEFT_BRACE); break;
            case '}': AddToken(TokenType.RIGHT_BRACE); break;
            case ',': AddToken(TokenType.COMMA); break;
            case '.': AddToken(TokenType.DOT); break;
            case ';': AddToken(TokenType.SEMICOLON); break;
            // operator sugar
            case '-':
                AddToken(Match('=') ? TokenType.MINUS_EQUAL : TokenType.MINUS);
                break;
            case '+':
                AddToken(Match('=') ? TokenType.PLUS_EQUAL : TokenType.PLUS);
                break;
            case '*':
                AddToken(Match('=') ? TokenType.STAR_EQUAL : TokenType.STAR);
                break;
            case '/':
                AddToken(Match('=') ? TokenType.SLASH_EQUAL : TokenType.STAR);
                break;
            case '%':
                AddToken(Match('=') ? TokenType.MODULUS_EQUAL : TokenType.MODULUS);
                break;
            // Multi Character Tokens
            case ':':
                AddToken(Match('=') ? TokenType.COLON_EQUAL : TokenType.COLON);
                break;
            case '!':
                AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                break;
            case '=':
                if (Match('>')) {
                    AddToken(TokenType.ARROW);
                    break;
                }
                if (Match('=')) {
                    AddToken(TokenType.EQUAL_EQUAL);
                    break;
                }
                AddToken(TokenType.EQUAL);
                break;
            case '&':
                if (Match('&')) {
                    AddToken(TokenType.AND);
                    break;
                }
                kula!.Error((line, column, tfile!), "", "Unexpected character '&'.");
                break;
            case '|':
                if (Match('|')) {
                    AddToken(TokenType.OR);
                    break;
                }
                kula!.Error((line, column, tfile!), "", "Unexpected character '|'.");
                break;
            // Comment
            case '#':
                while (Peek() != '\n' && !IsEnd()) {
                    Advance();
                }
                break;
            // Blank
            case '\n':
                Newline();
                break;
            case ' ':
            case '\t':
            case '\r':
                break;
            // Literial
            case '"':
            case '\'':
            case '`':
                String(c);
                break;
            default:
                if (IsDigit(c)) {
                    Number();
                }
                else if (IsAlpha(c)) {
                    Identifier();
                }
                else {
                    kula!.Error((line, column, tfile!), "", $"Unexpected character '{c}'.");
                }
                break;
        }
    }

    private void String(char quote)
    {
        while ((Peek() != quote) && !IsEnd()) {
            if (Peek() == '\n') Newline();
            if (Peek() == '\\') Advance();
            Advance();
        }

        if (IsEnd()) {
            kula!.Error((line, column, tfile!), "", "Unterminated string.");
            return;
        }

        Advance();

        string value = source!.Substring(start + 1, current - start - 2);
        AddToken(TokenType.STRING, System.Text.RegularExpressions.Regex.Unescape(value));
    }

    private void Number()
    {
        while (IsDigit(Peek())) {
            Advance();
        }
        if (Peek() == '.' && IsDigit(PeekNext())) {
            Advance();
            while (IsDigit(Peek())) {
                Advance();
            }
        }

        AddToken(TokenType.NUMBER, Double.Parse(source!.Substring(start, current - start)));
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek())) {
            Advance();
        }
        string text = source!.Substring(start, current - start);
        AddToken(keywordDict.GetValueOrDefault(text, TokenType.IDENTIFIER));
    }

    private bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    private bool IsAlpha(char c)
    {
        return c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c == '_';
    }

    private bool IsAlphaNumeric(char c)
    {
        return IsAlpha(c) || IsDigit(c);
    }

    private char Advance()
    {
        ++column;
        return source![current++];
    }

    private bool Match(char c)
    {
        // if (IsEnd()) return false;
        // if (source![current] != c) return false;
        // Advance();
        // return true;
        if (Peek() != c) return false;
        Advance();
        return true;
    }

    private char Peek()
    {
        if (IsEnd()) return '\0';
        return source![current];
    }

    private char PeekNext()
    {
        if (current + 1 >= source!.Length) return '\0';
        return source![current + 1];
    }

    private bool IsEnd()
    {
        return current >= source!.Length;
    }

    private void Newline()
    {
        line++;
        column = 1;
    }

    private void AddToken(TokenType type)
    {
        AddToken(type, null);
    }

    private void AddToken(TokenType type, object? literial)
    {
        string lexeme = source!.Substring(start, current - start);
        tokens!.Add(new Token(type, lexeme, literial, line, column - (type == TokenType.STRING ? 1 : lexeme.Length), tfile!));
    }
}