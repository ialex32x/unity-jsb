using QuickJS;
using System;

namespace jsb
{
    [JSType]
    public class AsyncTaskTest
    {
        public static System.Threading.Tasks.Task GetHostEntryAsync(string host)
        {
            return System.Net.Dns.GetHostEntryAsync(host);
        }
    }
}
