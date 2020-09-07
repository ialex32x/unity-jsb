
public class TWrapper<T>
{
    private T _obj;

    public TWrapper(T obj)
    {
        _obj = obj;
    }

    public T GetValue()
    {
        return _obj;
    }

    public void SetValue(T obj)
    {
        _obj = obj;
    }

    public static TWrapper<int> GetIntWrapper()
    {
        return null;
    }
}
