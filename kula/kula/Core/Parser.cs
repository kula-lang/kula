using System;
using System.Collections.Generic;
using kula.Core.VMObj;
using kula.DataObj;
using kula.Util;

namespace kula.Core
{
    class Parser
    {
        private static Parser instance = new Parser();
        public static Parser Instance { get => instance; }

        private List<LexToken> tokenStream;
        private List<KvmNode> nodeStream;

        private Stack<string> nameStack;
        private int pos;

        private Parser()
        {
            nameStack = new Stack<string>();
            nodeStream = new List<KvmNode>();
        }

        public List<KvmNode> Out() { return nodeStream; }
        public Parser Read(List<LexToken> tokenStream)
        {
            this.tokenStream = tokenStream;
            return this;
        }
        public Parser Show()
        {
            if (nodeStream == null) { Parse(); }
            Console.WriteLine("Parser ->");
            foreach (var node in nodeStream)
            {
                Console.ForegroundColor = ConsoleUtility.KvmColorDict[node.Type];
                Console.Write("\t");
                Console.WriteLine(node);
            }
            Console.WriteLine();
            Console.ResetColor();
            return this;
        }
        public Parser Parse()
        {
            pos = 0; int _pos = -1;
            nodeStream.Clear();
            nameStack.Clear();
            while (pos < tokenStream.Count && _pos != pos)
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
            if (pos != tokenStream.Count)
            {
                throw new KulaException.ParserException();
            }
            return this;
        }
        private bool PStatement()
        {
            int _pos = pos; int _size = nodeStream.Count;

            if (PSymbol(";")) { return true; }
            pos = _pos; nodeStream.RemoveRange(_size, nodeStream.Count - _size);

            if (PValue() && PSymbol(";")) { return true; }
            pos = _pos; nodeStream.RemoveRange(_size, nodeStream.Count - _size);

            if (PLeftVar() && PSymbol("=") && PValue() && PSymbol(";"))
            {
                string var_name = nameStack.Pop();
                nodeStream.Add(new KvmNode(KvmNodeType.VARIABLE, var_name));
                return true;
            }
            pos = _pos; nodeStream.RemoveRange(_size, nodeStream.Count - _size);

            if (PBlockIf()) { return true; }
            pos = _pos; nodeStream.RemoveRange(_size, nodeStream.Count - _size);

            if (PBlockWhile()) { return true; }
            pos = _pos; nodeStream.RemoveRange(_size, nodeStream.Count - _size);
            return false;
        }
        private bool PValue()
        {
            int _pos = pos; int _size = nodeStream.Count;

            if (PConst()) { return true; }
            pos = _pos; nodeStream.RemoveRange(_size, nodeStream.Count - _size);

            if (PConstString()) { return true; }
            pos = _pos; nodeStream.RemoveRange(_size, nodeStream.Count - _size);

            if (PLambda()) { return true; }
            pos = _pos; nodeStream.RemoveRange(_size, nodeStream.Count - _size);

            if (PFunc()) { return true; }
            pos = _pos; nodeStream.RemoveRange(_size, nodeStream.Count - _size);

            if (PRightVar()) { return true; }
            pos = _pos; nodeStream.RemoveRange(_size, nodeStream.Count - _size);

            return false;
        }
        private bool PFuncHead()
        {
            int _pos = pos, _size = nodeStream.Count;
            // 防止溢出
            if (pos + 2 >= tokenStream.Count) { return false; }
            var token1 = tokenStream[pos++]; var token2 = tokenStream[pos++];
            if (token1.Type == LexTokenType.NAME && token2.Type == LexTokenType.SYMBOL && token2.Value == "(")
            {
                nameStack.Push(token1.Value);
                return true;
            }

            pos = _pos; nodeStream.RemoveRange(_size, nodeStream.Count - _size);
            return false;
        }
        private bool PFunc()
        {
            int _pos = pos; int _size = nodeStream.Count;

            if (PFuncHead())
            {
                bool flag = true;
                while (flag)
                {
                    int _tmp_pos = pos; int _tmp_size = nodeStream.Count;
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
                        nodeStream.RemoveRange(_tmp_size, nodeStream.Count - _tmp_size);
                        flag = false;
                    }
                }

                if (PSymbol(")"))
                {
                    string func_name = nameStack.Pop();
                    nodeStream.Add(new KvmNode(KvmNodeType.FUNC, func_name));
                    return true;
                }
            }

