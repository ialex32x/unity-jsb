#if !UNITY_WEBGL 
namespace QuickJS.Extra.WebSockets
{
    public enum ulws_ssl_type
    {
        ULWS_DEFAULT = 0,
        ULWS_USE_SSL = 1,
        ULWS_USE_SSL_ALLOW_SELFSIGNED = 2,
    }
}
#endif