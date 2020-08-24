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
        public string adderName = null; // 绑定代码名
        public string removerName = null;
        public string proxyName = null; // 非静态event需要一个property.getter在实例上创建一个event object实例
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
            do
            {
                if (this.isStatic)
                {
                    this.adderName = "BindStaticAdd_" + eventInfo.Name;
                    this.removerName = "BindStaticRemove_" + eventInfo.Name;
                }
                else
                {
                    this.adderName = "BindAdd_" + eventInfo.Name;
                    this.removerName = "BindRemove_" + eventInfo.Name;
                    this.proxyName = "BindProxy_" + eventInfo.Name;
                }
            } while (false);

            this.regName = TypeBindingInfo.GetNamingAttribute(eventInfo);
        }
    }
}
