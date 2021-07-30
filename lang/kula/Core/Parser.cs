using System;
using System.Collections.Generic;

using Kula.Data;
using Kula.Util;

namespace Kula.Core
{
    class Parser
    {
        public static Parser Instance { get; } = new Parser();

        private Func aimFunc;

        private readonly Queue<Func> funcQueue = new Queue<Func>();
        private int pos;

        public static Dictionary<string, Type> TypeDict { get; private set; }
        public static Dictionary<Type, string> InvertTypeDict { get; private set; }

        private Parser() { }
        static Parser()
        {
            TypeDict = new Dictionary<string, Type>()
            {
                { "None", null },
                { "Any", typeof(object) },
                { "Num", typeof(float) },
                { "Str", typeof(string) },
                { "BuiltinFunc", typeof(Kula.Data.BFunc) },
                { "Func", typeof(Kula.Data.FuncWithEnv) },
                { "Array", typeof(Kula.Data.Array) },
                { "Map", typeof(Kula.Data.Map) },
            };

            if (InvertTypeDict == null)
            {
                InvertTypeDict = new Dictionary<Type, string>();
                foreach (var kv in TypeDict)
                {
                    if (kv.Value != null) InvertTypeDict[kv.Value] = kv.Key;
                }
            }
        }

        public Parser DebugShow()
        {
            Console.WriteLine("Parser ->");
            foreach (var node in aimFunc.NodeStream)
            {
                Console.ForegroundColor = VMNode.KvmColorDict[node.Type];
                Console.Write("\t");
                Console.WriteLine(node);
            }
            Console.WriteLine();
            Console.ResetColor();
            return this;
        }

        public Parser Parse(Func main, bool isDebug)
        {
            pos = 0; int _pos = -1;
            this.aimFunc = main;
            aimFunc.NodeStream.Clear();
            while (pos < aimFunc.TokenStream.Count && _pos != pos)
            {
                _pos = pos;
                try
                {
                    PStatement();
                }
                catch (IndexOutOfRangeException)
                {
                    throw new KulaException.ParserException();
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw new KulaException.ParserException();
                }
            }
            if (pos != aimFunc.TokenStream.Count)
            {
                if (isDebug) { DebugShow(); }
                throw new KulaException.ParserException();
            }
            aimFunc.TokenStream.Clear();
            if (isDebug) { DebugShow(); }

            while (funcQueue.Count > 0)
            {
                ParseLambda(funcQueue.Dequeue());
                if (isDebug) { DebugShow(); }
            }
            return this;
        }

        /// <summary>
        /// 整体解析 函数体
        /// </summary>
        /// <param name="func">函数</param>
        private Parser ParseLambda(Func func)
        {
            pos = 0; int _pos = -1;
            this.aimFunc = func;

            func.NodeStream.Clear();
            if (PLambdaDeclare())
            {
                if (MetaSymbol("{"))
                {
                    while (pos < aimFunc.TokenStream.Count - 1 && _pos != pos)
                    {
                        _pos = pos;
                        try
                        {
                            PStatement();
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new KulaException.ParserException();
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            throw new KulaException.ParserException();
                        }
                    }
                    if (pos == aimFunc.TokenStream.Count - 1 && MetaSymbol("}"))
                    {
                        aimFunc.TokenStream.Clear();
                        return this;
                    }
                }
            }
            aimFunc.NodeStream.Clear();
            aimFunc.TokenStream.Clear();
            throw new KulaException.ParserException();
        }

        /*
         *  以下部分涉及到一个硬核的手写递归下降语法分析器
         *  经过反复设计和重写，为了保证代码的可读，设计规范如下
         *  Meta系列函数：元分析，只单独分析单个Token，显式操作TokenSteam。必要时返回对应类型的值，做回溯处理。
         *  P系列函数：结构式分析，只调用Meta或递归，不显式操作TokenSteam，做回溯处理。
         */
        
        private bool MetaKeyword(string kword)
        {
            var rcd = Record();
            var token = aimFunc.TokenStream[pos++];
            if (token.Type == LexTokenType.NAME && token.Value == kword)
            {
                return true;
            }
            Backtrack(rcd);
            return false;
        }

