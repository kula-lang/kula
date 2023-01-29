using Kula.Core;
using Kula.Core.Ast;
using Kula.Core.Runtime;

namespace Kula;

public class KulaEngine
{
    private bool hadError = false;
    private bool hadRuntimeError = false;

    private AstPrinter astPrinter = new AstPrinter();
    private HashSet<string> usedFiles = new HashSet<string>();
    private Interpreter interpreter = new Interpreter(200);

    internal bool RunFile(FileInfo file)
    {
        if (!file.Exists) {
            throw new RuntimeInnerError($"File Not Exist.");
        }

        string fullname = file.FullName;
        if (usedFiles.Contains(fullname)) {
            return true;
        }
        else {
            usedFiles.Add(fullname);
            return RunSource(file.OpenText().ReadToEnd(), file.Name, false);
        }
    }

    internal bool RunSource(string source, string filename, bool isDebug)
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
            return false;
        }
        List<Stmt> asts = Parser.Instance.Parse(this, tfile.tokens);
        if (isDebug) {
            foreach (Stmt stmt in asts) {
                Console.WriteLine(astPrinter.Print(stmt));
            }
        }
        if (hadError) {
            return false;
        }

        interpreter.Interpret(this, asts);
        if (hadRuntimeError) {
            return false;
        }

        return true;
    }

    public bool Run(string source)
    {
        return RunSource(source, "<stdin>", false);
    }

    public bool Run(FileInfo file)
    {
        if (file.Exists) {
            return RunFile(file);
        }
        return false;
    }

    public void DeclareFunction(string fnName, NativeFunction function) {
        interpreter.globals.Define(fnName, function);
    }

    internal string? Input()
    {
        return Console.ReadLine();
    }

    internal void Print(string msg)
    {
        Console.WriteLine(msg);
    }

    internal void LexError((int, int, TokenFile) position, string lexeme, string msg)
    {
        ReportError(position, $"'{lexeme}'", msg, false);
    }

    internal void ParseError(Token token, string msg)
    {
        if (token.type == TokenType.EOF) {
            ReportError(token.position, "EOF", msg, false);
        }
        else {
            ReportError(token.position, $"'{token.lexeme}'", msg, false);
        }
    }

    internal void RuntimeError(RuntimeError runtimeError)
    {
        Token token = runtimeError.name;
        ReportError(token.position, token.lexeme, runtimeError.Message, true);
    }

    private void ReportError((int, int, TokenFile) pos, string lexeme, string msg, bool isExactly)
    {
        ConsoleColor color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;

        Console.Error.WriteLine($"File \"{pos.Item3.filename}\", ln {pos.Item1}, col {pos.Item2}, {lexeme}");
        if (pos.Item1 >= 0) {
            (string err_source, int err_pos) = pos.Item3.ErrorLog(pos.Item1, pos.Item2);
            Console.Error.WriteLine("  " + err_source);
            string prompt = "  " + new String('-', err_pos - 1);
            if (isExactly) {
                prompt += new String('^', lexeme.Length)
                        + new String('-', err_source.Length - lexeme.Length - err_pos + 1);
            }
            else {
                prompt += '^';
            }
            Console.Error.WriteLine(prompt);
        }
        Console.Error.WriteLine($"Error: {msg}");
        hadError = true;

        Console.ForegroundColor = color;
    }
}
