using System.Net;

namespace OneClickDesktop.Overseer.Helpers.Exceptions
{
    public class UnathorizedHttpException : HttpException
    {
        public UnathorizedHttpException(): base("Unathorized", (int)HttpStatusCode.Unauthorized)
        { }
    }
}
