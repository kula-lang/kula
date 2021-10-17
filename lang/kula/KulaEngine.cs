using Kula.Core;
using Kula.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;

namespace Kula
{
    /// <summary>
    /// Kula 源引擎
    /// </summary>
    public class KulaEngine
    {

        private readonly FuncRuntime mainRuntime;

        /// <summary>
        /// 获取依赖版本
        /// </summary>
        public string FrameworkVersion => Assembly.GetExecutingAssembly()
                                                  .GetCustomAttributes(true)
                                                  .OfType<TargetFrameworkAttribute>()
                                                  .First().FrameworkName
                                                  .Replace(",Version=", " ");

        /// <summary>
        /// 编译好的 字节码 集合
        /// </summary>
        private readonly Dictionary<string, FuncWithEnv> byteCodeMap = new Dictionary<string, FuncWithEnv>();

        /// <summary>
        /// 引擎数据域
        /// </summary>
        public Map DataMap { get; } = new Map();

        /// <summary>
        /// 获取当前 Kula 版本号字符串
        /// </summary>
        public static string Version { get => Util.KulaVersion.Version.ToString(); }

        /// <summary>
        /// 扩展函数集合
        /// </summary>
        public Dictionary<string, BFunc> ExtendFunc { get; } = new Dictionary<string, BFunc>();

        /// <summary>
        /// 构造函数，生成空运行时
        /// </summary>
        public KulaEngine()
        {
            mainRuntime = new FuncRuntime(null, this);
        }

        /// <summary>
        /// 编译生成字节码 存储到字节码集合
        /// </summary>
        /// <param name="sourceCode">源代码</param>
        /// <param name="codeID">字节码名称</param>
        /// <param name="isDebug">是否为Debug编译</param>
        public void Compile(string sourceCode, string codeID, bool isDebug = false)
        {
            lock (lock_this)
            {
                var lex_tokens = Lexer.Instance.Read(sourceCode).Scan(isDebug).Out();
                Func main = new Func(lex_tokens);
                FuncWithEnv mainEnv = new FuncWithEnv(main, null);

                Parser.Instance.Parse(main, isDebug);

                byteCodeMap[codeID] = mainEnv;
            }
        }
        private static readonly object lock_this = new object();

        /// <summary>
        /// 运行 字节码集合中 已编译的程序
        /// </summary>
        /// <param name="codeId">字节码名称</param>
        /// <param name="isDebug">是否为Debug输出</param>
        public void Run(string codeId, bool isDebug = false)
        {
            if (!isDebug)
            {
                mainRuntime.Root = byteCodeMap[codeId];
                mainRuntime.Run(null);
            }
            else
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                mainRuntime.Root = byteCodeMap[codeId];
                mainRuntime.DebugRun();
                stopwatch.Stop();
                Console.WriteLine("\tIt takes " + stopwatch.Elapsed.Milliseconds + " ms.\n");
            }
        }

        /// <summary>
        /// 清空 变量表 和 虚拟机栈
        /// </summary>
        public void Clear()
        {
            mainRuntime.Clear();
        }

        /// <summary>
        /// 通过 KulaEngine 调用传出的 Kula 函数
        /// </summary>
        /// <param name="func">Kula函数</param>
        /// <param name="arguments">参数列表</param>
        /// <returns></returns>
        public object Call(object func, object[] arguments)
        {
            FuncWithEnv fwe = func as FuncWithEnv;
            if (fwe is Kula.Data.FuncWithEnv)
            {
                return new Kula.Core.FuncRuntime(fwe, this).Run(arguments);
            }
            else
            {
                return null;
            }
        }
    }
}
