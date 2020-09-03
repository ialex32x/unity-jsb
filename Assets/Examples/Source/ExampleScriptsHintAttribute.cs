using System;
using UnityEngine;

namespace jsb
{
    public class ExampleScriptsHintAttribute : PropertyAttribute
    {
        public string path { get; set; }

        public ExampleScriptsHintAttribute(string path)
        {
            this.path = path;
        }
    }
}