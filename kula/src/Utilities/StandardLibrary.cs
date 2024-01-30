using Kula.ASTInterpreter.Runtime;
using Kula.BytecodeInterpreter.Runtime;

namespace Kula.Utilities;

public static class StandardLibrary
{
    private static readonly DateTime start = DateTime.UtcNow;
    public static readonly Dictionary<string, NativeFunction> global_functions = new() {
        {"clock", new NativeFunction(0, (_, args) => (DateTime.UtcNow - start).TotalMilliseconds / 1000.0)},
        {"String", new NativeFunction(1, (_, args) => Stringify(args[0]))},
        {"Bool", new NativeFunction(1, (_, args) => Booleanify(args[0]))},
        {"Object", new NativeFunction(0, (_, args) => new Runtime.KulaObject())},
        {"Array", new NativeFunction(0, (_, args) => new Runtime.KulaArray())},
        {"asArray", new NativeFunction(-1, (_, args) => new Runtime.KulaArray(args))},
        {"asObject", new NativeFunction(-1, (_, args) => {
            if (args.Count % 2 != 0) {
                throw new InterpreterInnerException("Need odd argument(s) but even is given.");
            }
            Runtime.KulaObject my_obj = new();
            for (int i = 0; i + 1 < args.Count; i += 2) {
                string key = Assert<string>(args[i]);
                object? value = args[i + 1];
                my_obj.Set(key, value);
            }
            return my_obj;
        })},
        {"typeof", new NativeFunction(1, (_, args) => TypeStringify(args[0]?.GetType()))},
        {"throw", new NativeFunction(1, (_, args) => throw new InterpreterInnerException(Assert<string>(args[0])))},
        {"printInline", new NativeFunction(-1, (_, args) => {
            List<string> items = new();
            foreach (object? item in args) {
                items.Add(Stringify(item));
            }
            Console.Write(string.Join("", items));
            return null;
        })},
    };

