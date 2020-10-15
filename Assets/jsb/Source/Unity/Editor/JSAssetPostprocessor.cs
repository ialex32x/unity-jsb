using System;
using System.Reflection;

namespace QuickJS.Unity
{
    using Native;
    using UnityEngine;
    using UnityEditor;

    public class JSAssetPostprocessor : AssetPostprocessor
    {
        private void OnPostprocessTexture(Texture2D texture)
        {
            // call js script 
        }
    }
}
