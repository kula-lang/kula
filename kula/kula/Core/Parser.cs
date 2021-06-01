using System;
using System.Collections.Generic;

using kula.Data;
using kula.Util;

namespace kula.Core
{
    class Parser
    {
        private static Parser instance = new Parser();
        public static Parser Instance { get => instance; }

        private Func aimFunc;

        private readonly Stack<string> nameStack = new Stack<string>();
        private int pos;

        private Parser() { }

        public Parser Show()
        {
            Console.WriteLine("Parser ->");
            foreach (var node in aimFunc.NodeStream)
            {
                Console.ForegroundColor = ConsoleUtility.KvmColorDict[node.Type];
                Console.Write("\t");
                Console.WriteLine(node);
            }
            Console.WriteLine();
            Console.ResetColor();
            return this;
        }
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
                catch (IndexOutOfRangeException) { throw new KulaException.ParserException(); }
                catch (ArgumentOutOfRangeException ) { throw new KulaException.ParserException(); }
                catch (Exception e) { throw e; }
            }
            if (pos != aimFunc.TokenStream.Count)
            {
                throw new KulaException.ParserException();
            }
            aimFunc.TokenStream.Clear();
            return this;
        }
        
        public Parser ParseLambda(Func func)
        {
            pos = 0; int _pos = -1;
            this.aimFunc = func;

            /*
            foreach (var token in aimRunnable.TokenStream) 
                Console.WriteLine(token);
            */

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
                        catch (IndexOutOfRangeException) { throw new KulaException.ParserException(); }
                        catch (ArgumentOutOfRangeException) { throw new KulaException.ParserException(); }
                        catch (Exception e) { throw e; }
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
            return this;
        }
        public bool PLambdaHead()
        {
            int _pos = pos; int _size = aimFunc.NodeStream.Count;

            var token1 = aimFunc.TokenStream[pos++];
            var token2 = aimFunc.TokenStream[pos++];
            if (token1.Type == LexTokenType.KEYWORD && token1.Value == "func"
                && token2.Type == LexTokenType.SYMBOL && token2.Value == "("
                )
            {
                return true;
            }

            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);
            return false;
        }
        public bool PLambdaDeclare()
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
                    if (final_type.Type == LexTokenType.TYPE)
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
        public bool PValAndType()
        {
            int _pos = pos; int _size = aimFunc.NodeStream.Count;
            var token1 = aimFunc.TokenStream[pos++];
            var token2 = aimFunc.TokenStream[pos++];
            var token3 = aimFunc.TokenStream[pos++];
            if (token1.Type == LexTokenType.NAME 
                && token2.Type == LexTokenType.SYMBOL && token2.Value == ":"
                && token3.Type == LexTokenType.TYPE
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
            Type final_type_t;
            switch (str)
            {
                case "Num":
                    final_type_t = typeof(float);
                    break;
                case "Str":
                    final_type_t = typeof(string);
                    break;
                case "Func":
                    final_type_t = typeof(Func);
                    break;
                case "Any":
                    final_type_t = typeof(object);
                    break;
                default:
                    final_type_t = null;
                    break;
            }
            return final_type_t;
        }
        private bool PStatement()
        {
            int _pos = pos; int _size = aimFunc.NodeStream.Count;

            if (PSymbol(";")) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PValue() && PSymbol(";")) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PLeftVar() && PSymbol("=") && PValue() && PSymbol(";"))
            {
                string var_name = nameStack.Pop();
                aimFunc.NodeStream.Add(new KvmNode(KvmNodeType.VARIABLE, var_name));
                return true;
            }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PBlockIf()) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PBlockWhile()) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PReturn() && PSymbol(";")) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            return false;
        }
        private bool PValue()
        {
            int _pos = pos; int _size = aimFunc.NodeStream.Count;

            if (PConst()) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PConstString()) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PFuncBody()) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PFunc()) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            if (PRightVar()) { return true; }
            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);

            return false;
        }
        private bool PFuncHead()
        {
            int _pos = pos, _size = aimFunc.NodeStream.Count;
            // 防止溢出
            if (pos + 2 >= aimFunc.TokenStream.Count) { return false; }
            var token1 = aimFunc.TokenStream[pos++]; var token2 = aimFunc.TokenStream[pos++];
            if (token1.Type == LexTokenType.NAME && token2.Type == LexTokenType.SYMBOL && token2.Value == "(")
            {
                nameStack.Push(token1.Value);
                return true;
            }

            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);
            return false;
        }
        private bool PFunc()
        {
            int _pos = pos; int _size = aimFunc.NodeStream.Count;

            if (PFuncHead())
            {
                bool flag = true;
                while (flag)
                {
                    int _tmp_pos = pos; int _tmp_size = aimFunc.NodeStream.Count;
                    if (PValue())
                    {
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
                    string func_name = nameStack.Pop();
                    aimFunc.NodeStream.Add(new KvmNode(KvmNodeType.FUNC, func_name));
                    return true;
                }
            }

            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);
            return false;
        }
        private bool PConst()
        {
            int _pos = pos;
            var token = aimFunc.TokenStream[pos++];
            if (token.Type == LexTokenType.NUMBER)
            {
                aimFunc.NodeStream.Add(new KvmNode(KvmNodeType.VALUE, float.Parse(token.Value)));
                return true;
            }
            pos = _pos; return false;
        }
        private bool PConstString()
        {
            int _pos = pos;
            var token = aimFunc.TokenStream[pos++];
            if (token.Type == LexTokenType.STRING)
            {
                aimFunc.NodeStream.Add(new KvmNode(KvmNodeType.STRING, token.Value));
                return true;
            }
            pos = _pos; return false;
        }
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
        private bool PRightVar()
        {
            int _pos = pos;
            var token = aimFunc.TokenStream[pos++];
            if (token.Type == LexTokenType.NAME)
            {
                aimFunc.NodeStream.Add(new KvmNode(KvmNodeType.NAME, token.Value));
                return true;
            }
            pos = _pos; return false;
        }
        private bool PKeyword(string kword)
        {
            int _pos = pos;
            var token = aimFunc.TokenStream[pos++];
            if (token.Type == LexTokenType.KEYWORD && token.Value == kword)
            {
                return true;
            }
            pos = _pos; return false;
        }
        private bool PReturn()
        {
            int _pos = pos;
            if (PKeyword("return") && PValue())
            {
                aimFunc.NodeStream.Add(new KvmNode(KvmNodeType.RETURN, null));
                return true;
            }
            return false;
        }
        /**
         * foo = func (Num v1, Str v2) { ... } 
         */
        private bool PFuncBody()
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

                aimFunc.NodeStream.Add(new KvmNode(KvmNodeType.VALUE, func));
                pos = count_pos + 1;
                return true;
            }
            pos = _pos;
            return false;
        }
        private bool PBlockIf()
        {
            int _pos = pos; int _size = aimFunc.NodeStream.Count;

            if (PKeyword("if") && PSymbol("(") && PValue() && PSymbol(")"))
            {
                int tmpId = aimFunc.NodeStream.Count;
                aimFunc.NodeStream.Add(null);

                if (PSymbol("{"))
                {
                    while (PStatement()) ;
                    if (PSymbol("}"))
                    {
                        aimFunc.NodeStream[tmpId] = new KvmNode(KvmNodeType.IFGOTO, aimFunc.NodeStream.Count);
                        return true;
                    }
                }
            }

            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);
            return false;
        }
        private bool PBlockWhile()
        {
            int _pos = 0; int _size = aimFunc.NodeStream.Count;

            int backPos = aimFunc.NodeStream.Count;
            if (PKeyword("while") && PSymbol("(") && PValue() && PSymbol(")"))
            {
                int tmpId = aimFunc.NodeStream.Count;
                aimFunc.NodeStream.Add(null);

                if (PSymbol("{"))
                {
                    while (PStatement()) ;
                    if (PSymbol("}"))
                    {
                        aimFunc.NodeStream[tmpId] = new KvmNode(KvmNodeType.IFGOTO, aimFunc.NodeStream.Count + 1);
                        aimFunc.NodeStream.Add(new KvmNode(KvmNodeType.GOTO, backPos));
                        return true;
                    }
                }
            }

            pos = _pos; aimFunc.NodeStream.RemoveRange(_size, aimFunc.NodeStream.Count - _size);
            return false;
        }
    }
}
