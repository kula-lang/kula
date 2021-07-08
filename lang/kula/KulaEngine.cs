using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

using Kula.Core;
using Kula.Data;

namespace Kula
{
    /// <summary>
    /// Kula 源引擎
    /// </summary>
    public class KulaEngine
    {
        /// <summary>
        /// 主运行时
        /// </summary>
        private readonly FuncRuntime mainRuntime;

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
        public Dictionary<string, BuiltinFunc> ExtendFunc { get; } = new Dictionary<string, BuiltinFunc>();

        /// <summary>
        /// 构造函数，生成空运行时
        /// </summary>
        public KulaEngine()
        {
            mainRuntime = new FuncRuntime(null, null, this);
        }

        /// <summary>
        /// 编译生成字节码 存储到字节码集合
        /// </summary>
        /// <param name="sourceCode">源代码</param>
        /// <param name="codeID">字节码名称</param>
        /// <param name="isDebug">是否为Debug编译</param>
        public void Compile(string sourceCode, string codeID, bool isDebug = false)
        {
            var tmp1 = Lexer.Instance.Read(sourceCode).Scan();
            if (isDebug) { tmp1.Show(); }
            List<LexToken> lexTokens = tmp1.Out();

            Func main = new Func(lexTokens);
            FuncWithEnv mainEnv = new FuncWithEnv(main, null);

            var tmp2 = Parser.Instance.Parse(main);
            if (isDebug) { tmp2.Show(); }

            byteCodeMap[codeID] = mainEnv;
        }

        /// <summary>
        /// 运行 字节码集合中 已编译的程序
        /// </summary>
        /// <param name="codeId">字节码名称</param>
        /// <param name="isDebug">是否为Debug输出</param>
        public void Run(string codeId, bool isDebug = false)
        {
            if (!isDebug)
            {
                mainRuntime.Read(byteCodeMap[codeId]).Run(null);
            }
            else
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                mainRuntime.Read(byteCodeMap[codeId]).DebugRun();
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
    }
}
