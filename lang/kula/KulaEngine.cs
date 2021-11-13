using Kula.Core;
using Kula.Data;
using Kula.Data.Container;
using Kula.Data.Function;
using Kula.Data.Type;
using System.Collections.Generic;

namespace Kula
{
    /// <summary>
    /// Kula 源引擎
    /// </summary>
    public class KulaEngine
    {
        /// <summary>
        /// 静态配置项
        /// </summary>
        public static class Config
        {
            /// <summary>
            /// 预配置：计时器
            /// </summary>
            public static readonly int STOP_WATCH = 0x1;
            /// <summary>
            /// 预配置：栈内存变量回显
            /// </summary>
            public static readonly int VALUE_STACK = 0x2;
            /// <summary>
            /// 预配置：词法分析
            /// </summary>
            public static readonly int LEXER = 0x4;
            /// <summary>
            /// 预配置：语法分析
            /// </summary>
            public static readonly int PARSER = 0x8;
            /// <summary>
            /// 预配置：REPL自动回显
            /// </summary>
            public static readonly int REPL_ECHO = 0x10;
            /// <summary>
            /// 预配置：类型约束检查
            /// </summary>
            public static readonly int TYPE_CHECK = 0x20;

            /// <summary>
            /// 监测配置项
            /// </summary>
            /// <param name="arg">debug参数</param>
            /// <param name="item">预配置项</param>
            /// <returns></returns>
            public static bool Check(int arg, int item)
            {
                return (arg & item) == item;
            }
        }

        public bool CheckMode(int debugValue) => (debug & debugValue) == debugValue;

        private readonly FuncRuntime mainRuntime;
        private int debug = 0;

        /// <summary>
        /// 获取依赖版本
        /// </summary>
        public static string FrameworkVersion { get => ".NET Standard v2.0"; }

        private readonly Dictionary<string, Func> byteCodeMap = new Dictionary<string, Func>();

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
        public Dictionary<string, SharpFunc> ExtendFunc { get; } = new Dictionary<string, SharpFunc>();

        /// <summary>
        /// 接口集合
        /// </summary>
        public Dictionary<string, DuckType> DuckTypeDict { get; } = new Dictionary<string, DuckType>();

        /// <summary>
        /// 构造函数，生成空运行时
        /// </summary>
        public KulaEngine() { mainRuntime = new FuncRuntime(null, this); }

        /// <summary>
        /// 编译生成字节码 存储到字节码集合
        /// </summary>
        /// <param name="sourceCode">源代码</param>
        /// <param name="codeID">字节码名称</param>
        /// <param name="isDebug">是否为Debug编译</param>
        public void Compile(string sourceCode, string codeID)
        {
            lock (lock_this)
            {
                var lex_tokens = Lexer.Instance.Read(sourceCode).Scan(Config.Check(debug, Config.LEXER)).Out();
                Lambda main = new Lambda(lex_tokens);
                Func mainEnv = new Func(main, null);

                Parser.Instance.Parse(this, main, Config.Check(debug, Config.PARSER));

                byteCodeMap[codeID] = mainEnv;
            }
        }
        private static readonly object lock_this = new object();

        /// <summary>
        /// 运行 字节码集合中 已编译的程序
        /// </summary>
        /// <param name="codeId">字节码名称</param>
        public void Run(string codeId)
        {
            mainRuntime.Root = byteCodeMap[codeId];
            mainRuntime.Run(null, debug);
        }

        /// <summary>
        /// 清空 变量表 和 虚拟机栈
        /// </summary>
        public void Clear() => mainRuntime.Clear();

        /// <summary>
        /// 通过 KulaEngine 调用传出的 Kula 函数
        /// </summary>
        /// <param name="func">Kula函数</param>
        /// <param name="arguments">参数列表</param>
        /// <returns>返回值</returns>
        public object Call(object func, object[] arguments)
        {
            Func fwe = func as Func;
            if (fwe is Func)
            {
                return new FuncRuntime(fwe, this).Run(arguments, 0);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 更新 Debug 属性
        /// </summary>
        /// <param name="debugValue">Debug参数</param>
        public void UpdateMode(int debugValue)
        {
            this.debug ^= debugValue;
        }

        /// <summary>
        /// 设置 Debug 属性
        /// </summary>
        /// <param name="debugValue">Debug参数</param>
        public void SetMode(int debugValue)
        {
            this.debug = debugValue;
        }
    }
}
