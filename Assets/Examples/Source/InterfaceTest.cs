using QuickJS;

[JSType]
public interface MyInterface
{
    void Foo();
}

[JSType]
public class MyClass : MyInterface
{
    public void Foo()
    {
        UnityEngine.Debug.Log("MyClassFoo");
    }

    public static MyInterface GetMyInterface()
    {
        return new MyClass();
    }
}
