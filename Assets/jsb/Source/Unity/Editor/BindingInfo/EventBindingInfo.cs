using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QuickJS.Editor
{
    using UnityEngine;
    using UnityEditor;

    public class EventBindingInfo
    {
        public string name = null; // 绑定代码名
        public string regName = null; // js 注册名

        public Type declaringType;
        public EventInfo eventInfo;

        public bool isStatic
        {
            get { return eventInfo.GetAddMethod().IsStatic; }
        }

        public EventBindingInfo(Type declaringType, EventInfo eventInfo)
        {
            this.declaringType = declaringType;
            this.eventInfo = eventInfo;

            if (this.isStatic)
            {
                this.name = "BindStaticEvent_" + eventInfo.Name;
            }
            else
            {
                this.name = "BindEvent_" + eventInfo.Name;
            }

            this.regName = TypeBindingInfo.GetNamingAttribute(eventInfo);
        }
    }
}
