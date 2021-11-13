namespace Kula.Data.Type
{
    public interface IType
    {
        System.Type ToType { get; }

        bool Check(object o);
        
        DuckType ToDuck { get; }
        
        bool IsDuck { get; }
    }
}
