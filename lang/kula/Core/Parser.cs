using System;
using System.Collections.Generic;

using Kula.Data;
using Kula.Util;

namespace Kula.Core
{
    class Parser
    {
        private static readonly Parser instance = new Parser();
        public static Parser Instance { get => instance; }

        private Func aimFunc;

        private readonly Stack<string> nameStack = new Stack<string>();
        private int pos;

        private static readonly Dictionary<string, Type> typeDict = new Dictionary<string, Type>
        {
            { "None", null },
            { "Any", typeof(object) },
            { "Num", typeof(float) },
            { "Str", typeof(string) },
            { "BuiltinFunc", typeof(Kula.Data.BuiltinFunc) },
            { "Func", typeof(Kula.Data.FuncEnv) },
            { "Array", typeof(Kula.Data.Array) },
            { "Map", typeof(Kula.Data.Map) },
        };
        public static Dictionary<string, Type> TypeDict { get => typeDict; }

        private Parser() { }

        public Parser Show()
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

        /// <summary>
        /// 整体解析 程序入口
        /// </summary>
        /// <param name="main">主代码块</param>
        public Parser Parse(Func main)
        {
            pos = 0; int _pos = -1;
            this.aimFunc = main;
            aimFunc.NodeStream.Clear();
            nameStack.Clear();
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
                catch (Exception e)
                {
                    throw e;
                }
            }
            if (pos != aimFunc.TokenStream.Count)
            {
                throw new KulaException.ParserException();
            }
            aimFunc.TokenStream.Clear();
            return this;
        }

        /// <summary>
        /// 整体解析 函数体
        /// </summary>
        /// <param name="func">函数</param>
        public Parser ParseLambda(Func func)
        {
            pos = 0; int _pos = -1;
            this.aimFunc = func;
            this.aimFunc.Compiled = true;

            func.NodeStream.Clear();
            nameStack.Clear();
            if (PLambdaDeclare())
            {
                if (PSymbol("{"))
                {
                    while (pos < aimFunc.TokenStream.Count - 1 && _pos != pos)
                    {
                        _pos = pos;
                        try
                        {
                            PStatement();
                        }
                        catch
                        {
                            break;
                        }
                            
                    }
                    if (pos == aimFunc.TokenStream.Count - 1 && PSymbol("}"))
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
        
        /// <summary>
        /// 只解析匿名函数 的大概形状
        /// </summary>
        private bool PLambdaHead()
        {
            int _pos = pos; int _size = aimFunc.NodeStream.Count;
            var token1 = aimFunc.TokenStream[pos++];
            var token2 = aimFunc.TokenStream[pos++];
            if (token1.Type == LexTokenType.NAME && token1.Value == "func"
                && token2.Type == LexTokenType.SYMBOL && token2.Value == "("
                )
            {
                return true;
            }

            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);
            return false;
        }

        /// <summary>
        /// 匿名函数 声明区
        /// </summary>
        private bool PLambdaDeclare()
        {
            int _pos = pos; int _size = aimFunc.NodeStream.Count;

            if (PLambdaHead())
            {
                while (!PSymbol(")"))
                {
                    if (PValAndType())
                    {
                        aimFunc.ArgTypes.Add(TypestrToType(nameStack.Pop()));
                        aimFunc.ArgNames.Add(nameStack.Pop());
                        PSymbol(",");
                    }
                    else
                    {
                        break;
                    }
                }
                if (PSymbol(":"))
                {
                    var final_type = aimFunc.TokenStream[pos++];
                    if (final_type.Type == LexTokenType.NAME && typeDict.ContainsKey(final_type.Value))
                    {
                        // 记录类型
                        aimFunc.ReturnType = (TypestrToType(final_type.Value));
                        return true;
                    }
                }
            }

            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);
            return false;
        }

        /// <summary>
        /// 值:类型 对
        /// </summary>
        private bool PValAndType()
        {
            int _pos = pos; int _size = aimFunc.NodeStream.Count;
            var token1 = aimFunc.TokenStream[pos++];
            var token2 = aimFunc.TokenStream[pos++];
            var token3 = aimFunc.TokenStream[pos++];
            if (token1.Type == LexTokenType.NAME 
                && token2.Type == LexTokenType.SYMBOL && token2.Value == ":"
                && token3.Type == LexTokenType.NAME && typeDict.ContainsKey(token3.Value)
            ) {
                nameStack.Push(token1.Value);
                nameStack.Push(token3.Value);
                return true;
            }

            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);
            return false;
        }
        private Type TypestrToType(string str)
        {
            return typeDict[str];
        }

