using System;
using System.Collections.Generic;

using Kula.Data;
using Kula.Util;

namespace Kula.Core
{
    class FuncRuntime
    {
        private readonly Dictionary<string, object> varDict;

        private readonly KulaEngine engine;
        private readonly Stack<object> envStack;

        public FuncWithEnv Root { private get; set; }

        public FuncRuntime(FuncWithEnv root, KulaEngine engine)
        {
            this.Root = root;
            this.engine = engine;

            this.envStack = new Stack<object>();
            this.varDict = new Dictionary<string, object>();
        }

        public void Clear()
        {
            this.varDict.Clear();
            this.envStack.Clear();
        }

        /// <summary>
        /// 函数调用
        /// Release 模式运行
        /// </summary>
        /// <param name="arguments">函数调用的参数列表</param>
        public object Run(object[] arguments)
        {
            if ((arguments == null ? 0 : arguments.Length) != Root.Func.ArgNames.Count)
            {
                throw new KulaException.FuncArgumentException(Root.Func.ArgTypes.ToArray());
            }
            for (int i = Root.Func.ArgNames.Count - 1; i >= 0 && arguments != null; --i)
            {
                object arg = arguments[i];
                if (Root.Func.ArgTypes[i] != typeof(object) && arg.GetType() != Root.Func.ArgTypes[i])
                {
                    throw new KulaException.ArgsTypeException(arg.GetType().Name, Root.Func.ArgTypes[i].Name);
                }
                else
                {
                    varDict.Add(Root.Func.ArgNames[i], arg);
                }
            }

            envStack.Clear();

            // 本函数返回值
            object @return = null;
            // 管道操作计数器
            int pipe_counter = 0;

