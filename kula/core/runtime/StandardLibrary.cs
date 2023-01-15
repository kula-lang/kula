namespace Kula.Core.Runtime;

public static class StandardLibrary {
    private static readonly DateTime start = DateTime.UtcNow;
    public static readonly Dictionary<string, NativeFunction> global_functions = new Dictionary<string, NativeFunction>{
        {"clock", new NativeFunction(0, (_, args) => (DateTime.UtcNow - start).TotalMilliseconds / 1000.0)},
        {"String", new NativeFunction(1, (_, args) => Stringify(args[0]))},
        {"Bool", new NativeFunction(1, (_, args) => Booleanify(args[0]))},
        {"Object", new NativeFunction(0, (_, args) => new Container.Object())},
        {"Array", new NativeFunction(0, (_, args) => new Container.Array())},
        {"asArray", new NativeFunction(-1, (_, args) => new Container.Array(args))},
        {"typeof", new NativeFunction(1, (_, args) => TypeStringify(args[0]?.GetType()))},
        {"throw", new NativeFunction(1, (_, args) => throw new RuntimeError(Assert<string>(args[0])))},
    };
    public static readonly Container.Object string_proto;
    public static readonly Container.Object array_proto;
    public static readonly Container.Object number_proto;
    public static readonly Container.Object object_proto;
    static StandardLibrary() {
        string_proto = new Container.Object();
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
            throw new RuntimeError("Illegal number-string.");
        }));
        string_proto.Set("split", new NativeFunction(1, (_this, args) => {
            string separator = Assert<string>(args[0]);
            string str = Assert<string>(_this);
            return new Container.Array(str.Split(separator));
        }));
        string_proto.Set("length", new NativeFunction(0, (_this, _) => {
            return (double)(Assert<string>(_this)).Length;
        }));

        number_proto = new Container.Object();

        array_proto = new Container.Object();
        array_proto.Set("insert", new NativeFunction(2, (_this, args) => {
            double d1 = Assert<double>(args[0]);
            Container.Array array = Assert<Container.Array>(_this);
            array.Insert(d1, args[1]);
            return null;
        }));
        array_proto.Set("remove", new NativeFunction(1, (_this, args) => {
            double d1 = Assert<double>(args[0]);
            Container.Array array = Assert<Container.Array>(_this);
            array.Remove(d1);
            return null;
        }));
        array_proto.Set("copy", new NativeFunction(0, (_this, _) => {
            return new Container.Array(Assert<Container.Array>(_this).data);
        }));
        array_proto.Set("length", new NativeFunction(0, (_this, _) => {
            return (double)(Assert<Container.Array>(_this).Size);
        }));

        object_proto = new Container.Object();
        object_proto.Set("copy", new NativeFunction(0, (_this, _) => {
            Container.Object origin_object = Assert<Container.Object>(_this);
            Container.Object new_object = new Container.Object();
            foreach (KeyValuePair<string, object?> item in origin_object.data) {
                new_object.Set(item.Key, item.Value);
            }
            return new_object;
        }));
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

    public static string TypeStringify(Type? o) {
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
        else if (o == typeof(ICallable)) {
            return "Function";
        }
        else if (o == typeof(Container.Array)) {
            return "Array";
        }
        else if (o == typeof(Container.Object)) {
            return "Object";
        }
        return "Unknown";
    }

    public static T Assert<T>(object? o) {
        if (o is T t) {
            return t;
        }
        throw new RuntimeError($"Need '{TypeStringify(typeof(T))}' but '{TypeStringify(o?.GetType())}' is given.");
    }
}