        private bool MetaSymbol(string sym)
        {
            var rcd = Record();
            var token = aimFunc.TokenStream[pos++];
            if (token.Type == LexTokenType.SYMBOL && token.Value == sym)
            {
                return true;
            }
            Backtrack(rcd);
            return false;
        }

        private bool MetaName(out string name)
        {
            var rcd = Record();
            var token = aimFunc.TokenStream[pos++];
            if (token.Type == LexTokenType.NAME)
            {
                name = token.Value;
                return true;
            }
            name = null;
            Backtrack(rcd);
            return false;
        }

        private bool MetaType(out string typeName)
        {
            var rcd = Record();
            var token = aimFunc.TokenStream[pos++];
            if (token.Type == LexTokenType.NAME && TypeDict.ContainsKey(token.Value))
            {
                typeName = token.Value;
                return true;
            }
            typeName = null;
            Backtrack(rcd);
            return false;
        }

        private KeyValuePair<int, int> Record()
        {
            return new KeyValuePair<int, int>(pos, aimFunc.NodeStream.Count);
        }

        private void Backtrack(KeyValuePair<int, int> rcd)
        {
            pos = rcd.Key; 
            aimFunc.NodeStream.RemoveRange(rcd.Value, aimFunc.NodeStream.Count - rcd.Value);
        }

        /// <summary>
        /// 匿名函数 声明区
        /// </summary>
        private bool PLambdaDeclare()
        {
            var rcd = Record();

            if (MetaKeyword("func") && MetaSymbol("("))
            {
                while (!MetaSymbol(")"))
                {
                    if (PValAndType(out string val_name, out string type_name))
                    {
                        aimFunc.ArgTypes.Add(TypestrToType(type_name));
                        aimFunc.ArgNames.Add(val_name);
                        MetaSymbol(",");
                    }
                    else
                    {
                        break;
                    }
                }
                if (MetaSymbol(":"))
                {
                    if (MetaType(out string final_type_name))
                    {
                        // 记录类型
                        aimFunc.ReturnType = TypestrToType(final_type_name);
                        return true;
                    }
                }
            }

            Backtrack(rcd);
            return false;
        }

        /// <summary>
        /// 值:类型 对
        /// </summary>
        private bool PValAndType(out string valName, out string typeName)
        {
            var rcd = Record();
            if (MetaName(out string token_value1)
                && MetaSymbol(":")
                && MetaName(out string token_value2)
                && TypeDict.ContainsKey(token_value2))
            {
                valName = token_value1;
                typeName = token_value2;
                return true;
            }

            Backtrack(rcd);
            valName = null;
            typeName = null;
            return false;
        }
        private Type TypestrToType(string str)
        {
            return TypeDict[str];
        }

        /// <summary>
        /// 单个语句  
        ///     ;  
        ///     n;  
        ///     n := 1;  
        ///     n = 1;  
        ///     if 块  
        ///     while 块  
        /// </summary>
        private bool PStatement()
        {
            var rcd = Record();

            if (MetaSymbol(";")) { return true; }
            Backtrack(rcd);

            if (PReturn() && MetaSymbol(";")) { return true; }
            Backtrack(rcd);

            if (PValue() && MetaSymbol(";")) { return true; }
            Backtrack(rcd);

            if (PLeftVar(out string var_name))
            {
                bool flag = MetaSymbol(":");
                if (MetaSymbol("=") && PValue() && MetaSymbol(";"))
                {
                    aimFunc.NodeStream.Add(new VMNode(flag ? VMNodeType.VAR : VMNodeType.LET, var_name));
                    return true;
                }
            }
            Backtrack(rcd);

            if (PBlockIf()) { return true; }
            Backtrack(rcd);

            if (PBlockWhile()) { return true; }
            Backtrack(rcd);


            return false;
        }

        /// <summary>
        /// 一个右值
        /// </summary>
        private bool PValue()
        {
            bool count = false;
            while (PNode()) { count = true; }
            return count;
        }

