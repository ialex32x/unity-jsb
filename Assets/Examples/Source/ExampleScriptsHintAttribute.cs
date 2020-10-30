using System;
using UnityEngine;

namespace Example
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