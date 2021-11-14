using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OneClickDesktop.Backend.Classes
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
