namespace jsb
{
    using UnityEngine;

    public class Sample:MonoBehaviour
    {
        void Awake()
        {
            var rt = JSBridgeDLL.XJS_NewRuntime();
            Debug.Log(rt);
            JSBridgeDLL.XJS_FreeRuntime(rt);
        }
    }
}