        /// <summary>
        /// 单个语句  
        /// ;  
        /// n;  
        /// n := 1;  
        /// n = 1;  
        /// if 块  
        /// while 块  
        /// </summary>
        private bool PStatement()
        {
            int _pos = pos; int _size = aimFunc.NodeStream.Count;

            if (PSymbol(";")) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PReturn() && PSymbol(";")) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PValue() && PSymbol(";")) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PLeftVar() && PSymbol(":") && PSymbol("=") && PValue() && PSymbol(";"))
            {
                string var_name = nameStack.Pop();
                aimFunc.NodeStream.Add(new VMNode(VMNodeType.VAR, var_name));
                return true;
            }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PLeftVar() && PSymbol("=") && PValue() && PSymbol(";"))
            {
                string var_name = nameStack.Pop();
                aimFunc.NodeStream.Add(new VMNode(VMNodeType.LET, var_name));
                return true;
            }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PBlockIf()) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PBlockWhile()) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);


            return false;
        }

        /// <summary>
        /// 一个右值
        /// </summary>
        private bool PValue()
        {
            int count = 0;
            while (PNode()) { count++; }
            return count > 0;
        }

        /// <summary>
        /// 值元素
        /// （这个比较复杂
        /// </summary>
        private bool PNode()
        {
            int _pos = pos; int _size = aimFunc.NodeStream.Count;

            if (PConst()) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PConstString()) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PLambdaBody()) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PFuncBras()) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PRightVar()) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PRightIndex()) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PRightKey()) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            return false;
        }

        /// <summary>
        /// 解析 调用函数的一对括号
        /// </summary>
        private bool PFuncBras()
        {
            int _pos = pos; int _size = aimFunc.NodeStream.Count;

            if (PSymbol("("))
            {
                int count = 0;
                bool flag = true;
                while (flag)
                {
                    int _tmp_pos = pos; int _tmp_size = aimFunc.NodeStream.Count;
                    if (PValue())
                    {
                        ++count;
                        if (!PSymbol(","))
                        {
                            flag = false;
                        }
                    }
                    else
                    {
                        pos = _tmp_pos;
                        aimFunc.NodeStream.RemoveRange(_tmp_size, aimFunc.NodeStream.Count - _tmp_size);
                        flag = false;
                    }
                }

                if (PSymbol(")"))
                {
                    aimFunc.NodeStream.Add(new VMNode(VMNodeType.FUNC, count));
                    return true;
                }
            }

            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);
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
        /// 符号
        /// </summary>
        /// <param name="sym">解析的目标符号，不支持正则</param>
        private bool PSymbol(string sym)
        {
            int _pos = pos;
            var token = aimFunc.TokenStream[pos++];
            if (token.Type == LexTokenType.SYMBOL && token.Value == sym)
            {
                return true;
            }
            pos = _pos; return false;
        }

        /// <summary>
        /// 左值 
        /// 等待变量接收到本地表内
        /// </summary>
        private bool PLeftVar()
        {
            int _pos = pos;
            var token = aimFunc.TokenStream[pos++];
            if (token.Type == LexTokenType.NAME)
            {
                nameStack.Push(token.Value);
                return true;
            }
            pos = _pos; return false;
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
            int _pos = pos, _size = aimFunc.NodeStream.Count;
            if (PSymbol("[") && PValue() && PSymbol("]"))
            {
                aimFunc.NodeStream.Add(new VMNode(VMNodeType.CONKEY, '['));
                return true;
            }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);
            return false;
        }

        /// <summary>
        /// Map类型的右索引
        /// </summary>
        private bool PRightKey()
        {
            int _pos = pos, _size = aimFunc.NodeStream.Count;
            
            // <"key">
            if (PSymbol("<") && PValue() && PSymbol(">"))
            {
                aimFunc.NodeStream.Add(new VMNode(VMNodeType.CONKEY, '<'));
                return true;
            }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            // .key
            if (PSymbol("."))
            {
                var token_key = aimFunc.TokenStream[pos++];
                if (token_key.Type == LexTokenType.NAME)
                {
                    aimFunc.NodeStream.Add(new VMNode(VMNodeType.STRING, token_key.Value));
                    aimFunc.NodeStream.Add(new VMNode(VMNodeType.CONKEY, '<'));
                    return true;
                }
            }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            return false;
        }

        /// <summary>
        /// 解析 关键字
        /// </summary>
        /// <param name="kword">解析的目标关键字，不支持正则</param>
        private bool PKeyword(string kword)
        {
            int _pos = pos;
            var token = aimFunc.TokenStream[pos++];
            if (token.Type == LexTokenType.NAME && token.Value == kword)
            {
                return true;
            }
            pos = _pos; return false;
        }

        /// <summary>
        /// return 关键字 和 返回值
        /// </summary>
        private bool PReturn()
        {
            if (PKeyword("return") && PValue())
            {
                aimFunc.NodeStream.Add(new VMNode(VMNodeType.RETURN, null));
                return true;
            }
            return false;
        }

        /// <summary>
        /// lambda 函数体
        /// 格式如：
        ///     func (Num v1, Str v2) { ... } 
        /// 
        /// </summary>
        private bool PLambdaBody()
        {
            int _pos = pos, start_pos = _pos;
            if (PKeyword("func"))
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
            int _pos = pos; int _size = aimFunc.NodeStream.Count;

            if (PKeyword("if") && PSymbol("(") && PValue() && PSymbol(")"))
            {
                // 保存当前位置新建节点，用于稍后填充 IFGOTO
                // 注意：VMNode 是 struct，没有GC负担
                int tmpId = aimFunc.NodeStream.Count;
                aimFunc.NodeStream.Add(new VMNode());

                if (PSymbol("{"))
                {
                    while (PStatement()) ;
                    if (PSymbol("}"))
                    {
                        aimFunc.NodeStream[tmpId] = new VMNode(VMNodeType.IFGOTO, aimFunc.NodeStream.Count);
                        return true;
                    }
                }
            }

            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);
            return false;
        }

        /// <summary>
        /// while 块
        /// 格式如：
        ///     while ( xxx ) { yyy... }
        /// </summary>
        private bool PBlockWhile()
        {
            int _pos = 0; int _size = aimFunc.NodeStream.Count;

            int backPos = aimFunc.NodeStream.Count;
            if (PKeyword("while") && PSymbol("(") && PValue() && PSymbol(")"))
            {
                int tmpId = aimFunc.NodeStream.Count;
                aimFunc.NodeStream.Add(new VMNode());

                if (PSymbol("{"))
                {
                    while (PStatement()) ;
                    if (PSymbol("}"))
                    {
                        aimFunc.NodeStream[tmpId] = new VMNode(VMNodeType.IFGOTO, aimFunc.NodeStream.Count + 1);
                        aimFunc.NodeStream.Add(new VMNode(VMNodeType.GOTO, backPos));
                        return true;
                    }
                }
            }

            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);
            return false;
        }
    }
}
