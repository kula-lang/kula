using System;
using System.Text;

class Repl
{
    static readonly (char, char)[] brackets = new (char, char)[]
    {
            ('(', ')'),
            ('{', '}'),
            ('[', ']'),
    };

    readonly int[] bracketCounter;
    readonly StringBuilder codeBuilder;

    /// 检查括号是否匹配
    /// 0   =>  完整匹配
    /// x>0 =>  还剩下 x 个括号未匹配
    /// -1  =>  无法匹配
    private int IsBracketsMatched()
    {
        int uncompleted = 0;
        for (int i = 0; i < bracketCounter.Length; ++i)
        {
            int cc = bracketCounter[i];
            if (cc < 0)
                return -1;
            if (cc > 0)
                uncompleted += cc;
        }
        return uncompleted;
    }

    private void AddBracket(char c)
    {
        for (int i = 0; i < brackets.Length; ++i)
        {
            var bra = brackets[i];
            if (c == bra.Item1)
            {
                bracketCounter[i]++;
                break;
            }
            if (c == bra.Item2)
            {
                bracketCounter[i]--;
                break;
            }
        }
    }

    public Repl()
    {
        bracketCounter = new int[brackets.Length];
        codeBuilder = new StringBuilder();
    }

    public string ReadCode()
    {
        bracketCounter.Initialize();
        codeBuilder.Clear();

        int matched_flag = 0xff;
        int lines = 0;
        Console.WriteLine();

        for (; ; )
        {
            Console.Write(matched_flag == 0xff ? ">> " : ".. ");
            string code = Console.ReadLine();
            lines += 1;
            foreach (char cc in code)
            {
                AddBracket(cc);
            }
            codeBuilder.Append(code);
            codeBuilder.Append('\n');
            matched_flag = IsBracketsMatched();

            if (matched_flag < 0)
                break;
            if (matched_flag == 0)
            {
                if (lines == 1 || code == string.Empty)
                    break;
            }
        }

        if (matched_flag == -1)
        {
            return string.Empty;
        }
        else
        {
            return codeBuilder.ToString();
        }
    }
}