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

        public static async System.Threading.Tasks.Task SimpleTest(int ms)
        {
            await System.Threading.Tasks.Task.Delay(ms);
        }
    }
}
