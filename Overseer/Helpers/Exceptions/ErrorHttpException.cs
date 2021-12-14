using System.Net;

namespace OneClickDesktop.Overseer.Helpers.Exceptions
{
    public class ErrorHttpException : HttpException
    {        
        public ErrorHttpException(int errorCode): base(errorCode)
        { }

        public ErrorHttpException(string message, int errorCode): base(message, errorCode) 
        { }

        public ErrorHttpException(HttpStatusCode errorCode) : base((int)errorCode)
        { }

        public ErrorHttpException(string message, HttpStatusCode errorCode) : base(message, (int)errorCode)
        { }
    }
}
