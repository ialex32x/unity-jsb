namespace jsb
{
    using UnityEngine;

    public class Sample:MonoBehaviour
    {
        void Awake()
        {
            var c = JSBridgeDLL.test(1, 2);
            Debug.Log(c);
            JSBridgeDLL.init();
        }
    }
}