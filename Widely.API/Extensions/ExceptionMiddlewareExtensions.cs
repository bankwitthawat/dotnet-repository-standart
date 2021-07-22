using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Widely.API.Infrastructure.Exceptions;

namespace Widely.API.Extensions
{
    public class ExceptionMiddlewareExtensions
    {

        private readonly RequestDelegate _next;
        private readonly Logger _logger;
        public ExceptionMiddlewareExtensions(RequestDelegate next)
        {
            _next = next;
            _logger = NLog.LogManager.GetCurrentClassLogger();
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

                switch (error)
                {
                    case AppException e:
                        // custom application error
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;
                    case KeyNotFoundException e:
                        // not found error
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    default:
                        // unhandled error
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                _logger.Error($"Something went wrong: {response.StatusCode} {error}");
                var result = JsonSerializer.Serialize(new { message = $"Something went wrong: {error?.Message}" });
                await response.WriteAsync(result);
            }
        }
    }
}
