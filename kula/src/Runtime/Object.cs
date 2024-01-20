using Kula.Utilities;

namespace Kula.Runtime;

public class KulaObject
{
    private static readonly string __proto__ = "__proto__";
    internal readonly Dictionary<string, object?> data;

    public KulaObject()
    {
        data = new Dictionary<string, object?>();
        Set(__proto__, StandardLibrary.object_proto);
    }

    public object? Get(string key)
    {
        if (data.ContainsKey(key)) {
            return data[key];
        }
        else if (data.GetValueOrDefault("__proto__", null) is KulaObject proto) {
            return proto.Get(key);
        }
        return null;
    }

    public void Set(string key, object? value)
    {
        data[key] = value;
    }

    public override string ToString()
    {
        List<string> items = new List<string>();
        foreach (KeyValuePair<string, object?> item in data) {
            if (item.Key != __proto__) {
                string value = StandardLibrary.Stringify(item.Value);
                items.Add($"\"{item.Key}\":{(item.Value is string ? ($"\"{value}\"") : (value))}");
            }
        }
        return $"{{{string.Join(',', items)}}}";
    }
}