        /// <summary>
        /// 值元素
        /// （这个比较复杂
        /// 可能包含：
        ///     常量
        ///     常字符串
        ///     匿名函数体
        ///     函数调用括号
        ///     右值
        ///     两种索引
        /// </summary>
        private bool PNode()
        {
            var rcd = Record();

            if (PConst()) { return true; }
            Backtrack(rcd);

            if (PConstString()) { return true; }
            Backtrack(rcd);

            if (PLambdaBody()) { return true; }
            Backtrack(rcd);

            if (PFuncBras()) { return true; }
            Backtrack(rcd);

            if (PRightVar()) { return true; }
            Backtrack(rcd);

            if (PRightIndex()) { return true; }
            Backtrack(rcd);

            if (PRightKey()) { return true; }
            Backtrack(rcd);

            return false;
        }

        /// <summary>
        /// 解析 调用函数的一对括号
        /// </summary>
        private bool PFuncBras()
        {
            var rcd = Record();

            if (MetaSymbol("("))
            {
                int count = 0;
                bool flag = true;
                while (flag)
                {
                    var tmp_rcd = Record();
                    if (PValue())
                    {
                        ++count;
                        if (!MetaSymbol(","))
                        {
                            flag = false;
                        }
                    }
                    else
                    {
                        Backtrack(tmp_rcd);
                        flag = false;
                    }
                }

                if (MetaSymbol(")"))
                {
                    aimFunc.NodeStream.Add(new VMNode(VMNodeType.FUNC, count));
                    return true;
                }
            }

            Backtrack(rcd);
            return false;
        }

        /// <summary>
        /// 常量
        /// </summary>
        private bool PConst()
        {
            int _pos = pos;
            var token = aimFunc.TokenStream[pos++];
            if (token.Type == LexTokenType.NUMBER)
            {
                aimFunc.NodeStream.Add(new VMNode(VMNodeType.VALUE, float.Parse(token.Value)));
                return true;
            }
            pos = _pos; return false;
        }

        /// <summary>
        /// 常字符串
        /// </summary>
        private bool PConstString()
        {
            int _pos = pos;
            var token = aimFunc.TokenStream[pos++];
            if (token.Type == LexTokenType.STRING)
            {
                aimFunc.NodeStream.Add(new VMNode(VMNodeType.STRING, token.Value));
                return true;
            }
            pos = _pos; return false;
        }

        /// <summary>
        /// 左值 
        /// 等待变量接收到本地表内
        /// </summary>
        private bool PLeftVar(out string name)
        {
            var rcd = Record();
            if (MetaName(out string token_value))
            {
                name = token_value;
                return true;
            }
            Backtrack(rcd);
            name = null;
            return false;
        }

        /// <summary>
        /// 右值 
        /// 可以解析变量名携带的值
        /// </summary>
        private bool PRightVar()
        {
            int _pos = pos;
            // 单一变量做右值
            var token = aimFunc.TokenStream[pos++];
            if (token.Type == LexTokenType.NAME)
            {
                aimFunc.NodeStream.Add(new VMNode(VMNodeType.NAME, token.Value));
                return true;
            }
            pos = _pos;

            return false;
        }

        /// <summary>
        /// Array类型的右索引
        /// </summary>
        private bool PRightIndex()
        {
            var rcd = Record();
            if (MetaSymbol("[") && PValue() && MetaSymbol("]"))
            {
                aimFunc.NodeStream.Add(new VMNode(VMNodeType.CONKEY, '['));
                return true;
            }
            Backtrack(rcd);
            return false;
        }

        /// <summary>
        /// Map类型的右索引
        /// </summary>
        private bool PRightKey()
        {
            var rcd = Record();

            // <"key">
            if (MetaSymbol("<") && PValue() && MetaSymbol(">"))
            {
                aimFunc.NodeStream.Add(new VMNode(VMNodeType.CONKEY, '<'));
                return true;
            }
            Backtrack(rcd);

            // .key 语法糖
            if (MetaSymbol(".") && MetaName(out string token_key))
            {
                aimFunc.NodeStream.Add(new VMNode(VMNodeType.STRING, token_key));
                aimFunc.NodeStream.Add(new VMNode(VMNodeType.CONKEY, '<'));
                return true;
            }
            Backtrack(rcd);

            return false;
        }

        

        /// <summary>
        /// return 关键字 和 返回值
        /// </summary>
        private bool PReturn()
        {
            if (MetaKeyword("return") && PValue())
            {
                aimFunc.NodeStream.Add(new VMNode(VMNodeType.RETURN, null));
                return true;
            }
            return false;
        }

