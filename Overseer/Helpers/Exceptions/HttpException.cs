using System;

namespace OneClickDesktop.Overseer.Helpers.Exceptions
{
    public class HttpException: Exception
    {
        public int ErrorCode { get; private set; }

        public HttpException(int errorCode) : base("")
        {
            ErrorCode = errorCode;
        }

        public HttpException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
