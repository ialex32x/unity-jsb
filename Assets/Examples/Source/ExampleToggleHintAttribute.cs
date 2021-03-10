using System;
using UnityEngine;

namespace Example
{
    public class ExampleToggleHintAttribute : PropertyAttribute
    {
        public string text { get; set; }

        public ExampleToggleHintAttribute(string text)
        {
            this.text = text;
        }
    }
}