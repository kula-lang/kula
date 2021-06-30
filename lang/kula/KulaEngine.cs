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
        private readonly Dictionary<string, FuncEnv> byteCodeMap = new Dictionary<string, FuncEnv>();

        /// <summary>
        /// 构造函数，生成空运行时
        /// </summary>
        public KulaEngine()
        {
            mainRuntime = new FuncRuntime(null, null, this);
        }

        /// <summary>
        /// 编译 生成字节码 存储到字节码集合
        /// </summary>
        /// <param name="sourceCode">源代码</param>
        /// <param name="codeID">字节码名称</param>
        /// <param name="isDebug">是否为Debug编译</param>
        public void Compile(string sourceCode, string codeID, bool isDebug)
        {
            var tmp1 = Lexer.Instance.Read(sourceCode).Scan();
            if (isDebug) { tmp1.Show(); }
            List<LexToken> lexTokens = tmp1.Out();

            Func main = new Func(lexTokens);
            FuncEnv mainEnv = new FuncEnv(main, null);

            var tmp2 = Parser.Instance.Parse(main);
            if (isDebug) { tmp2.Show(); }

            byteCodeMap[codeID] = mainEnv;
        }

        /// <summary>
        /// 运行 字节码集合 中的 字节码
        /// </summary>
        /// <param name="codeId">字节码名称</param>
        /// <param name="isDebug">是否为Debug输出</param>
        public void Run(string codeId, bool isDebug)
        {
            if (!isDebug)
            {
                mainRuntime.Read(byteCodeMap[codeId]).Run(null);
            }
            else
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                mainRuntime.Read(byteCodeMap[codeId]).Run(null);
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
        /// 获取当前 Kula 版本号字符串
        /// </summary>
        public static string Version { get => Kula.Util.KulaVersion.Version.ToString(); }

        // 静态

        /// <summary>
        /// 扩展函数集合
        /// </summary>
        private static readonly Dictionary<string, BuiltinFunc> extendFunc = new Dictionary<string, BuiltinFunc>();

        /// <summary>
        /// 扩展函数集合
        /// </summary>
        public static Dictionary<string, BuiltinFunc> ExtendFunc { get => extendFunc; }
    }
}
