namespace Kula.Data.Type
{
    public interface IType
    {
        bool Check(object o);

        bool CheckType(IType type);
    }
}
