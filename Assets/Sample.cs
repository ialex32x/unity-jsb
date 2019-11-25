namespace jsb
{
    using UnityEngine;

    public class Sample:MonoBehaviour
    {
        void Awake()
        {
            var c = JSBridgeDLL.foo(1, 2);
            Debug.Log(c);
        }
    }
}