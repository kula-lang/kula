using kula.Util;

namespace kula.Data
{
    class KvmNode
    {
        KvmNodeType type;
        object value;

        public KvmNode(KvmNodeType type, object value)
        {
            this.type = type;
            this.value = value;
        }

        public KvmNodeType Type { get => type; set => type = value; }
        public object Value { get => value; set => this.value = value; }

        public override string ToString()
        {
            string str_type = type.ToString(), str_value = value.ToString();
            return ""
                    + "[ "
                    + str_type.PadRight(9)
                    + "| "
                    + str_value.PadRight(18)
                    + " ]";
        }
    }

    enum KvmNodeType : byte
    {
        VALUE,      // 常量值
        STRING,     // 常字符串
        VARIABLE,   // 待接收
        NAME,       // 变量名，解析为值
        FUNC,       // 函数名，解析为函数
        IFGOTO,     // 为零时跳转
        GOTO,       // 无条件跳转
        RETURN,     // 返回值
    }
}
