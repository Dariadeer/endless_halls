namespace Shared.Data;

public interface ISnapshot<T> where T : ISnapshot<T>
{
    public T Copy();
}