using Kula.Core.Runtime;

namespace Kula.Core.Container;

public class Array {
    internal readonly List<object?> data;

    public int Size => data.Count;
    public Array() {
        data = new List<object?>();
    }

    public Array(IEnumerable<object?> list) {
        data = new List<object?>(list);
    }

    public object? Get(double index) {
        int j = (int)index;
        if (j < Size && j >= 0) {
            return data[j];
        }
        throw new RuntimeError("Array index out of range.");
    }

    public void Set(double index, object? value) {
        int j = (int)index;
        if (j < Size && j >= 0) {
            data[j] = value;
        }
        else {
            throw new RuntimeError("Array index out of range.");
        }
    }

    public void Insert(double index, object? value) {
        int j = (int)index;
        if (j <= Size && j >= 0) {
            data.Insert(j, value);
        }
        else {
            throw new RuntimeError("Array index out of range.");
        }
    }

    public void Remove(double index) {
        int j = (int)index;
        if (j < Size && j >= 0) {
            data.RemoveAt(j);
        }
        else {
            throw new RuntimeError("Array index out of range.");
        }
    }

    public override string ToString() {
        string[] items = new string[Size];
        int i = 0;
        foreach (object? item in data) {
            string value = StandardLibrary.Stringify(item);
            items[i++] = item is string ? $"\"{value}\"" : value;
        }
        return $"[{string.Join(',', items)}]";
    }
}
