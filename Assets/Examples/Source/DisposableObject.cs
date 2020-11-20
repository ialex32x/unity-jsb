using QuickJS;
using System;

namespace Example
{
    using UnityEngine;

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
    }
}
