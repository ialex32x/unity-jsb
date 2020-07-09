#if !UNITY_WEBGL || UNITY_WEBGL
namespace WebSockets
{
    /*
    * NOTE: These public enums are part of the abi.  If you want to add one,
    * add it at where specified so existing users are unaffected.
    */
    public enum lws_write_protocol
    {
        LWS_WRITE_TEXT = 0,
        /**< Send a ws TEXT message,the pointer must have LWS_PRE valid
         * memory behind it.  The receiver expects only valid utf-8 in the
         * payload */
        LWS_WRITE_BINARY = 1,
        /**< Send a ws BINARY message, the pointer must have LWS_PRE valid
         * memory behind it.  Any sequence of bytes is valid */
        LWS_WRITE_CONTINUATION = 2,
        /**< Continue a previous ws message, the pointer must have LWS_PRE valid
         * memory behind it */
        LWS_WRITE_HTTP = 3,
        /**< Send HTTP content */

        /* LWS_WRITE_CLOSE is handled by lws_close_reason() */
        LWS_WRITE_PING = 5,
        LWS_WRITE_PONG = 6,

        /* Same as write_http but we know this write ends the transaction */
        LWS_WRITE_HTTP_FINAL = 7,

        /* HTTP2 */

        LWS_WRITE_HTTP_HEADERS = 8,
        /**< Send http headers (http2 encodes this payload and LWS_WRITE_HTTP
         * payload differently, http 1.x links also handle this correctly. so
         * to be compatible with both in the future,header response part should
         * be sent using this regardless of http version expected)
         */
        LWS_WRITE_HTTP_HEADERS_CONTINUATION = 9,
        /**< Continuation of http/2 headers
         */

        /****** add new things just above ---^ ******/

        /* flags */

        LWS_WRITE_NO_FIN = 0x40,
        /**< This part of the message is not the end of the message */

        LWS_WRITE_H2_STREAM_END = 0x80,
        /**< Flag indicates this packet should go out with STREAM_END if h2
         * STREAM_END is allowed on DATA or HEADERS.
         */

        LWS_WRITE_CLIENT_IGNORE_XOR_MASK = 0x80
        /**< client packet payload goes out on wire unmunged
         * only useful for security tests since normal servers cannot
         * decode the content if used */
    };
}
#endif