    public static readonly Dictionary<string, Runtime.KulaObject> global_protos;
    public static readonly Runtime.KulaObject string_proto;
    public static readonly Runtime.KulaObject array_proto;
    public static readonly Runtime.KulaObject number_proto;
    public static readonly Runtime.KulaObject object_proto;
    public static readonly Runtime.KulaObject function_proto;
    static StandardLibrary()
    {
        string_proto = new Runtime.KulaObject();
        string_proto.Set("at", new NativeFunction(1, (_this, args) => {
            string str = Assert<string>(_this);
            double d1 = Assert<double>(args[0]);
            return str.Substring((int)d1, 1);
        }));
        string_proto.Set("cut", new NativeFunction(2, (_this, args) => {
            string str = Assert<string>(_this);
            double d1 = Assert<double>(args[0]);
            double d2 = Assert<double>(args[1]);
            return str.Substring((int)d1, (int)d2);
        }));
        string_proto.Set("parse", new NativeFunction(0, (_this, _) => {
            string str = Assert<string>(_this);
            if (double.TryParse(str, out double d)) {
                return d;
            }
            return null;
        }));
        string_proto.Set("split", new NativeFunction(1, (_this, args) => {
            string separator = Assert<string>(args[0]);
            string str = Assert<string>(_this);
            return new Runtime.KulaArray(str.Split(separator));
        }));
        string_proto.Set("length", new NativeFunction(0, (_this, _) =>
            (double)Assert<string>(_this).Length
        ));
        string_proto.Set("charCode", new NativeFunction(0, (_this, _) =>
            (double)(short)Assert<string>(_this)[0]
        ));

        number_proto = new Runtime.KulaObject();
        number_proto.Set("floor", new NativeFunction(0, (_this, _) =>
            Math.Floor(Assert<double>(_this))
        ));
        number_proto.Set("round", new NativeFunction(0, (_this, _) =>
            Math.Round(Assert<double>(_this))
        ));

        array_proto = new Runtime.KulaObject();
        array_proto.Set("insert", new NativeFunction(2, (_this, args) => {
            double d1 = Assert<double>(args[0]);
            Runtime.KulaArray array = Assert<Runtime.KulaArray>(_this);
            array.Insert(d1, args[1]);
            return null;
        }));
        array_proto.Set("remove", new NativeFunction(1, (_this, args) => {
            double d1 = Assert<double>(args[0]);
            Runtime.KulaArray array = Assert<Runtime.KulaArray>(_this);
            array.Remove(d1);
            return null;
        }));
        array_proto.Set("copy", new NativeFunction(0, (_this, _) => {
            return new Runtime.KulaArray(Assert<Runtime.KulaArray>(_this).data);
        }));
        array_proto.Set("length", new NativeFunction(0, (_this, _) => {
            return (double)Assert<Runtime.KulaArray>(_this).Size;
        }));
        array_proto.Set("foreach", new NativeFunction(1, (_this, args) => {
            ICallable lambda = Assert<ICallable>(args[0]);
            Runtime.KulaArray arr = Assert<Runtime.KulaArray>(_this);
            for (int i = 0; i < arr.data.Count; ++i) {
                lambda.Call(new List<object?>() { arr.data[i], (double)i });
            }
            return null;
        }));
        array_proto.Set("map", new NativeFunction(1, (_this, args) => {
            ICallable lambda = Assert<ICallable>(args[0]);
            Runtime.KulaArray arr = Assert<Runtime.KulaArray>(_this);
            List<object?> new_arr = new();
            for (int i = 0; i < arr.data.Count; ++i) {
                object? item = lambda.Call(new List<object?>() { arr.data[i], (double)i });
                new_arr.Add(item);
            }
            return new Runtime.KulaArray(new_arr);
        }));
        array_proto.Set("reduce", new NativeFunction(2, (_this, args) => {
            ICallable lambda = Assert<ICallable>(args[0]);
            Runtime.KulaArray arr = Assert<Runtime.KulaArray>(_this);
            object? total = args[1];
            List<object?> new_arr = new();
            for (int i = 0; i < arr.data.Count; ++i) {
                object? item = lambda.Call(new List<object?>() { total, arr.data[i], (double)i });
                total = item;
            }
            return total;
        }));
        array_proto.Set("filter", new NativeFunction(1, (_this, args) => {
            ICallable lambda = Assert<ICallable>(args[0]);
            Runtime.KulaArray arr = Assert<Runtime.KulaArray>(_this);
            List<object?> new_arr = new();
            for (int i = 0; i < arr.data.Count; ++i) {
                object? flag = lambda.Call(new List<object?>() { arr.data[i], (double)i });
                if (Assert<bool>(flag)) {
                    new_arr.Add(arr.data[i]);
                }
            }
            return new Runtime.KulaArray(new_arr);
        }));

        object_proto = new Runtime.KulaObject();
        object_proto.Set("copy", new NativeFunction(0, (_this, _) => {
            Runtime.KulaObject origin_object = Assert<Runtime.KulaObject>(_this);
            Runtime.KulaObject new_object = new();
            foreach (KeyValuePair<string, object?> item in origin_object.data) {
                new_object.Set(item.Key, item.Value);
            }
            return new_object;
        }));
        object_proto.Set("foreach", new NativeFunction(1, (_this, args) => {
            ICallable lambda = Assert<ICallable>(args[0]);
            Runtime.KulaObject obj = Assert<Runtime.KulaObject>(_this);
            foreach (KeyValuePair<string, object?> item in obj.data) {
                lambda.Call(new List<object?>() { item.Key, item.Value });
            }
            return null;
        }));

        function_proto = new Runtime.KulaObject();
        function_proto.Set("apply", new NativeFunction(2, (_this, args) => {
            ICallable function = Assert<ICallable>(_this);
            Runtime.KulaArray arglist = Assert<Runtime.KulaArray>(args[1]);
            function.Bind(args[0]);
            return function.Call(arglist.data);
        }));

        global_protos = new Dictionary<string, Runtime.KulaObject> {
            {"__string_proto__", string_proto},
            {"__array_proto__", array_proto},
            {"__number_proto__", number_proto},
            {"__object_proto__", object_proto},
            {"__function_proto__", function_proto},
        };
    }

    public static string Stringify(object? @object)
    {
        if (@object is bool object_bool) {
            return object_bool.ToString().ToLower();
        }
        else if (@object is double object_double) {
            return object_double.ToString();
        }

        return @object?.ToString() ?? "null";
    }

    public static bool Booleanify(object? @object)
    {
        if (@object is null) {
            return false;
        }
        if (@object is bool object_bool) {
            return object_bool;
        }

        return true;
    }

    public static string TypeStringify(Type? o)
    {
        if (o is null) {
            return "None";
        }
        else if (o == typeof(double)) {
            return "Number";
        }
        else if (o == typeof(string)) {
            return "String";
        }
        else if (o == typeof(bool)) {
            return "Bool";
        }
        else if (o == typeof(ICallable) || o == typeof(VMFunction)) {
            return "Function";
        }
        else if (o == typeof(Runtime.KulaArray)) {
            return "Array";
        }
        else if (o == typeof(Runtime.KulaObject)) {
            return "Object";
        }
        return o.ToString();
    }

    public static T Assert<T>(object? o)
    {
        if (o is T t) {
            return t;
        }
        throw new InterpreterInnerException($"Need '{TypeStringify(typeof(T))}' but '{TypeStringify(o?.GetType())}' is given.");
    }
}
