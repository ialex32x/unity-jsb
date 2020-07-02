using System;
using UnityEngine;

namespace QuickJS.Hotfix
{
    public delegate int ProxyCall4(ILAccessSample a0, string a1, ref int a2, out char a3);

    public class ILAccessSample
    {
        public static Action<ILAccessSample, int, string> _proxy_Call = null;
        public static Func<ILAccessSample, string, int> _proxy_Call2 = null;
        public static ProxyCall4 _proxy_Call4 = null;
        public static Action<ILAccessSample> _proxy_Call5_replace = null;
        public static Action<ILAccessSample> _proxy_Call5_before = null;
        public static Action<ILAccessSample> _proxy_Call5_after = null;
        private static string __xxxx = "xxxx;";

        public static void Foo()
        {
            System.Console.WriteLine("Foo");
        }

        public void Call(int a, string b)
        {
            if (_proxy_Call != null)
            {
                _proxy_Call(this, a, b);
                return;
            }

            var loc0 = 0;
            Debug.LogFormat("old project code");
        }

        public static int Call2(string x)
        {
            if (_proxy_Call2 != null)
            {
                var ret = _proxy_Call2.Invoke(null, x);
                return ret;
            }

            return 2;
        }

        public int Call3(string x)
        {
            if (_proxy_Call2 != null)
            {
                return _proxy_Call2.Invoke(this, x);
            }

            string xxx = "lkjasdf;";
            var ggg = 123;
            return ggg;
        }

        public static int Call4(string x, ref int coderef, out char codeout)
        {
            if (_proxy_Call4 != null)
            {
                return _proxy_Call4(null, x, ref coderef, out codeout);
            }

            string xxx = "lkjasdf;";
            var ggg = 123 + coderef;
            codeout = 'x';
            return ggg;
        }

        public void Call5()
        {
            if (_proxy_Call5_replace != null)
            {
                _proxy_Call5_replace(this);
                return;
            }

            try
            {
                if (_proxy_Call5_before != null)
                {
                    _proxy_Call5_before(this);
                }

                Debug.LogErrorFormat("hello, world!!!");
            }
            finally
            {
                if (_proxy_Call5_after != null)
                {
                    _proxy_Call5_after(this);
                }
            }
        }
    }
}