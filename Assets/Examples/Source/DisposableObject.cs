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

        // 存在交叉使用, 且互相保留引用的情况下, 生命周期管理将变得复杂, 比如: 由CS构造的实例传递给JS, 或由JS构造的CS实例传递回CS.
        // 目前行为是不进行处理. 可以手工使用 jsb.SetDisposable(o, managed) 介入
        public static DisposableObject CreateDisposableObject()
        {
            return new DisposableObject();
        }
    }
}
