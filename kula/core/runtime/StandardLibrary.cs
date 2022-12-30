namespace Kula.Core.Runtime;

static class StandardLibrary {
    private static readonly DateTime start = DateTime.UtcNow;
    public static readonly Dictionary<string, NativeFunction> global_functions = new Dictionary<string, NativeFunction>{
        {"clock", new NativeFunction(0, (_, args) => {
            return (DateTime.UtcNow - start).TotalMilliseconds / 1000.0;
        })},
        {"String", new NativeFunction(1, (_, args) => Stringify(args[0]))},
        {"Bool", new NativeFunction(1, (_, args) => Booleanify(args[0]))},
        {"Object", new NativeFunction(0, (_, args) => new Container.Object())},
        {"Array", new NativeFunction(0, (_, args) => new Container.Array())},
        {"AsArray", new NativeFunction(-1, (_, args) => new Container.Array(args))}
    };
    public static readonly Container.Object string_proto;
    public static readonly Container.Object array_proto;
    public static readonly Container.Object number_proto;
    public static readonly Container.Object object_proto;
    static StandardLibrary() {
        string_proto = new Container.Object();
        string_proto.Set("at", new NativeFunction(1, (_this, args) => {
            if (_this is string str && args[0] is double d1) {
                return str.Substring((int)d1, 1);
            }
            throw new RuntimeError("Wrong Arguments in 'string.at'.");
        }));
        string_proto.Set("cut", new NativeFunction(2, (_this, args) => {
            if (_this is string str && args[0] is double d1 && args[1] is double d2) {
                return str.Substring((int)d1, (int)d2);
            }
            throw new RuntimeError("Wrong Arguments in 'string.cut'.");
        }));
        string_proto.Set("parse", new NativeFunction(0, (_this, _) => {
            if (_this is string str) {
                if (double.TryParse(str, out double d)) {
                    return d;
                }
            }
            throw new RuntimeError("Wrong Arguments in 'string.parse'.");
        }));
        string_proto.Set("length", new NativeFunction(0, (_this, _) => {
            return (double)((string)_this!).Length;
        }));

        number_proto = new Container.Object();

        array_proto = new Container.Object();
        array_proto.Set("insert", new NativeFunction(2, (_this, args) => {
            if (args[0] is double d1 && _this is Container.Array array) {
                array.Insert(d1, args[1]);
                return null;
            }
            throw new RuntimeError("Wrong Arguments in 'array.insert'.");
        }));
        array_proto.Set("remove", new NativeFunction(1, (_this, args) => {
            if (args[0] is double d1 && _this is Container.Array array) {
                array.Remove(d1);
                return null;
            }
            throw new RuntimeError("Wrong Arguments in 'array.remove'.");
        }));
        array_proto.Set("copy", new NativeFunction(0, (_this, _) => {
            return new Container.Array(((Container.Array)_this!).data);
        }));
        array_proto.Set("length", new NativeFunction(0, (_this, _) => {
            return (double)(((Container.Array)_this!).Size);
        }));

        object_proto = new Container.Object();
    }

    public static string Stringify(object? @object) {
        if (@object is bool object_bool) {
            return object_bool.ToString().ToLower();
        }
        else if (@object is double object_double) {
            return object_double.ToString();
        }

        return @object?.ToString() ?? "null";
    }

    public static bool Booleanify(object? @object) {
        if (@object is null) {
            return false;
        }
        if (@object is bool object_bool) {
            return object_bool;
        }

        return true;
    }

    public static string TypeOf(object? @object) {
        if (@object is null) {
            return "None";
        }
        else if (@object is double) {
            return "Number";
        }
        else if (@object is string) {
            return "String";
        }
        else if (@object is bool) {
            return "Bool";
        }
        else if (@object is ICallable) {
            return "Function";
        }
        else if (@object is Container.Array) {
            return "Array";
        }
        else if (@object is Container.Object) {
            return "Object";
        }
        return "Unknown";
    }
}
