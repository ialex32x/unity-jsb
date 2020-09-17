using System;

namespace QuickJS.Unity
{
    public class DefaultPathResolver : Utils.PathResolver
    {
        public DefaultPathResolver()
        : base(new DefaultJsonConverter())
        {
        }
    }
}
