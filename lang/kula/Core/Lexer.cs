using Kula.Xception;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kula.Core
{
    class Lexer
    {
        public static Lexer Instance { get; } = new Lexer();

        private StreamReader sourceCode;
        private List<LexToken> tokenStream;

        static class Is
        {
            public static bool CNumber(char c) { return (c <= '9' && c >= '0') || c == '.'; }
            public static bool CNumberHead(char c) { return (c <= '9' && c >= '0') || c == '+' || c == '-'; }
            public static bool CPlusMinus(char c) { return c == '+' || c == '-'; }
            public static bool COperator(char c) { return CPlusMinus(c) || c == '*' || c == '/'; }
            public static bool CName(char c)
            { return (c <= 'z' && c >= 'a') || (c <= 'Z' && c >= 'A') || (c == '_') || (c <= '9' && c >= '0'); }
            public static bool CSpace(char c) { return (c == '\n' || c == '\t' || c == '\r' || c == ' '); }
            public static bool CEnter(char c) { return c == '\r'; }
            public static bool CSymbol(char c)
            {
                switch (c)
                {
                    case ';':       // 语句结束符
                    case ',':       // 逗号分隔
                    case '.':       // 点操作符
                    case '|':       // 管道操作符
                    case ':':       // 类型限定
                    case '=':       // 赋值符
                        return true;
                    default:
                        return CBracket(c);
                }
            }
            public static bool CBracket(char c)
            {
                switch (c)
                {
                    case '(':
                    case ')':
                    case '{':
                    case '}':
                    case '<':
                    case '>':
                    case '[':
                    case ']':
                        return true;
                    default:
                        return false;
                }
            }
            public static bool CAnnotation(char c) { return c == '#'; }
            public static bool CQuote(char c) { return c == '\"' || c == '\''; }
        }

        private Lexer() { }

        public Lexer Read(StreamReader code, bool isDebug) 
        { 
            sourceCode = code;
            return Scan(isDebug);
        }

        private Lexer Scan(bool isDebug)
        {
            tokenStream = new List<LexToken>();
            LexTokenType state = LexTokenType.NULL;
            StringBuilder tokenBuilder = new StringBuilder();

                while (!sourceCode.EndOfStream)
                {
                    if (state == LexTokenType.NULL)
                    {
                        // char c = sourceCode.EndOfStream ? '\n' : (char)sourceCode.Read();
                        char c = (char)sourceCode.Read();
                        if (Is.CSpace(c)) { continue; }
                        else if (Is.CQuote(c))
                        {
                            state = LexTokenType.STRING;
                        }
                        else if (Is.CPlusMinus(c))
                        {
                            tokenBuilder.Append(c);
                            state = LexTokenType.NUMBER_OR_NAME;
                        }
                        else if (Is.COperator(c))
                        {
                            tokenStream.Add(new LexToken(LexTokenType.NAME, c.ToString()));
                        }
                        else if (Is.CNumberHead(c))
                        {
                            tokenBuilder.Append(c);
                            state = LexTokenType.NUMBER;
                        }
                        else if (Is.CName(c))
                        {
                            tokenBuilder.Append(c);
                            state = LexTokenType.NAME;
                        }
                        else if (Is.CSymbol(c))
                        {
                            tokenStream.Add(new LexToken(LexTokenType.SYMBOL, c.ToString()));
                        }
                        else if (Is.CAnnotation(c))
                        {
                            char cc = (char)sourceCode.Read();
                            while (!Is.CEnter(cc)) { cc = (char)sourceCode.Read(); }
                        }
                    }
                    else
                    {
                        switch (state)
                        {
                            case LexTokenType.NAME:
                                {
                                    while (Is.CName((char)sourceCode.Peek()))
                                    {
                                        tokenBuilder.Append((char)sourceCode.Read());
                                    }
                                }
                                break;
                            case LexTokenType.NUMBER:
                                {
                                    while (Is.CNumber((char)sourceCode.Peek()))
                                    {
                                        tokenBuilder.Append((char)sourceCode.Read());
                                    }
                                }
                                break;
                            case LexTokenType.NUMBER_OR_NAME:
                                {
                                    if (Is.CNumber((char)sourceCode.Peek()))
                                    {
                                        while (Is.CNumber((char)sourceCode.Peek()))
                                        {
                                            tokenBuilder.Append((char)sourceCode.Read());
                                        }
                                        state = LexTokenType.NUMBER;
                                    }
                                    else
                                    {
                                        state = LexTokenType.NAME;
                                    }
                                }
                                break;
                            case LexTokenType.STRING:
                                {
                                    bool trans = false;
                                    while (trans || !Is.CQuote((char)sourceCode.Peek()))
                                    {
                                        if (sourceCode.EndOfStream)
                                            throw new LexerException("string overflow");
                                    trans = !trans && (char)sourceCode.Peek() == '\\';
                                        tokenBuilder.Append((char)sourceCode.Read());
                                    }
                                    sourceCode.Read();
                                }
                                break;
                            default:
                                break;
                        }
                        string tokenString = tokenBuilder.ToString();
                        tokenStream.Add(new LexToken(state, tokenString));
                        state = LexTokenType.NULL;
                        tokenBuilder.Clear();
                    }
                }
            if (isDebug) { DebugShow(); }

            sourceCode = null;

            return this;
        }

        public Lexer DebugShow()
        {
            Console.WriteLine("Lexer ->");
            foreach (var token in tokenStream)
            {
                Console.ForegroundColor = LexToken.LexColorDict[token.Type];
                Console.Write("\t");
                Console.WriteLine(token);
            }
            Console.ResetColor();
            return this;
        }

        public List<LexToken> Out() { return this.tokenStream; }
    }
}
