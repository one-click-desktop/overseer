using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OneClickDesktop.Api.Models;
using OneClickDesktop.Backend.Classes;

namespace OneClickDesktop.Overseer
{
    public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
    {
        public int Order { get; } = int.MaxValue - 10;

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is ErrorHttpException exception)
            {
                Error e = new Error()
                {
                    Code = exception.ErrorCode,
                    Message = exception.Message
                };

                context.Result = new JsonResult(e);
                context.ExceptionHandled = true;
            }
        }
    }
}