using QuickJS;
using System;

namespace Example
{
    using UnityEngine;

    /// <summary>
    /// If the lifetime of C# object could be fully controlled by JS, it could be mark as disposable at runtime.
    /// Or marking this type by SetDisposable in CustomBinding.
    /// objects/types marked by disposable will be automatically disposed when it's corresponding JSValue being finalized
    /// <example>
    ///     : in typescript/javascript
    ///     <code>
    ///         jsb.SetDisposable(inst, true); // mark instances up to you
    ///     </code>
    /// </example>
    /// <example>
    ///     : or in CustomBinding
    ///     <code>
    ///         bindingManager.AddExportedType(typeof(DisposableObject)).SetDisposable();
    ///     </code>
    /// </example>
    /// <see href="https://github.com/ialex32x/unity-jsb/blob/c584aec2f2721faf0c76f0f80fc45f05ffd4cb26/Assets/Examples/Source/Editor/CustomBinding.cs">Example in CustomBinding</see>
    /// </summary>
    public class DisposableObject : IDisposable
    {
        public DisposableObject()
        {
            Debug.Log("DisposableObject.Constructor");
        }

        public void Dispose()
        {
            Debug.Log("DisposableObject.Dispose");
        }

        public static DisposableObject CreateDisposableObject()
        {
            return new DisposableObject();
        }
    }
}
