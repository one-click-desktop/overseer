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

                ErrorDTO res = new ErrorDTO();
                res.Message = error.Message;
                switch (error)
                {
                    case HttpException e:
                        res.Code = e.ErrorCode;
                        break;
                    default:
                        // unhandled error
                        res.Code = (int)HttpStatusCode.InternalServerError;
                        break;
                }
                
                response.StatusCode = res.Code;
                var result = JsonSerializer.Serialize(res);
                await response.WriteAsync(result);
                throw;
            }
        }
    }
}
