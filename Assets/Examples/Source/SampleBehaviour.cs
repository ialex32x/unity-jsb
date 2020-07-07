using UnityEngine;
using QuickJS;

[JSType]
public class SampleBehaviour : MonoBehaviour
{
    private ScriptPromise _p;

    public ScriptPromise Wait()
    {
        if (_p != null)
        {
            return null;
        }

        var context = ScriptEngine.GetRuntime().GetMainContext();
        var promise = new ScriptPromise(context);

        _p = promise;
        return _p;
    }

    void OnGUI()
    {
        var p = _p;
        if (p == null)
        {
            return;
        }
        if (GUILayout.Button("Resolve"))
        {
            _p = null;
            p.Resolve("test");
        }
    }
}