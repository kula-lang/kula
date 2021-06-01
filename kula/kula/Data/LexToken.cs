using kula.Util;

namespace kula.Data
{
    class LexToken
    {
        private LexTokenType type;
        private string value;

        public string Value { get => value; }
        public LexTokenType Type { get => type; }

        public LexToken(LexTokenType type, string value)
        {
            this.type = type;
            this.value = value;
        }

        public override string ToString()
        {
            string str_type = type.ToString();
            return ""
                    + "< "
                    + str_type.PadRight(9)
                    + "| "
                    + value.PadRight(18)
                    + " >";
        }
    }

    enum LexTokenType : byte
    {
        KEYWORD,    // 关键字
        TYPE,       // 类型名
        NAME,       // 名字，可解析为 变量名 函数名
        NUMBER,     // 数字，float
        STRING,     // 字符串，string
        SYMBOL,     // 符号
    }
}
