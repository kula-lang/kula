using Kula.Core;
using Kula.Data.Container;
using Kula.Data.Function;
using Kula.Data.Type;
using System.Collections.Generic;
using System.IO;

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

        /// <summary>
        /// 检查当前 引擎状态参数
        /// </summary>
        /// <param name="debugValue">待检查项</param>
        /// <returns></returns>
        public bool CheckMode(int debugValue) => (debug & debugValue) == debugValue;

        private readonly FuncRuntime mainRuntime;
        private int debug = 0;

        /// <summary>
        /// 获取依赖版本
        /// </summary>
        public static string FrameworkVersion { get => ".NET Standard v2.0"; }

        private readonly IDictionary<string, Func> byteCodeDict = new Dictionary<string, Func>();
        private readonly ISet<string> moduleIgnoreSet = new HashSet<string>();

        /// <summary>
        /// 引擎数据域
        /// </summary>
        public Map Engine { get; } = new Map();

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
        internal Dictionary<string, DuckType> DuckTypeDict { get; } = new Dictionary<string, DuckType>();

        /// <summary>
        /// 构造函数，生成空运行时
        /// </summary>
        public KulaEngine() 
        {
            mainRuntime = new FuncRuntime(null, this);
            // envRuntime = new FuncRuntime(null, this);
            debug = Config.TYPE_CHECK;
        }

        public void CompileFile(string sourcePath, string codeID)
        {
            lock (lock_this)
            {
                if (!File.Exists(sourcePath))
                {
                    throw new System.Exception($"File Not Found => {sourcePath}");
                }
                moduleIgnoreSet.Add(sourcePath.Replace("\\", "/"));
                string root_path = Directory.GetParent(sourcePath).FullName.Replace("\\", "/");
                using (StreamReader streamReader = 
                    new StreamReader(
                        new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    Module.Instance.SplitModule(streamReader, root_path, out IList<string> module_list);
                    foreach (string module_item_path in module_list)
                    {
                        // System.Console.WriteLine(module_item_path);
                        if (!moduleIgnoreSet.Contains(module_item_path))
                        {
                            moduleIgnoreSet.Add(module_item_path);
                            CompileFile(module_item_path, "$");
                            Run("$");
                            byteCodeDict.Remove("$");
                        }
                    }

                    var lex_tokens = Lexer.Instance.Read(streamReader, Config.Check(debug, Config.LEXER), module_list.Count).Out();
                    Lambda main_lambda = new Lambda(lex_tokens);
                    Func main_func = new Func(main_lambda, null);

                    Parser.Instance.Parse(this, main_lambda, Config.Check(debug, Config.PARSER));

                    byteCodeDict[codeID] = main_func;
                }
            }
        }

        /// <summary>
        /// 编译生成字节码 存储到字节码集合
        /// </summary>
        /// <param name="sourceCode">源代码</param>
        /// <param name="codeID">字节码名称</param>
        /// <param name="isDebug">是否为Debug编译</param>
        public void CompileCode(string sourceCode, string codeID)
        {
            lock (lock_this)
            {
                using (StreamReader streamReader =
                    new StreamReader(
                        new MemoryStream(
                            System.Text.Encoding.Default.GetBytes(sourceCode))))
                {
                    var lex_tokens = Lexer.Instance.Read(streamReader, Config.Check(debug, Config.LEXER), 0).Out();
                    Lambda main_lambda = new Lambda(lex_tokens);
                    Func main_func = new Func(main_lambda, null);

                    Parser.Instance.Parse(this, main_lambda, Config.Check(debug, Config.PARSER));

                    byteCodeDict[codeID] = main_func;
                }
            }
        }
        private static readonly object lock_this = new object();

        /// <summary>
        /// 运行 字节码集合中 已编译的程序
        /// </summary>
        /// <param name="codeId">字节码名称</param>
        public void Run(string codeId)
        {
            mainRuntime.Root = byteCodeDict[codeId];
            mainRuntime.Run(null, debug);
        }

        internal void Inject(KulaEngine anotherEngine)
        {
            mainRuntime.Inject(anotherEngine.mainRuntime);
            foreach (var kv in anotherEngine.ExtendFunc)
            {
                ExtendFunc[kv.Key] = kv.Value;
            }
            foreach (var kv in anotherEngine.DuckTypeDict)
            {
                DuckTypeDict[kv.Key] = kv.Value;
            }
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
            if (func is Func fwe)
                return new FuncRuntime(fwe, this).Run(arguments, 0);
            throw new Xception.FuncUsingException("Wrong Usage of 'Call'");
        }

        /// <summary>
        /// 更新 Debug 属性
        /// </summary>
        /// <param name="debugValue">Debug参数</param>
        public void UpdateMode(bool flag, params int[] debugValue)
        {
            foreach(int i in debugValue)
            {
                if (flag)
                    this.debug |= i;
                else
                    this.debug &= ~i;
            }
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
