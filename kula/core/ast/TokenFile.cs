namespace Kula.Core.Ast;

class TokenFile
{
    public string filename { get; private set; }
    public List<Token> tokens { get; private set; }
    public string source { get; private set; }

    public TokenFile(string filename, List<Token> tokens, string source)
    {
        this.filename = filename;
        this.tokens = tokens;
        this.source = source;
    }

    public (string, int) ErrorLog(int line, int column) {
        string str = source.Split(Environment.NewLine)[line-1];
        if (str.Length > 30) {
            int cut_pos = Math.Min(column - 15, str.Length - 30);
            return (str.Substring(cut_pos), column - cut_pos);
        }
        return (str, column);
    }
}