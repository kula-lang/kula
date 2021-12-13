using Kula.Data.Function;
using Kula.Data.Type;
using Kula.Xception;
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

        private int lineNumber;

        public Parser DebugShow()
        {
            Console.WriteLine("Parser ->");
            foreach (var node in aimLambda.CodeStream)
            {
                Console.ForegroundColor = ByteCode.KvmColorDict[node.Type];
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

            aimLambda.CodeStream.Clear();
            while (pos < aimLambda.TokenStream.Count && _pos != pos)
            {
                _pos = pos;
                PStatement();
            }
            if (pos != aimLambda.TokenStream.Count)
            {
                if (isDebug) { DebugShow(); }
                throw new ParserException("Unknown Error in Main", lineNumber);
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

            func.CodeStream.Clear();
            if (PLambdaDeclare())
            {
                if (MSymbol("{"))
                {
                    while (pos < aimLambda.TokenStream.Count - 1 && _pos != pos)
                    {
                        _pos = pos;
                        PStatement();
                    }
                    if (pos == aimLambda.TokenStream.Count - 1 && MSymbol("}"))
                    {
                        aimLambda.TokenStream.Clear();
                        return this;
                    }
                }
            }
            aimLambda.CodeStream.Clear();
            aimLambda.TokenStream.Clear();
            throw new ParserException("Unknown Error in lambda", lineNumber);
        }

        /*
         *  以下部分涉及到一个硬核的手写递归下降语法分析器
         *  经过反复设计和重写，为了保证代码的可读，设计规范如下
         *  Meta 系列函数：  元分析，只单独分析单个Token，显式操作TokenSteam。必要时返回对应类型的值，做回溯处理。
         *  Parse 系列函数： 结构式分析，只调用Meta或递归，不显式操作TokenSteam，做回溯处理。
         */

        private bool MKeyword(string kword)
        {
            var rcd = Record();
            // 过长
            if (pos >= aimLambda.TokenStream.Count) return false;
            var token = aimLambda.TokenStream[pos++];
            if (token.Type == LexTokenType.NAME && token.Value == kword)
            {
                lineNumber = token.LineNum;
                return true;
            }
            Backtrack(rcd);
            return false;
        }

        private bool MSymbol(string sym)
        {
            var rcd = Record();
            if (pos >= aimLambda.TokenStream.Count) return false;
            var token = aimLambda.TokenStream[pos++];
            if (token.Type == LexTokenType.SYMBOL && token.Value == sym)
            {
                lineNumber = Math.Max(token.LineNum, lineNumber);
                return true;
            }
            Backtrack(rcd);
            return false;
        }

        private bool MName(out string name)
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
                lineNumber = Math.Max(token.LineNum, lineNumber);
                return true;
            }
            name = null;
            Backtrack(rcd);
            return false;
        }

        private bool MConst()
        {
            int _pos = pos;
            if (pos >= aimLambda.TokenStream.Count)
                return false;
            var token = aimLambda.TokenStream[pos++];
            if (token.Type == LexTokenType.NUMBER)
            {
                aimLambda.CodeStream.Add(new ByteCode(ByteCodeType.VALUE, float.Parse(token.Value)));
                lineNumber = Math.Max(token.LineNum, lineNumber);
                return true;
            }
            pos = _pos; return false;
        }

        private bool MConstString()
        {
            int _pos = pos;
            if (pos >= aimLambda.TokenStream.Count)
                return false;
            var token = aimLambda.TokenStream[pos++];
            if (token.Type == LexTokenType.STRING)
            {
                aimLambda.CodeStream.Add(new ByteCode(ByteCodeType.STRING, token.Value));
                lineNumber = Math.Max(token.LineNum, lineNumber);
                return true;
            }
            pos = _pos; return false;
        }

        /**
         *  回溯节点
         */

        private (int, int) Record()
        {
            return (pos, aimLambda.CodeStream.Count);
        }

        private void Backtrack((int, int) rcd)
        {
            pos = rcd.Item1;
            if (rcd.Item2 != aimLambda.CodeStream.Count)
            {
                aimLambda.CodeStream.RemoveRange(rcd.Item2, aimLambda.CodeStream.Count - rcd.Item2);
            }
        }

        /**
         * 这个往下都是正常的递归下降分析
         */

        private bool PType(out IType type)
        {
            var rcd = Record();
            if (MName(out string word_type_name) && !MSymbol("<"))
            {
                type = TypestrToType(word_type_name);
                return true;
            }
            Backtrack(rcd);

            if (PFuncTypeExp(out IType func_type))
            {
                type = func_type;
                return true;
            }
            type = null;
            Backtrack(rcd);
            return false;
        }

        private bool PFuncTypeExp(out IType funcType)
        {
            var rcd = Record();
            if (MKeyword("Func") && MSymbol("<"))
            {
                List<IType> func_type_list = new List<IType>();
                while (!MSymbol(">"))
                {
                    if (PType(out IType item_type))
                    {
                        func_type_list.Add(item_type);
                    }
                    else
                    {
                        throw new KTypeException("What Happen in FuncTypeExpression?", "ItemType");
                    }

                    if (!MSymbol(","))
                    {
                        throw new ParserException("Missing ',' ", lineNumber);
                    }
                }
                if (MSymbol(":") && PType(out IType return_type))
                {
                    funcType = new FuncType(return_type, func_type_list);
                    return true;
                }
                else
                {
                    throw new KTypeException("FuncTypeExpression has no Return Type", "ReturnType");
                }
            }
            funcType = null;
            Backtrack(rcd);
            return false;
        }


        /// <summary>
        /// 匿名函数 声明区
        /// </summary>
        private bool PLambdaDeclare()
        {
            var rcd = Record();

            if (MKeyword("func") && MSymbol("("))
            {
                while (!MSymbol(")"))
                {
                    if (PValAndType(out string val_name, out string type_name))
                    {
                        aimLambda.ArgList.Add((val_name, TypestrToType(type_name)));
                        MSymbol(",");
                    }
                    else
                    {
                        break;
                    }
                }
                if (MSymbol(":"))
                {
                    if (PType(out IType final_type))
                    {
                        // 记录类型
                        aimLambda.ReturnType = final_type;
                        return true;
                    }
                }
                else
                {
                    throw new ParserException("Missing ReturnType in Function Declaration.", lineNumber);
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
            if (MName(out string token_value1)
                && MSymbol(":")
                && MName(out string token_value2)
                && !MSymbol("<"))
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
            throw new KTypeException(str);
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

            if (MSymbol(";")) { return true; }
            Backtrack(rcd);

            if (PDuckType()) { return true; }
            Backtrack(rcd);

            if (PReturn() && MSymbol(";")) { return true; }
            Backtrack(rcd);

            if (PValue() && MSymbol(";")) { return true; }
            Backtrack(rcd);

            if (PLeftVar(out string var_name))
            {
                bool flag = MSymbol(":");
                if (MSymbol("=") && PValue() && MSymbol(";"))
                {
                    aimLambda.CodeStream.Add(new ByteCode(flag ? ByteCodeType.VAR : ByteCodeType.SET, var_name));
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

            if (MConst()) { return true; }
            Backtrack(rcd);

            if (MConstString()) { return true; }
            Backtrack(rcd);

            if (PLambdaBody()) { return true; }
            Backtrack(rcd);

            // Using Lambda
            if (PFuncBras()) { return true; }
            Backtrack(rcd);

            // PIPE
            if (MSymbol("|"))
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

            if (MSymbol("("))
            {
                int count = 0;
                bool flag = true;
                while (flag)
                {
                    var tmp_rcd = Record();
                    if (PValue())
                    {
                        ++count;
                        if (!MSymbol(","))
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

                if (MSymbol(")"))
                {
                    aimLambda.CodeStream.Add(new ByteCode(ByteCodeType.FUNC, count + (_pipes << 16)));
                    return true;
                }
                else
                {
                    throw new ParserException("Missing '(' in usage of Function.", lineNumber);
                }
            }

            Backtrack(rcd);
            pipes = _pipes;
            return false;
        }

        
        /// <summary>
        /// 左值 
        /// 等待变量接收到本地表内
        /// </summary>
        private bool PLeftVar(out string name)
        {
            var rcd = Record();
            if (MName(out string token_value))
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
            if (pos >= aimLambda.TokenStream.Count)
                return false;
            var token = aimLambda.TokenStream[pos++];
            if (token.Type == LexTokenType.NAME)
            {
                aimLambda.CodeStream.Add(new ByteCode(ByteCodeType.NAME, token.Value));
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
            if (MSymbol(".") && MName(out string token_key))
            {
                aimLambda.CodeStream.Add(new ByteCode(ByteCodeType.STRING, token_key));
                aimLambda.CodeStream.Add(new ByteCode(ByteCodeType.CONKEY, '.'));
                return true;
            }
            Backtrack(rcd);

            if (MSymbol("[") && PValue() && MSymbol("]"))
            {
                aimLambda.CodeStream.Add(new ByteCode(ByteCodeType.CONKEY, '['));
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
            if (MKeyword("return") && PValue())
            {
                aimLambda.CodeStream.Add(new ByteCode(ByteCodeType.RETURN, null));
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
            if (MKeyword("func"))
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
                Lambda func = new Lambda(func_tokens);

                // 将待使用函数送入编译队列
                lambdaQue.Enqueue(func);

                aimLambda.CodeStream.Add(new ByteCode(ByteCodeType.LAMBDA, func));
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

            if (MKeyword("if") && MSymbol("(") && PValue() && MSymbol(")"))
            {
                // 保存当前位置新建节点，用于稍后填充 IFGOTO
                // 注意：VMNode 是 struct，没有GC负担，且需要 空new
                int if_id = aimLambda.CodeStream.Count;
                aimLambda.CodeStream.Add(new ByteCode());

                if (MSymbol("{"))
                {
                    while (PStatement()) ;
                    if (MSymbol("}"))
                    {
                        // 到这里已经完成了 IF 语法的解析
                        // 补充一个 ELSE 实现

                        var else_rcd = Record();
                        if (MKeyword("else") && MSymbol("{"))
                        {
                            aimLambda.CodeStream[if_id] = new ByteCode(ByteCodeType.IFGOTO, aimLambda.CodeStream.Count + 1);
                            int else_id = aimLambda.CodeStream.Count;
                            aimLambda.CodeStream.Add(new ByteCode());

                            while (PStatement()) ;
                            if (MSymbol("}"))
                            {
                                aimLambda.CodeStream[else_id] = new ByteCode(ByteCodeType.GOTO, aimLambda.CodeStream.Count);
                            }
                        }
                        else
                        {
                            aimLambda.CodeStream[if_id] = new ByteCode(ByteCodeType.IFGOTO, aimLambda.CodeStream.Count);
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

            int backPos = aimLambda.CodeStream.Count;
            if (MKeyword("while") && MSymbol("(") && PValue() && MSymbol(")"))
            {
                int while_id = aimLambda.CodeStream.Count;
                aimLambda.CodeStream.Add(new ByteCode());

                if (MSymbol("{"))
                {
                    while (PStatement()) ;
                    if (MSymbol("}"))
                    {
                        aimLambda.CodeStream[while_id] = new ByteCode(ByteCodeType.IFGOTO, aimLambda.CodeStream.Count + 1);
                        aimLambda.CodeStream.Add(new ByteCode(ByteCodeType.GOTO, backPos));
                        return true;
                    }
                    else
                    {
                        throw new ParserException("Missing '}' end of while-block.", lineNumber);
                    }
                }
                else
                {
                    throw new ParserException("Missing '{' in while-block.", lineNumber);
                }
            }

            Backtrack(rcd);
            return false;
        }

        /// <summary>
        /// 鸭子接口声明
        /// </summary>
        public bool PDuckType()
        {
            var rcd = Record();
            if (MKeyword("type"))
            {
                if (MName(out string duck_name))
                {
                    if (!MSymbol("{"))
                        throw new ParserException("Missing '{' in Type Declaration.", lineNumber);

                    var node_list = new List<(string, IType)>();
                    while (!MSymbol("}"))
                    {
                        if (MName(out string item_name)
                            && MSymbol(":")
                            && PType(out IType item_type))
                        {
                            node_list.Add((item_name, item_type));
                            if (!MSymbol(","))
                            {
                                throw new ParserException("Missing ',' in Type Declaration.", lineNumber);
                            }
                        }

                    }
                    if (MSymbol(";"))
                    {
                        // 添加Type 不允许重复
                        engineRoot.DuckTypeDict.Add(duck_name, new DuckType(duck_name, engineRoot, node_list));
                        return true;
                    }
                    else
                    {
                        throw new ParserException("Missing ';' after Type Declaration.", lineNumber);
                    }
                }
                else
                {
                    throw new ParserException("Missing TypeName after keyword 'type'.", lineNumber);
                }
            }

            Backtrack(rcd);
            return false;
        }

        /// <summary>
        /// for 遍历
        /// 格式如：
        ///     for (k, v in container)
        /// </summary>
        /// <returns></returns>

        /*private bool PBlockFor()
        {
        }
        */
    }
}
