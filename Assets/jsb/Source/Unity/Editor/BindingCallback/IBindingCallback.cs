using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public interface IBindingCallback
    {
        bool OnTypeGenerating(TypeBindingInfo typeBindingInfo, int current, int total);
        void OnGenerateFinish();
    }
}
