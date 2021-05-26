using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using kula.Core;
using kula.Core.VMObj;

namespace kula.DataObj
{
    delegate void KvmBuiltinFunc(Stack<object> stack);
    class Func
    {
        private Type[] argTypes;
        private List<LexToken> tokenStream;
        private List<KvmNode> nodeStream;

        private static Dictionary<string, KvmBuiltinFunc> builtinFunc = new Dictionary<string, KvmBuiltinFunc>()
        {
            {"plus", (stack) => {
                    var args = new object[2];
                    for(int i = args.Length - 1; i >= 0; --i) {args[i] = stack.Pop(); }
                    foreach (var arg in args) {if (arg.GetType() != typeof(float)) throw new Exception("Func Runtime ERROR - wrong type"); }
                    stack.Push((float)args[0] + (float)args[1]);
            } },
            {"minus", (stack) => {
                    var args = new object[2];
                    for(int i = args.Length - 1; i >= 0; --i) {args[i] = stack.Pop(); }
                    foreach (var arg in args) {if (arg.GetType() != typeof(float)) throw new Exception("Func Runtime ERROR - wrong type"); }
                    stack.Push((float)args[0] - (float)args[1]);
            } },
            {"times", (stack) => {
                    var args = new object[2];
                    for(int i = args.Length - 1; i >= 0; --i) {args[i] = stack.Pop(); }
                    foreach (var arg in args) {if (arg.GetType() != typeof(float)) throw new Exception("Func Runtime ERROR - wrong type"); }
                    stack.Push((float)args[0] * (float)args[1]);
            } },
            {"div", (stack) => {
                    var args = new object[2];
                    for(int i = args.Length - 1; i >= 0; --i) {args[i] = stack.Pop(); }
                    foreach (var arg in args) {if (arg.GetType() != typeof(float)) throw new Exception("Func Runtime ERROR - wrong type"); }
                    stack.Push((float)args[0] / (float)args[1]);
            } },
            {"println", (stack) => { Console.WriteLine("\t" + stack.Pop()); } },
            {"toStr", (stack) => {
                    stack.Push(stack.Pop().ToString());
            } },
            {"cut", (stack) => {
                    var args = new object[3];
                    for(int i = args.Length - 1; i >= 0; --i) {args[i] = stack.Pop(); }
                    if (args[0].GetType() != typeof(string) || args[1].GetType() != typeof(float) || args[2].GetType() != typeof(float))
                    {
                        throw new Exception("Func Runtime ERROR - wrong type");
                    }
                    stack.Push(((string)args[0]).Substring((int)args[1], (int)args[2]));
            } },
            {"concat", (stack)=> {
                    var args = new object[2];
                    for(int i = args.Length - 1; i >= 0; --i) {args[i] = stack.Pop(); }
                    foreach (var arg in args)
                    {
                        if (arg.GetType() != typeof(string)) { throw new Exception("Func Runtime ERROR - wrong type"); }
                    }
                    stack.Push((string)args[0] + (string)args[1]);
            } },
            {"type", (stack)=> {
                    var arg_type = stack.Pop().GetType();
                    switch (Type.GetTypeCode(arg_type))
                    {
                        case TypeCode.Single:
                            stack.Push("Num");
                            break;
                        case TypeCode.String:
                            stack.Push("Str");
                            break;
                        default:
                            {
                                if (arg_type == typeof(Func)) { stack.Push("Func"); }
                                else { stack.Push("None"); }
                            }
                            break;
                    }
            } },
            {"equal", (stack)=> {
                    stack.Push( object.Equals(stack.Pop(), stack.Pop() ) ? 1f : 0f);                
            } },
            {"greater", (stack)=> {
                    var args = new object[2];
                    for(int i = args.Length - 1; i >= 0; --i) {args[i] = stack.Pop(); }
                    foreach (var arg in args) {if (arg.GetType() != typeof(float)) throw new Exception("Func Runtime ERROR - wrong type"); }
                    stack.Push( ((float)args[0] > (float)args[1]) ? 1f : 0f);
            } },
            {"less", (stack)=> {
                    var args = new object[2];
                    for(int i = args.Length - 1; i >= 0; --i) {args[i] = stack.Pop(); }
                    foreach (var arg in args) {if (arg.GetType() != typeof(float)) throw new Exception("Func Runtime ERROR - wrong type"); }
                    stack.Push( ((float)args[0] < (float)args[1]) ? 1f : 0f);
            } },
            {"and", (stack) => {
                    var args = new object[2];
                    for(int i = args.Length - 1; i >= 0; --i) {args[i] = stack.Pop(); }
                    foreach (var arg in args) {if (arg.GetType() != typeof(float)) throw new Exception("Func Runtime ERROR - wrong type"); }
                    bool flag = ((float)args[0] != 0) && ((float)args[1] != 0);
                    stack.Push(flag ? 1f : 0f);
            } },
            {"or", (stack) => {
                    var args = new object[2];
                    for(int i = args.Length - 1; i >= 0; --i) {args[i] = stack.Pop(); }
                    foreach (var arg in args) {if (arg.GetType() != typeof(float)) throw new Exception("Func Runtime ERROR - wrong type"); }
                    bool flag = ((float)args[0] != 0) || ((float)args[1] != 0);
                    stack.Push(flag ? 1f : 0f);
            } },
            {"not", (stack) => {
                    var arg = stack.Pop();
                    if (arg.GetType() != typeof(float)) { throw new Exception("Func Runtime ERROR - wrong type"); }
                    stack.Push((float)arg == 0f ? 1f : 0f);
            } },
        };
        public static Dictionary<string, KvmBuiltinFunc> BuiltinFunc { get => builtinFunc; }


        public Func(List<LexToken> tokenStream)
        {
            this.tokenStream = tokenStream;
        }

        public override string ToString()
        {
            return "{lambda}";
        }
    }
}