            for (int i = 0; i < Root.Func.NodeStream.Count && @return == null; ++i)
            {
                var node = Root.Func.NodeStream[i];
                try
                {
                    switch (node.Type)
                    {
                        // 取值
                        case VMNodeType.VALUE:
                            {
                                envStack.Push(node.Value);
                            }
                            break;

                        // 取值-字符串
                        case VMNodeType.STRING:
                            {
                                string val = (string)node.Value;
                                val = System.Text.RegularExpressions.Regex.Unescape(val);
                                envStack.Push(val);
                            }
                            break;

                        // 取值-匿名函数
                        case VMNodeType.LAMBDA:
                            {
                                object value = node.Value;
                                envStack.Push(new FuncWithEnv((Func)value, this));
                            }
                            break;

                        // 赋值
                        case VMNodeType.LET:
                            {
                                bool flag = false;
                                FuncRuntime now_env = this;

                                while (flag == false && now_env != null)
                                {
                                    if (now_env.varDict.ContainsKey((string)node.Value))
                                    {
                                        now_env.varDict[(string)node.Value] = envStack.Pop();
                                        envStack.Clear();
                                        flag = true;
                                    }
                                    now_env = now_env.Root.Runtime;
                                }
                                if (!flag)
                                {
                                    varDict[(string)node.Value] = envStack.Pop();
                                    envStack.Clear();
                                }
                            }
                            break;

                        // 声明赋值 （原地
                        case VMNodeType.VAR:
                            {
                                this.varDict[(string)node.Value] = envStack.Pop();
                                envStack.Clear();
                            }
                            break;

                        // 按名寻址
                        case VMNodeType.NAME:
                            {
                                bool flag = false;
                                FuncRuntime now_env = this;

                                string node_value = node.Value as string;

                                // 常量 查询
                                if (BuiltinFunc.BVals.ContainsKey(node_value))
                                {
                                    envStack.Push(BuiltinFunc.BVals[node_value](engine));
                                    flag = true;
                                }
                                // 扩展函数 覆盖 内置函数
                                else if (engine.ExtendFunc.ContainsKey(node_value))
                                {
                                    flag = true;
                                    envStack.Push(engine.ExtendFunc[node_value]);
                                }
                                // 内置函数 查询
                                else if (BuiltinFunc.BFuncs.ContainsKey(node_value))
                                {
                                    flag = true;
                                    envStack.Push(BuiltinFunc.BFuncs[node_value]);
                                }
                                // 链式查询
                                while (flag == false && now_env != null)
                                {
                                    if (now_env.varDict.ContainsKey(node_value))
                                    {
                                        envStack.Push(now_env.varDict[node_value]);
                                        flag = true;
                                    }
                                    now_env = now_env.Root.Runtime;
                                }
                                if (!flag)
                                {
                                    throw new KulaException.VariableException(node_value);
                                }
                            }
                            break;

                        // 寻找函数
                        case VMNodeType.FUNC:
                            {
                                // 按 参数个数 获取 参数值
                                // 管道操作下，改变参数个数
                                object[] args = new object[(int)node.Value + pipe_counter];
                                for (int k = args.Length - 1; k >= pipe_counter; --k)
                                {
                                    args[k] = envStack.Pop();
                                }
                                object func = envStack.Pop();
                                // 管道操作下的剩余参数
                                while (pipe_counter > 0)
                                {
                                    args[pipe_counter - 1] = envStack.Pop();
                                    --pipe_counter;
                                }

                                // 函数正常调用
                                if (func is BFunc builtin_func)
                                {
                                    var tmp_return = builtin_func(args, engine);
                                    if (tmp_return != null) { envStack.Push(tmp_return); }
                                }
                                else if (func is FuncWithEnv func_wth_env)
                                {
                                    var tmp_return = new FuncRuntime(func_wth_env, engine).Run(args);
                                    if (tmp_return != null) { envStack.Push(tmp_return); }
                                }
                                else
                                {
                                    throw new KulaException.FuncUsingException(func.ToString());
                                }
                            }
                            break;

                        // 条件跳转：0
                        case VMNodeType.IFGOTO:
                            {
                                float arg = (float)envStack.Pop();
                                if (arg == 0)
                                {
                                    i = (int)node.Value - 1;
                                }
                            }
                            break;

                        // 无条件跳转
                        case VMNodeType.GOTO:
                            {
                                i = (int)node.Value - 1;
                            }
                            break;

                        // 返回值
                        case VMNodeType.RETURN:
                            {
                                object return_val = envStack.Pop();
                                if (return_val == null) { break; }
                                if (Root.Func.ReturnType != typeof(object) && return_val.GetType() != Root.Func.ReturnType)
                                {
                                    throw new KulaException.ReturnValueException(return_val.GetType().Name, Root.Func.ReturnType.Name);
                                }
                                @return = return_val;
                            }
                            break;

                        // 索引
                        case VMNodeType.CONKEY:
                            {
                                object vector_key = envStack.Pop();
                                object vector = envStack.Pop();

                                // 数组索引处理
                                if ((char)node.Value == '[')
                                {
                                    if (!(vector_key is float) || !(vector is Data.Array))
                                    {
                                        throw new KulaException.ArrayTypeException();
                                    }
                                    envStack.Push(
                                        ((Data.Array)vector).Data
                                            [(int)(float)vector_key]
                                    );
                                }

                                // 表索引处理
                                else if ((char)node.Value == '<')
                                {
                                    if (!(vector_key is string) || !(vector is Data.Map))
                                    {
                                        throw new KulaException.MapTypeException();
                                    }
                                    envStack.Push(
                                        ((Data.Map)vector).Data[(string)vector_key]
                                    );
                                }
                            }
                            break;
                        case VMNodeType.PIPE:
                            ++pipe_counter;
                            break;
                    }
                }
                catch (InvalidOperationException)
                {
                    throw new KulaException.VMUnderflowException();
                }
            }
            if (@return == null && Root.Func.ReturnType != null)
            {
                throw new KulaException.ReturnValueException("None", Root.Func.ReturnType.Name);
            }
            return @return;
        }

        /// <summary>
        /// Debug 模式运行
        /// </summary>
        public void DebugRun()
        {
            Console.WriteLine("Output ->");
            Run(null);
            Show();
        }

        /// <summary>
        /// Debug 模式下，输出实时编译信息
        /// </summary>
        private void Show()
        {
            int cnt = 0;
            Console.WriteLine("\nVM ->");
            while (envStack.Count > 0)
            {
                object tmp = envStack.Pop();
                if (tmp == null)
                {
                    tmp = "null";
                }
                else if (tmp.GetType() == typeof(string))
                {
                    tmp = "\"" + tmp + "\"";
                }
                Console.WriteLine("\tVM-Stack {" + cnt++ + "} : " + tmp);
            }
            Console.WriteLine();
            Console.ResetColor();
        }
    }
}
