using Kula.Data;
using Kula.Data.Container;
using Kula.Data.Function;
using Kula.Data.Type;
using Kula.Util;
using System;
using System.Collections.Generic;

namespace Kula.Core
{
    class Parser
    {
        private Parser() { }
        public static Parser Instance { get; } = new Parser();

        private Lambda aimLambda;

        private readonly Queue<Lambda> lambdaQue = new Queue<Lambda>();
        private int pos;
        private int pipes;
        private KulaEngine engineRoot;

        public Parser DebugShow()
        {
            Console.WriteLine("Parser ->");
            foreach (var node in aimLambda.NodeStream)
            {
                Console.ForegroundColor = VMNode.KvmColorDict[node.Type];
                Console.Write("\t");
                Console.WriteLine(node);
            }
            Console.ResetColor();
            return this;
        }

        public Parser Parse(KulaEngine engine, Lambda main, bool isDebug)
        {
            this.engineRoot = engine;
            pos = 0; int _pos = -1;
            pipes = 0;
            this.aimLambda = main;
            this.aimLambda.ReturnType = RawType.None;

            aimLambda.NodeStream.Clear();
            while (pos < aimLambda.TokenStream.Count && _pos != pos)
            {
                _pos = pos;
                PStatement();
            }
            if (pos != aimLambda.TokenStream.Count)
            {
                if (isDebug) { DebugShow(); }
                throw new KulaException.ParserException();
            }
            aimLambda.TokenStream.Clear();
            if (isDebug) { DebugShow(); }

            while (lambdaQue.Count > 0)
            {
                ParseLambda(lambdaQue.Dequeue());
                if (isDebug) { DebugShow(); }
            }
            return this;
        }

        /// <summary>
        /// 整体解析 函数体
        /// </summary>
        /// <param name="func">函数</param>
        private Parser ParseLambda(Lambda func)
        {
            pos = 0; int _pos = -1;
            this.aimLambda = func;

            func.NodeStream.Clear();
            if (PLambdaDeclare())
            {
                if (MetaSymbol("{"))
                {
                    while (pos < aimLambda.TokenStream.Count - 1 && _pos != pos)
                    {
                        _pos = pos;
                        PStatement();
                    }
                    if (pos == aimLambda.TokenStream.Count - 1 && MetaSymbol("}"))
                    {
                        aimLambda.TokenStream.Clear();
                        return this;
                    }
                }
            }
            aimLambda.NodeStream.Clear();
            aimLambda.TokenStream.Clear();
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
            // 过长
            if (pos >= aimLambda.TokenStream.Count) return false;
            var token = aimLambda.TokenStream[pos++];
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
            if (pos >= aimLambda.TokenStream.Count) return false;
            var token = aimLambda.TokenStream[pos++];
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
            if (pos >= aimLambda.TokenStream.Count)
            {
                name = null;
                return false;
            }
            var token = aimLambda.TokenStream[pos++];
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
            if (pos >= aimLambda.TokenStream.Count)
            {
                typeName = null;
                return false;
            }
            var token = aimLambda.TokenStream[pos++];
            if (token.Type == LexTokenType.NAME /*&& TypeDict.ContainsKey(token.Value)*/)
            {
                typeName = token.Value;
                return true;
            }
            typeName = null;
            Backtrack(rcd);
            return false;
        }

        private (int, int) Record()
        {
            return (pos, aimLambda.NodeStream.Count);
        }

        private void Backtrack((int, int) rcd)
        {
            pos = rcd.Item1;
            if (rcd.Item2 != aimLambda.NodeStream.Count)
                aimLambda.NodeStream.RemoveRange(rcd.Item2, aimLambda.NodeStream.Count - rcd.Item2);
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
                        aimLambda.ArgTypes.Add(TypestrToType(type_name));
                        aimLambda.ArgNames.Add(val_name);
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
                        aimLambda.ReturnType = TypestrToType(final_type_name);
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
                && MetaName(out string token_value2))
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
        private IType TypestrToType(string str)
        {
            if (RawType.TypeDict.ContainsKey(str))
            {
                return RawType.TypeDict[str];
            }
            if (engineRoot.DuckTypeDict.ContainsKey(str))
            {
                return engineRoot.DuckTypeDict[str];
            }
            throw new KulaException.KTypeException(str);
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

            if (PInterface()) { return true; }
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
                    aimLambda.NodeStream.Add(new VMNode(flag ? VMNodeType.VAR : VMNodeType.LET, var_name));
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

            // Using Lambda
            if (PFuncBras()) { return true; }
            Backtrack(rcd);

            // PIPE
            if (MetaSymbol("|"))
            {
                // aimFunc.NodeStream.Add(new VMNode(VMNodeType.PIPE, "|"));
                ++pipes;
                return true;
            }
            Backtrack(rcd);

            if (PRightVar()) { return true; }
            Backtrack(rcd);

            if (PRightIndex()) { return true; }
            Backtrack(rcd);

            /*
            if (PRightKey()) { return true; }
            Backtrack(rcd);
            */

            return false;
        }

        /// <summary>
        /// 解析 调用函数的一对括号
        /// </summary>
        private bool PFuncBras()
        {
            var rcd = Record();
            int _pipes = pipes;
            pipes = 0;

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
                    aimLambda.NodeStream.Add(new VMNode(VMNodeType.FUNC, count + (_pipes << 16)));
                    return true;
                }
            }

