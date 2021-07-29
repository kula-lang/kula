using System;
using System.Collections.Generic;
using System.Text;

using Kula.Data;
using Kula.Util;

namespace Kula.Core
{
    class Lexer
    {
        public static Lexer Instance { get; } = new Lexer();

        private string sourceCode;
        private List<LexToken> tokenStream;

        static class Is
        {
            public static bool CNumber(char c) { return (c <= '9' && c >= '0') || c == '.'; }
            public static bool CNumberHead(char c) { return (c <= '9' && c >= '0') || c == '+' || c == '-'; }
            public static bool CName(char c)
            { return (c <= 'z' && c >= 'a') || (c <= 'Z' && c >= 'A') || (c == '_') || (c <= '9' && c >= '0'); }
            public static bool CSpace(char c) { return (c == '\n' || c == '\t' || c == '\r' || c == ' '); }
            public static bool CNewLine(char c) { return c == '\n'; }
            public static bool CSymbol(char c)
            { return c == ';' || c == ',' || c == '.' || c == ':' || c == '=' || CBracket(c); }
            public static bool CBracket(char c)
            { return c == '(' || c == '{' || c == ')' || c == '}' || c == '[' || c == ']' || c == '<' || c == '>'; }
            public static bool CAnnotation(char c) { return c == '#'; }
            public static bool CQuote(char c) { return c == '\"' || c == '\''; }
        }
        private Lexer() { sourceCode = ""; }
        public Lexer Read(string code) { sourceCode = code; return this; }
        public Lexer Scan(bool isDebug)
        {
            tokenStream = new List<LexToken>();
            LexTokenType state = LexTokenType.NULL;
            StringBuilder tokenBuilder = new StringBuilder();

            try
            {
                for (int i = 0; i < sourceCode.Length; ++i)
                {
                    if (state == LexTokenType.NULL)
                    {
                        char c = sourceCode[i];
                        if (Is.CSpace(c)) { continue; }
                        else if (Is.CQuote(c))
                        {
                            state = LexTokenType.STRING;
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
                        // else if (Is.CPoint(c) || Is.CBracket(c) || Is.CAssign(c) || Is.CComma(c) || Is.CColon(c) || Is.CEnd(c))
                        else if (Is.CSymbol(c))
                        {
                            tokenStream.Add(new LexToken(LexTokenType.SYMBOL, c.ToString()));
                        }
                        else if (Is.CAnnotation(c))
                        {
                            while (i + 1 < sourceCode.Length && !Is.CNewLine(sourceCode[++i])) { }
                        }
                    }
                    else
                    {
                        switch (state)
                        {
                            case LexTokenType.NAME:
                                {
                                    while (i < sourceCode.Length && Is.CName(sourceCode[i]))
                                    {
                                        tokenBuilder.Append(sourceCode[i++]);
                                    }
                                }
                                break;
                            case LexTokenType.NUMBER:
                                {
                                    while (i < sourceCode.Length && Is.CNumber(sourceCode[i]))
                                    {
                                        tokenBuilder.Append(sourceCode[i++]);
                                    }
                                }
                                break;
                            case LexTokenType.STRING:
                                {
                                    while (i < sourceCode.Length && !Is.CQuote(sourceCode[i]))
                                    {
                                        tokenBuilder.Append(sourceCode[i++]);
                                    }
                                    ++i;
                                }
                                break;
                            default:
                                break;
                        }
                        string tokenString = tokenBuilder.ToString();
                        tokenStream.Add(new LexToken(state, tokenString));
                        state = LexTokenType.NULL;
                        tokenBuilder.Clear();
                        --i;
                    }
                }
            }
            catch (Exception)
            {
                tokenStream.Clear();
                throw new KulaException.LexerException();
            }
            if (isDebug) { DebugShow(); }

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
            Console.WriteLine();
            Console.ResetColor();
            return this;
        }
        public List<LexToken> Out() { return this.tokenStream; }
    }
}
