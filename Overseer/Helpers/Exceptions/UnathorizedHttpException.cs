using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OneClickDesktop.Overseer.Helpers.Exceptions
{
    public class UnathorizedHttpException : HttpException
    {
        public UnathorizedHttpException(): base("Unathorized", (int)HttpStatusCode.Unauthorized)
        { }
    }
}
