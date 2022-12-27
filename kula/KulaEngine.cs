using Kula.Core;
using Kula.Core.Ast;
using Kula.Core.Runtime;

namespace Kula;

public class KulaEngine {
    private bool hadError = false;
    private bool hadRuntimeError = false;

    private AstPrinter astPrinter = new AstPrinter();

    public void Run(String source) {
        List<Token> tokens = Lexer.Instance.ScanTokens(this, source);
        foreach (Token token in tokens) {
            Console.WriteLine(token);
        }
        if (hadError) {
            Console.Error.WriteLine("LEX ERROR");
            return;
        }


        List<Stmt> asts = Parser.Instance.Parse(this, tokens);
        foreach (Stmt statement in asts) {
            Console.WriteLine(astPrinter.Print(statement));
        }
        if (hadError) {
            Console.Error.WriteLine("PARSE ERROR");
            return;
        }

        Interpreter.Instance.Interpret(this, asts);
        if (hadRuntimeError) {
            Console.Error.WriteLine("RUNTIME ERROR");
            return;
        }
    }

    internal void Print(string msg) {
        Console.WriteLine(msg);
    }

    internal void Error(int line, string msg) {
        ReportError(line, "", msg);
    }

    internal void Error(Token token, string msg) {
        if (token.type == TokenType.EOF) {
            ReportError(token.line, "at end", msg);
        }
        else {
            ReportError(token.line, $"at '{token.lexeme}'", msg);
        }
    }

    internal void RuntimeError(RuntimeError runtimeError) {
        Console.Error.WriteLine($"Runtime Error At [line {runtimeError.name.line}]:");
        Console.Error.WriteLine(runtimeError.Message);
        hadRuntimeError = true;
    }

    private void ReportError(int line, string position, string msg) {
        Console.Error.WriteLine($"[line {line}] Error {position}: {msg}");
        hadError = true;
    }
}
