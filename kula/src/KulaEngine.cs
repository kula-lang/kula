using Kula.ASTCompiler.Lexer;
using Kula.ASTCompiler.Parser;
using Kula.ASTCompiler.Resolver;
using Kula.ASTInterpreter;
using Kula.ASTInterpreter.Runtime;
using Kula.BytecodeCompiler.Compiler;
using Kula.BytecodeInterpreter;
using Kula.Utilities;

namespace Kula;

public class KulaEngine
{
    private bool hadError = false;
    private bool hadRuntimeError = false;

    private readonly AstPrinter astPrinter = new();
    private readonly Interpreter interpreter = new();
    private readonly VM vm = new();

    private Dictionary<string, AstFile> ReadFile(FileInfo file)
    {
        Dictionary<string, AstFile> readFiles = new();
        ReadFile1(file, readFiles);
        return readFiles;
    }

    private void ReadFile1(FileInfo file, Dictionary<string, AstFile> readFiles)
    {
        if (hadError) { return; }

        if (!file.Exists) {
            throw new InterpreterInnerException($"File Not Exist. {file.FullName}");
        }
        if (readFiles.ContainsKey(file.FullName)) {
            return;
        }

        TokenFile tfile = Lexer.Instance.Lex(this, file.OpenText().ReadToEnd(), file.Name);
        if (hadError) { return; }
        List<Stmt> asts = Parser.Instance.Parse(this, tfile.tokens);
        if (hadError) { return; }

        List<FileInfo> nexts = ModuleResolver.Instance.AnalyzeAST(file.Directory!, asts);
        readFiles.Add(file.FullName, new AstFile(file, nexts, asts));

        foreach (var next in nexts) {
            ReadFile1(next, readFiles);
        }
    }

    private bool RunStmts(List<Stmt> stmts)
    {
        interpreter.Interpret(this, stmts);
        if (hadRuntimeError) {
            return false;
        }
        return true;
    }

    private bool RunFile(FileInfo file)
    {
        hadError = false;
        hadRuntimeError = false;

        Dictionary<string, AstFile> readFiles;
        try {
            readFiles = ReadFile(file);
        }
        catch (InterpreterInnerException e) {
            ReportError(e.Message);
            return false;
        }
        if (hadError) {
            return false;
        }

        List<AstFile> ast_files = ModuleResolver.Instance.Resolve(readFiles, file);
        List<Stmt> stmts = new();
        foreach (var ast_file in ast_files) {
            stmts.AddRange(ast_file.stmts);
        }

        Resolver.Instance.Resolve(this, stmts);
        if (hadError) {
            return false;
        }

        RunStmts(stmts);

        return true;
    }

    private bool RunCompiledFile(FileInfo file)
    {
        hadError = false;
        hadRuntimeError = false;
        using (BinaryReader bw = new(file.OpenRead())) {
            CompiledFile compiledFile = new(bw);
            try {
                vm.Interpret(this, compiledFile);
            }
            catch (InterpreterException e) {
                RuntimeError(e);
                return false;
            }
        }
        return true;
    }

    private bool CompileFile(FileInfo file, FileInfo aim)
    {
        hadError = false;
        hadRuntimeError = false;

        Dictionary<string, AstFile> readFiles;
        try {
            readFiles = ReadFile(file);
        }
        catch (InterpreterInnerException e) {
            ReportError(e.Message);
            return false;
        }
        if (hadError) {
            return false;
        }

        List<AstFile> ast_files = ModuleResolver.Instance.Resolve(readFiles, file);
        List<Stmt> stmts = new();
        foreach (var ast_file in ast_files) {
            stmts.AddRange(ast_file.stmts);
        }

        Resolver.Instance.Resolve(this, stmts);
        if (hadError) {
            return false;
        }

        CompiledFile compiledFile = Compiler.Instance.Compile(stmts);
        using (var stream = aim.Open(FileMode.Create)) {
            using var bw = new BinaryWriter(stream);
            compiledFile.Write(bw);
        }
#if DEBUG
        Console.WriteLine(compiledFile.ToString());
#endif

        return true;
    }

    internal bool RunSource(string source, string filename)
    {
        hadError = false;
        hadRuntimeError = false;

        TokenFile tfile = Lexer.Instance.Lex(this, source, filename);
#if DEBUG
        foreach (Token token in tfile.tokens) {
            Console.WriteLine(token);
        }
#endif
        if (hadError) {
            return false;
        }
        List<Stmt> asts = Parser.Instance.Parse(this, tfile.tokens);
#if DEBUG
        foreach (Stmt stmt in asts) {
            Console.WriteLine(astPrinter.Print(stmt));
        }
#endif
        if (hadError) {
            return false;
        }

        Resolver.Instance.Resolve(this, asts);
        if (hadError) {
            return false;
        }

        return RunStmts(asts);
    }

    public bool Run(string source)
    {
        return RunSource(source, "<stdin>");
    }

    public bool Run(FileInfo file)
    {
        if (file.Exists) {
            return RunFile(file);
        }
        return false;
    }

    public bool RunC(FileInfo file)
    {
        if (file.Exists) {
            return RunCompiledFile(file);
        }
        return false;
    }

    public void DeclareFunction(string fnName, NativeFunction function)
    {
        interpreter.globals.Define(fnName, function);
    }

    public bool Compile(FileInfo file, FileInfo aim)
    {
        if (file.Exists) {
            return CompileFile(file, aim);
        }
        return false;
    }
    
    internal void LexError((int, int, TokenFile) position, string lexeme, string msg)
    {
        ReportError(position, $"'{lexeme}'", msg, false);
    }

    internal void ParseError(Token token, string msg)
    {
        ReportError(token.position, token.type == TokenType.EOF ? "EOF" : $"'{token.lexeme}'", msg, false);
    }

    internal void ResolveError(Token token, string msg)
    {
        ReportError(token.position, token.lexeme, msg, false);
    }

    internal void RuntimeError(InterpreterException runtimeError)
    {
        Token token = runtimeError.token;
        ReportError(token.position, token.lexeme, runtimeError.Message, true);
    }

    internal void RuntimeError(BytecodeInterpreter.Runtime.VMException runtimeError)
    {
        ReportError($"Error: [{runtimeError.instruction.Op}] {runtimeError.Message}");
    }

    private void ReportError((int, int, TokenFile) pos, string lexeme, string msg, bool isExactly)
    {
        ConsoleColor color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;

        Console.Error.WriteLine($"File \"{pos.Item3.filename}\", ln {pos.Item1}, col {pos.Item2}, {lexeme}");
        if (pos.Item1 >= 0) {
            (string err_source, int err_pos) = pos.Item3.ErrorLog(pos.Item1, pos.Item2);
            Console.Error.WriteLine("  " + err_source);
            string prompt = "  " + new string('-', err_pos - 1);
            if (isExactly) {
                prompt += new string('^', lexeme.Length)
                        + new string('-', err_source.Length - lexeme.Length - err_pos + 1);
            }
            else {
                prompt += '^';
            }
            Console.Error.WriteLine(prompt);
        }
        Console.Error.WriteLine($"Error: {msg}");

        Console.ForegroundColor = color;

        hadError = true;
    }

    private void ReportError(string msg)
    {
        ConsoleColor color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(msg);
        Console.ForegroundColor = color;

        hadError = true;
    }
}