        /// <summary>
        /// 分析 lambda 函数体，记录后备用
        /// 格式如：
        ///     func (Num v1, Str v2) { ... } 
        /// 
        /// </summary>
        private bool PLambdaBody()
        {
            int _pos = pos, start_pos = _pos;
            if (MetaKeyword("func"))
            {
                // 数大括号儿
                int count_stack = 1, count_pos = pos;
                while (aimFunc.TokenStream[count_pos].Type != LexTokenType.SYMBOL || aimFunc.TokenStream[count_pos].Value != "{")
                {
                    ++count_pos;
                }
                while (count_stack > 0)
                {
                    ++count_pos;
                    if (aimFunc.TokenStream[count_pos].Type == LexTokenType.SYMBOL)
                    {
                        if (aimFunc.TokenStream[count_pos].Value == "{") { ++count_stack; }
                        else if (aimFunc.TokenStream[count_pos].Value == "}") { --count_stack; }
                    }
                }
                // 截取Token 写入函数 待编译
                List<LexToken> func_tokens = aimFunc.TokenStream.GetRange(start_pos, count_pos - start_pos + 1);
                var func = new Func(func_tokens);

                // 将待使用函数送入编译队列
                funcQueue.Enqueue(func);

                aimFunc.NodeStream.Add(new VMNode(VMNodeType.LAMBDA, func));
                pos = count_pos + 1;
                return true;
            }
            pos = _pos;
            return false;
        }

        /// <summary>
        /// if 块
        /// 格式如：
        ///     if ( xxx ) { yyy... }
        /// </summary>
        private bool PBlockIf()
        {
            var rcd = Record();

            if (MetaKeyword("if") && MetaSymbol("(") && PValue() && MetaSymbol(")"))
            {
                // 保存当前位置新建节点，用于稍后填充 IFGOTO
                // 注意：VMNode 是 struct，没有GC负担，且需要 空new
                int if_id = aimFunc.NodeStream.Count;
                aimFunc.NodeStream.Add(new VMNode());

                if (MetaSymbol("{"))
                {
                    while (PStatement()) ;
                    if (MetaSymbol("}"))
                    {
                        // 到这里已经完成了 IF 语法的解析
                        // 补充一个 ELSE 实现
                        var else_rcd = Record();
                        if (MetaKeyword("else") && MetaSymbol("{"))
                        {
                            aimFunc.NodeStream[if_id] = new VMNode(VMNodeType.IFGOTO, aimFunc.NodeStream.Count + 1);

                            int else_id = aimFunc.NodeStream.Count;
                            aimFunc.NodeStream.Add(new VMNode());

                            while (PStatement()) ;
                            if (MetaSymbol("}"))
                            {
                                aimFunc.NodeStream[else_id] = new VMNode(VMNodeType.GOTO, aimFunc.NodeStream.Count);
                            }
                        }
                        else
                        {
                            aimFunc.NodeStream[if_id] = new VMNode(VMNodeType.IFGOTO, aimFunc.NodeStream.Count);
                            Backtrack(else_rcd);
                        }
                        return true;
                    }
                }
            }

            Backtrack(rcd);
            return false;
        }

        /// <summary>
        /// while 块
        /// 格式如：
        ///     while ( xxx ) { yyy... }
        /// </summary>
        private bool PBlockWhile()
        {
            var rcd = Record();

            int backPos = aimFunc.NodeStream.Count;
            if (MetaKeyword("while") && MetaSymbol("(") && PValue() && MetaSymbol(")"))
            {
                int while_id = aimFunc.NodeStream.Count;
                aimFunc.NodeStream.Add(new VMNode());

                if (MetaSymbol("{"))
                {
                    while (PStatement()) ;
                    if (MetaSymbol("}"))
                    {
                        aimFunc.NodeStream[while_id] = new VMNode(VMNodeType.IFGOTO, aimFunc.NodeStream.Count + 1);
                        aimFunc.NodeStream.Add(new VMNode(VMNodeType.GOTO, backPos));
                        return true;
                    }
                }
            }

            Backtrack(rcd);
            return false;
        }
    }
}
