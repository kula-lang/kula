using Kula.Data.Function;
using Kula.Data.Type;
using Kula.Xception;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kula.Core
{
    class FuncRuntime
    {
        private readonly Dictionary<string, object> varDict;

        private readonly KulaEngine engine;
        private readonly Stack<object> envStack;

        public Func Root { get; set; }

        public FuncRuntime(Func root, KulaEngine engine)
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

        private object RunSelf(object[] arguments)
        {
            // 参数数目不等
            if ((arguments == null ? 0 : arguments.Length) != Root.Lambda.ArgList.Count)
            {
                // 校验类型
                if (engine.CheckMode(KulaEngine.Config.TYPE_CHECK))
                {
                    IType[] error_types = new IType[Root.Lambda.ArgList.Count];
                    for (int i = 0; i < error_types.Length; ++i)
                    {
                        error_types[i] = Root.Lambda.ArgList[i].Item2;
                    }
                    throw new FuncArgumentException("Func-Runtime", error_types, Root.Lambda.ArgList.Count);
                }
                // 无需校验类型，只检查参数个数
                else
                {
                    throw new FuncArgumentException("Func-Runtime-Without-TypeCheck", new IType[0], Root.Lambda.ArgList.Count);
                }
            }

            for (int i = Root.Lambda.ArgList.Count - 1; i >= 0 && arguments != null; --i)
            {
                object arg = arguments[i];
                // 类型校验
                if (engine.CheckMode(KulaEngine.Config.TYPE_CHECK)
                    && Root.Lambda.ArgList[i].Item2 != RawType.Any
                    && !Root.Lambda.ArgList[i].Item2.Check(arg))
                {
                    throw new ArgsTypeException(arg.GetType().Name, Root.Lambda.ArgList[i].ToString());
                }
                else
                {
                    varDict[Root.Lambda.ArgList[i].Item1] = arg;
                }
            }

            // envStack.Clear();

            // 返回值
            object @return = null;

            for (int i = 0; i < Root.Lambda.CodeStream.Count && @return == null; ++i)
            {
                var node = Root.Lambda.CodeStream[i];
                switch (node.Type)
                {
                    // 取值
                    case ByteCodeType.VALUE:
                        {
                            envStack.Push(node.Value);
                        }
                        break;

                    // 取值-字符串
                    case ByteCodeType.STRING:
                        {
                            string val = (string)node.Value;
                            val = System.Text.RegularExpressions.Regex.Unescape(val);
                            envStack.Push(val);
                        }
                        break;

                    // 取值-匿名函数
                    case ByteCodeType.LAMBDA:
                        {
                            object value = node.Value;
                            envStack.Push(new Func((Lambda)value, this));
                        }
                        break;

                    // 赋值
                    case ByteCodeType.SET:
                        {
                            bool flag = false;
                            FuncRuntime now_env = this;

                            while (flag == false && now_env != null)
                            {
                                if (now_env.varDict.ContainsKey((string)node.Value))
                                {
                                    if (envStack.Count == 0)
                                        throw new VMUnderflowException($"when assign to {(string)node.Value}");
                                    now_env.varDict[(string)node.Value] = envStack.Pop();
                                    envStack.Clear();
                                    flag = true;
                                }
                                now_env = now_env.Root.Runtime;
                            }
                            if (!flag)
                            {
                                if (envStack.Count == 0)
                                    throw new VMUnderflowException($"when declare {(string)node.Value}");
                                varDict[(string)node.Value] = envStack.Pop();
                                envStack.Clear();
                            }
                        }
                        break;

                    // 声明赋值 （原地
                    case ByteCodeType.VAR:
                        {
                            if (envStack.Count == 0)
                                throw new VMUnderflowException($"when explicitly declare {(string)node.Value}");
                            this.varDict[(string)node.Value] = envStack.Pop();
                            envStack.Clear();
                        }
                        break;

                    // 按名寻址
                    case ByteCodeType.NAME:
                        {
                            bool flag = false;
                            FuncRuntime now_env = this;

                            string node_value = node.Value as string;

                            // 常量 查询
                            if (SharpFunc.SharpVals.ContainsKey(node_value))
                            {
                                envStack.Push(SharpFunc.SharpVals[node_value](engine));
                                flag = true;
                            }
                            // 扩展函数 覆盖 内置函数
                            else if (engine.ExtendFunc.ContainsKey(node_value))
                            {
                                flag = true;
                                envStack.Push(engine.ExtendFunc[node_value]);
                            }
                            // 内置函数 查询
                            else if (SharpFunc.SharpFuncs.ContainsKey(node_value))
                            {
                                flag = true;
                                envStack.Push(SharpFunc.SharpFuncs[node_value]);
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
                                throw new VariableException(node_value);
                            }
                        }
                        break;

                    // 寻找函数
                    case ByteCodeType.FUNC:
                        {
                            // 按 参数个数 获取 参数值
                            // 管道操作下，改变参数个数
                            int node_value = (int)node.Value;

                            int func_args_count = node_value & 0xffff;
                            int func_pipes_count = node_value >> 16;

                            object[] args = new object[func_args_count + func_pipes_count];
                            for (int k = func_args_count + func_pipes_count - 1; k >= func_pipes_count; --k)
                            {
                                if (envStack.Count == 0)
                                    throw new VMUnderflowException("Wrong usage of Arguments");
                                args[k] = envStack.Pop();
                            }
                            if (envStack.Count == 0)
                                throw new VMUnderflowException("Wrong usage of Func");
                            object func = envStack.Pop();

                            // 管道操作下的剩余参数
                            while (func_pipes_count > 0)
                            {
                                if (envStack.Count == 0)
                                    throw new VMUnderflowException("Wrong usage of Pipe Arguments");
                                args[func_pipes_count - 1] = envStack.Pop();
                                --func_pipes_count;
                            }

                            // 函数正常调用
                            if (func is Func func_wth_env)
                            {
                                var tmp_return = new FuncRuntime(func_wth_env, engine).Run(args, 0);
                                if (tmp_return != null) { envStack.Push(tmp_return); }
                            }
                            else if (func is SharpFunc builtin_func)
                            {
                                var tmp_return = builtin_func.Run(args, engine);
                                if (tmp_return != null) { envStack.Push(tmp_return); }
                            }
                            else
                            {
                                throw new FuncUsingException(func.ToString());
                            }
                        }
                        break;

                    // 条件跳转：0
                    case ByteCodeType.IFGOTO:
                        {
                            if (envStack.Count == 0)
                                throw new VMUnderflowException("Wrong usage of IF");
                            float arg = (float)envStack.Pop();
                            if (arg == 0)
                            {
                                i = (int)node.Value - 1;
                            }
                        }
                        break;

                    // 无条件跳转
                    case ByteCodeType.GOTO:
                        {
                            i = (int)node.Value - 1;
                        }
                        break;

                    // 返回值
                    case ByteCodeType.RETURN:
                        {
                            if (envStack.Count == 0)
                                throw new VMUnderflowException("No return value");
                            object return_val = envStack.Pop();
                            if (return_val == null) { break; }
                            if (engine.CheckMode(KulaEngine.Config.TYPE_CHECK)
                                && Root.Lambda.ReturnType != RawType.Any
                                && !Root.Lambda.ReturnType.Check(return_val))
                            {
                                throw new ReturnValueException(return_val.ToString(), Root.Lambda.ReturnType.ToString());
                            }
                            @return = return_val;
                        }
                        break;

                    // 索引
                    case ByteCodeType.CONKEY:
                        {
                            if (envStack.Count == 0)
                                throw new VMUnderflowException("Wrong usage of operator[]");
                            object vk = envStack.Pop();
                            object v = envStack.Pop();

                            // Map语法 索引处理
                            if ((char)node.Value == '.')
                            {
                                if (!(vk is string str_vector_key) || !(v is Data.Container.Map vector_map))
                                {
                                    throw new MapTypeException(vk.ToString());
                                }
                                envStack.Push(vector_map.Data[str_vector_key]);
                            }
                            // 标准索引处理
                            else if ((char)node.Value == '[')
                            {
                                if (v is Data.Container.Array v_array)
                                {
                                    if (vk is float vk_num)
                                    {
                                        int vector_array_size = v_array.Data.Length;
                                        int vk_pos = (int)vk_num;
                                        if (vk_pos >= 0 && vk_pos < vector_array_size)
                                        {
                                            envStack.Push(v_array.Data[vk_pos]);
                                        }
                                        else
                                        {
                                            throw new ContainerException(vk_pos, vector_array_size);
                                        }
                                    }
                                    else
                                    {
                                        throw new ArrayTypeException();
                                    }
                                }
                                else if (v is Data.Container.Map v_map)
                                {
                                    if (vk is string vk_str)
                                    {
                                        if (v_map.Data.ContainsKey(vk_str))
                                        {
                                            envStack.Push(v_map.Data[vk_str]);
                                        }
                                        else
                                        {
                                            throw new ContainerException(vk_str);
                                        }
                                    }
                                    else
                                    {
                                        throw new MapTypeException(vk.ToString());
                                    }
                                }
                                else
                                {
                                    throw new KTypeException(v.GetType().Name);
                                }
                            }
                        }
                        break;
                }
            }
            if (@return == null && Root.Lambda.ReturnType != RawType.None)
            {
                throw new ReturnValueException("None", Root.Lambda.ReturnType.ToString());
            }
            return @return;
        }

        /// <summary>
        /// 运行
        /// </summary>
        public object Run(object[] arguments, FuncRuntime reinject, int debug)
        {
            object @return;
            if (KulaEngine.Config.Check(debug, KulaEngine.Config.STOP_WATCH))
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                @return = RunSelf(arguments);
                stopwatch.Stop();
                Console.WriteLine("Timer ->\n\t" + stopwatch.Elapsed.Milliseconds + " ms.");
            }
            else
            {
                @return = RunSelf(arguments);
            }
            if (KulaEngine.Config.Check(debug, KulaEngine.Config.REPL_ECHO))
            {
                if (envStack.Count > 0)
                {
                    Console.WriteLine(envStack.Peek());
                }
            }
            if (KulaEngine.Config.Check(debug, KulaEngine.Config.VALUE_STACK))
            {
                Show();
            }

            if (reinject != null)
                reinject.Inject(this);

            return @return;
        }

        public object Run(object[] arguments, int debug)
        {
            return Run(arguments, null, debug);
        }

        private void Show()
        {
            int cnt = 0;
            Console.WriteLine("VM ->");
            while (envStack.Count > 0)
            {
                object tmp = envStack.Pop();
                if (tmp == null)
                {
                    tmp = "null";
                }
                else if (RawType.Str.Check(tmp))
                {
                    tmp = "\"" + tmp + "\"";
                }
                Console.WriteLine("\tVM-Stack {" + cnt++ + "} : " + tmp);
            }
            Console.ResetColor();
        }

        public void Inject(FuncRuntime anotherRuntime)
        {
            foreach (var kv in anotherRuntime.varDict)
            {
                this.varDict[kv.Key] = kv.Value;
            }
        }
    }
}
