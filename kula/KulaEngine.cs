using Kula.Core;
using Kula.Core.Ast;
using Kula.Core.Runtime;

namespace Kula;

public class KulaEngine
{
    private bool hadError = false;
    private bool hadRuntimeError = false;

    private AstPrinter astPrinter = new AstPrinter();
    private static Dictionary<string, List<Stmt>> scannedFiles = new Dictionary<string, List<Stmt>>();
    private HashSet<string> finishedFiles = new HashSet<string>();

    internal void LoadRun(FileInfo file) {
        if (!file.Exists) {
            throw new RuntimeInnerError($"File Not Exist.");
        }
        
        string fullname = file.FullName;
        if (finishedFiles.Contains(fullname)) {
            return;
        }
        else {
            finishedFiles.Add(fullname);
            RunSource(file.OpenText().ReadToEnd(), file.Name, false);
        }
    }

    internal void RunSource(string source, string filename, bool isDebug)
    {
        hadError = false;
        hadRuntimeError = false;

        TokenFile tfile = Lexer.Instance.Lex(this, source, filename);
        if (isDebug) {
            foreach (Token token in tfile.tokens) {
                Console.WriteLine(token);
            }
        }
        if (hadError) {
            return;
        }
        List<Stmt> asts = Parser.Instance.Parse(this, tfile.tokens);
        if (isDebug) {
            foreach (Stmt stmt in asts) {
                Console.WriteLine(astPrinter.Print(stmt));
            }
        }
        if (hadError) {
            return;
        }

        Interpreter.Instance.Interpret(this, asts);
        if (hadRuntimeError) {
            return;
        }
    }

    public void Run(string source)
    {
        RunSource(source, "<stdin>", false);
    }

    public void Run(FileInfo file)
    {
        if (file.Exists) {
            LoadRun(file);
        }
        // List<FileInfo> source_files = ModuleResolver.Instance.Resolve(this, file);
        // foreach (FileInfo file_info in source_files) {
        //     Run(file_info.OpenText().ReadToEnd(), file_info.Name, false);
        //     if (hadError || hadRuntimeError) {
        //         return;
        //     }
        // }
    }

    public void DebugRun(string source)
    {
        RunSource(source, "<stdin>", true);
    }

    internal string? Input()
    {
        return Console.ReadLine();
    }

    internal void Print(string msg)
    {
        Console.WriteLine(msg);
    }

    internal void Error((int, int, TokenFile) position, string lexeme, string msg)
    {
        ReportError(position, $"'{lexeme}'", msg);
    }

    internal void Error(Token token, string msg)
    {
        if (token.type == TokenType.EOF) {
            ReportError(token.position, "EOF", msg);
        }
        else {
            ReportError(token.position, $"'{token.lexeme}'", msg);
        }
    }

    internal void RuntimeError(RuntimeError runtimeError)
    {
        var pos = runtimeError.name.position;
        Console.Error.WriteLine(
            $"File \"{pos.Item3.filename}\", ln {pos.Item1}, col {pos.Item2}, '{runtimeError.name.lexeme}'");
        if (pos.Item1 >= 0) {
            (string err_source, int err_pos) = pos.Item3.ErrorLog(pos.Item1, pos.Item2);
            Console.Error.WriteLine("  " + err_source);
            Console.Error.WriteLine(
                "  "
                + new String('-', err_pos - 1)
                + new String('^', runtimeError.name.lexeme.Length)
                + new String('-', err_source.Length - runtimeError.name.lexeme.Length - err_pos + 1));
        }
        Console.Error.WriteLine($"Runtime Error: {runtimeError.Message}");
        hadRuntimeError = true;
    }

    private void ReportError((int, int, TokenFile) pos, string lexeme, string msg)
    {
        Console.Error.WriteLine($"File \"{pos.Item3.filename}\", ln {pos.Item1}, col {pos.Item2}, {lexeme}");
        if (pos.Item1 >= 0) {
            (string err_source, int err_pos) = pos.Item3.ErrorLog(pos.Item1, pos.Item2);
            Console.Error.WriteLine("  " + err_source);
            Console.Error.WriteLine(
                "  "
                + new String('-', err_pos - 1)
                + '^');
        }
        Console.Error.WriteLine($"Compile Error: {msg}");
        hadError = true;
    }
}
