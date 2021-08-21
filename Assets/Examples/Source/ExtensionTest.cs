using QuickJS;
using System;

namespace Example
{
    using UnityEngine;

    public static class ExtensionTest
    {
        public static void ResetAll(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        public static void TestWithArgs(this Transform transform, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            transform.localPosition = pos;
            transform.localRotation = rot;
            transform.localScale = scale;
        }

        public static bool TestWithArgsAndOut(this Transform transform, Vector3 pos, Quaternion rot, Vector3 scale, out float dist)
        {
            transform.localPosition = pos;
            transform.localRotation = rot;
            transform.localScale = scale;
            dist = pos.magnitude;
            return true;
        }

        public static void TestWithScriptObject(this Transform transform, ScriptFunction function)
        {
            function?.Invoke();
        }
    }
}