            Backtrack(rcd);
            pipes = _pipes;
            return false;
        }

        /// <summary>
        /// 常量
        /// </summary>
        private bool PConst()
        {
            int _pos = pos;
            var token = aimLambda.TokenStream[pos++];
            if (token.Type == LexTokenType.NUMBER)
            {
                aimLambda.NodeStream.Add(new VMNode(VMNodeType.VALUE, float.Parse(token.Value)));
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
            var token = aimLambda.TokenStream[pos++];
            if (token.Type == LexTokenType.STRING)
            {
                aimLambda.NodeStream.Add(new VMNode(VMNodeType.STRING, token.Value));
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
            var token = aimLambda.TokenStream[pos++];
            if (token.Type == LexTokenType.NAME)
            {
                aimLambda.NodeStream.Add(new VMNode(VMNodeType.NAME, token.Value));
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

            // .key 语法糖
            if (MetaSymbol(".") && MetaName(out string token_key))
            {
                aimLambda.NodeStream.Add(new VMNode(VMNodeType.STRING, token_key));
                aimLambda.NodeStream.Add(new VMNode(VMNodeType.CONKEY, '.'));
                return true;
            }
            Backtrack(rcd);

            if (MetaSymbol("[") && PValue() && MetaSymbol("]"))
            {
                aimLambda.NodeStream.Add(new VMNode(VMNodeType.CONKEY, '['));
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
                aimLambda.NodeStream.Add(new VMNode(VMNodeType.RETURN, null));
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
                while (aimLambda.TokenStream[count_pos].Type != LexTokenType.SYMBOL || aimLambda.TokenStream[count_pos].Value != "{")
                {
                    ++count_pos;
                }
                while (count_stack > 0)
                {
                    ++count_pos;
                    if (aimLambda.TokenStream[count_pos].Type == LexTokenType.SYMBOL)
                    {
                        if (aimLambda.TokenStream[count_pos].Value == "{") { ++count_stack; }
                        else if (aimLambda.TokenStream[count_pos].Value == "}") { --count_stack; }
                    }
                }

                // 截取Token 写入函数 待编译
                List<LexToken> func_tokens = aimLambda.TokenStream.GetRange(start_pos, count_pos - start_pos + 1);
                var func = new Lambda(func_tokens);

                // 将待使用函数送入编译队列
                lambdaQue.Enqueue(func);

                aimLambda.NodeStream.Add(new VMNode(VMNodeType.LAMBDA, func));
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
                int if_id = aimLambda.NodeStream.Count;
                aimLambda.NodeStream.Add(new VMNode());

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
                            aimLambda.NodeStream[if_id] = new VMNode(VMNodeType.IFGOTO, aimLambda.NodeStream.Count + 1);
                            int else_id = aimLambda.NodeStream.Count;
                            aimLambda.NodeStream.Add(new VMNode());

                            while (PStatement()) ;
                            if (MetaSymbol("}"))
                            {
                                aimLambda.NodeStream[else_id] = new VMNode(VMNodeType.GOTO, aimLambda.NodeStream.Count);
                            }
                        }
                        else
                        {
                            aimLambda.NodeStream[if_id] = new VMNode(VMNodeType.IFGOTO, aimLambda.NodeStream.Count);
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

            int backPos = aimLambda.NodeStream.Count;
            if (MetaKeyword("while") && MetaSymbol("(") && PValue() && MetaSymbol(")"))
            {
                int while_id = aimLambda.NodeStream.Count;
                aimLambda.NodeStream.Add(new VMNode());

                if (MetaSymbol("{"))
                {
                    while (PStatement()) ;
                    if (MetaSymbol("}"))
                    {
                        aimLambda.NodeStream[while_id] = new VMNode(VMNodeType.IFGOTO, aimLambda.NodeStream.Count + 1);
                        aimLambda.NodeStream.Add(new VMNode(VMNodeType.GOTO, backPos));
                        return true;
                    }
                }
            }

            Backtrack(rcd);
            return false;
        }

        /// <summary>
        /// 鸭子接口声明
        /// </summary>
        public bool PInterface()
        {
            var rcd = Record();
            if (MetaKeyword("type"))
            {
                if (MetaName(out string duck_name))
                {
                    if (!MetaSymbol("{"))
                        throw new KulaException.ParserException("{ ?");

                    var node_list = new List<(string, string)>();
                    while (!MetaSymbol("}"))
                    {
                        if (MetaType(out string item_name)
                            && MetaSymbol(":")
                            && MetaName(out string type_name))
                        {
                            node_list.Add((item_name, type_name));
                            if (!MetaSymbol(","))
                            {
                                throw new KulaException.ParserException(", ?");
                            }
                        }

                    }
                    if (MetaSymbol(";"))
                    {
                        // 添加Type 不允许重复
                        engineRoot.DuckTypeDict.Add(duck_name, new DuckType(duck_name, engineRoot, node_list));
                        return true;
                    }
                }
            }

            Backtrack(rcd);
            return false;
        }

        /// <summary>
        /// for 遍历
        /// 格式如：
        ///     for (k, v in )
        /// </summary>
        /// <returns></returns>

        /*private bool PBlockFor()
        {
        }
        */
    }
}
