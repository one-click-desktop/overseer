using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using OneClickDesktop.Overseer.Helpers.Exceptions;
using OneClickDesktop.Api.Models;

namespace OneClickDesktop.Overseer.Helpers
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                Error res = new Error();
                res.Message = error.Message;
                switch (error)
                {
                    case AppException e:
                        // custom application error
                        res.Code = (int)HttpStatusCode.BadRequest;
                        break;
                    case HttpException e:
                        res.Code = e.ErrorCode;
                        break;
                    default:
                        // unhandled error
                        res.Code = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                var result = JsonSerializer.Serialize(new { message = error?.Message });
                await response.WriteAsync(result);
            }
        }
    }
}