            pos = _pos; nodeStream.RemoveRange(_size, nodeStream.Count - _size);
            return false;
        }
        private bool PConst()
        {
            int _pos = pos;
            var token = tokenStream[pos++];
            if (token.Type == LexTokenType.NUMBER)
            {
                nodeStream.Add(new KvmNode(KvmNodeType.VALUE, float.Parse(token.Value)));
                return true;
            }
            pos = _pos; return false;
        }
        private bool PConstString()
        {
            int _pos = pos;
            var token = tokenStream[pos++];
            if (token.Type == LexTokenType.STRING)
            {
                nodeStream.Add(new KvmNode(KvmNodeType.STRING, token.Value));
                return true;
            }
            pos = _pos; return false;
        }
        private bool PSymbol(string sym)
        {
            int _pos = pos;
            var token = tokenStream[pos++];
            if (token.Type == LexTokenType.SYMBOL && token.Value == sym)
            {
                return true;
            }
            pos = _pos; return false;
        }
        private bool PLeftVar()
        {
            int _pos = pos;
            var token = tokenStream[pos++];
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
            var token = tokenStream[pos++];
            if (token.Type == LexTokenType.NAME)
            {
                nodeStream.Add(new KvmNode(KvmNodeType.NAME, token.Value));
                return true;
            }
            pos = _pos; return false;
        }
        private bool PKeyword(string kword)
        {
            int _pos = pos;
            var token = tokenStream[pos++];
            if (token.Type == LexTokenType.KEYWORD && token.Value == kword)
            {
                return true;
            }
            pos = _pos; return false;
        }
        /**
         * foo = func (Num v1, Str v2) { ... } 
         */
        private bool PLambda()
        {
            int _pos = pos, start_pos = _pos;
            if (PKeyword("func"))
            {
                // 数大括号儿
                int count_stack = 1, count_pos = pos;
                while (tokenStream[count_pos].Type != LexTokenType.SYMBOL || tokenStream[count_pos].Value != "{")
                {
                    ++count_pos;
                }
                while (count_stack > 0)
                {
                    ++count_pos;
                    if (tokenStream[count_pos].Type == LexTokenType.SYMBOL)
                    {
                        if (tokenStream[count_pos].Value == "{") { ++count_stack; }
                        else if (tokenStream[count_pos].Value == "}") { --count_stack; }
                    }
                }
                // 截取Token 写入函数 待编译
                List<LexToken> func_tokens = tokenStream.GetRange(start_pos, count_pos - start_pos + 1);
                var func = new Func(func_tokens);

                nodeStream.Add(new KvmNode(KvmNodeType.VALUE, func));
                pos = count_pos + 1;
                return true;
            }
            pos = _pos;
            return false;
        }
        private bool PBlockIf()
        {
            int _pos = pos; int _size = nodeStream.Count;

            if (PKeyword("if") && PSymbol("(") && PValue() && PSymbol(")"))
            {
                int tmpId = nodeStream.Count;
                nodeStream.Add(null);

                if (PSymbol("{"))
                {
                    while (PStatement()) ;
                    if (PSymbol("}"))
                    {
                        nodeStream[tmpId] = new KvmNode(KvmNodeType.IFGOTO, nodeStream.Count);
                        return true;
                    }
                }
            }

            pos = _pos; nodeStream.RemoveRange(_size, nodeStream.Count - _size);
            return false;
        }
        private bool PBlockWhile()
        {
            int _pos = 0; int _size = nodeStream.Count;

            int backPos = nodeStream.Count;
            if (PKeyword("while") && PSymbol("(") && PValue() && PSymbol(")"))
            {
                int tmpId = nodeStream.Count;
                nodeStream.Add(null);

                if (PSymbol("{"))
                {
                    while (PStatement()) ;
                    if (PSymbol("}"))
                    {
                        nodeStream[tmpId] = new KvmNode(KvmNodeType.IFGOTO, nodeStream.Count + 1);
                        nodeStream.Add(new KvmNode(KvmNodeType.GOTO, backPos));
                        return true;
                    }
                }
            }

            pos = _pos; nodeStream.RemoveRange(_size, nodeStream.Count - _size);
            return false;
        }
    }
}
