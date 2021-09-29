using QuickJS;

/// <summary>
/// demonstrates the binding of types implementing interfaces
/// </summary>
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
