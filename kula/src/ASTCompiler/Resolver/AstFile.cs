using Kula.ASTCompiler.Parser;

namespace Kula.ASTCompiler.Resolver;

class AstFile
{
    public FileInfo fileInfo;
    public List<FileInfo> nexts;
    public List<Stmt> stmts;

    public AstFile(FileInfo fileInfo, List<FileInfo> nexts, List<Stmt> stmts)
    {
        this.fileInfo = fileInfo;
        this.nexts = nexts;
        this.stmts = stmts;
    }

    public override bool Equals(object? obj)
    {
        return obj is AstFile file && file.fileInfo.FullName == this.fileInfo.FullName;
    }

    public override int GetHashCode()
    {
        return fileInfo.FullName.GetHashCode();
    }
}