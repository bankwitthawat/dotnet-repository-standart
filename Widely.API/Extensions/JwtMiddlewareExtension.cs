using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Widely.API.Infrastructure.Exceptions;
using Widely.BusinessLogic.Utilities;

namespace Widely.API.Extensions
{
    public class JwtMiddlewareExtension
    {
        private readonly RequestDelegate _next;
        public readonly IConfiguration _configuration;

        public JwtMiddlewareExtension(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context, JwtManager jwtUtils)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = jwtUtils.ValidateJwtToken(token);
            if (userId == null)
            {
                throw new UnauthorizeException("Access Token Invalid.");
            }

            await _next(context);
        }
    }
}
