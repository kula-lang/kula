using Kula.Core;
using Kula.Core.Ast;
using Kula.Core.Runtime;

namespace Kula;

public class KulaEngine {
    private bool hadError = false;
    private bool hadRuntimeError = false;
    private string filename = string.Empty;

    private AstPrinter astPrinter = new AstPrinter();

    private void Run(string source, string filename, bool isDebug) {
        this.filename = filename;

        hadError = false;
        hadRuntimeError = false;

        List<Token> tokens = Lexer.Instance.ScanTokens(this, source);
        if (isDebug) {
            foreach (Token token in tokens) {
                Console.WriteLine(token);
            }
        }
        if (hadError) {
            return;
        }
        List<Stmt> asts = Parser.Instance.Parse(this, tokens);
        if (isDebug) {
            foreach (Stmt stmt in asts) {
                Console.WriteLine(stmt);
            }
        }
        if (hadError) {
            return;
        }

        Interpreter.Instance.Interpret(this, asts);
    }

    public void Run(string source) {
        Run(source, "TempFile", false);
    }

    public void Run(FileInfo file) {
        List<FileInfo> source_files = ModuleResolver.Instance.Resolve(this, file);
        foreach (FileInfo file_info in source_files) {
            Run(file_info.OpenText().ReadToEnd(), file_info.Name, false);
            if (hadError || hadRuntimeError) {
                return;
            }
        }
    }

    public void DebugRun(string source) {
        Run(source, "DebugTempFile", true);
    }

    internal string? Input() {
        return Console.ReadLine();
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
        Console.Error.WriteLine($"Runtime Error in '{filename}'");
        if (runtimeError.name is not null) {
            Console.Error.WriteLine($"Runtime Error At [line {runtimeError.name?.line}]:");
        }
        else {
            Console.Error.WriteLine($"Runtime Error:");
        }
        Console.Error.WriteLine(runtimeError.Message);
        hadRuntimeError = true;
    }

    private void ReportError(int line, string position, string msg) {
        Console.Error.WriteLine($"Error in '{filename}'");
        Console.Error.WriteLine($"[line {line}] Error {position}: {msg}");
        hadError = true;
    }
}
