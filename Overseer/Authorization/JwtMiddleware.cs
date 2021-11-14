using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OneClickDesktop.Overseer.Helpers;
using OneClickDesktop.Overseer.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace OneClickDesktop.Overseer.Authorization
{

    //Jakiekolwiek zmiany w autoryzacji można wprowadzić patrzaąc na ten poradnik
    //https://jasonwatmore.com/post/2021/07/29/net-5-role-based-authorization-tutorial-with-example-api
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtSettings _appSettings;

        public JwtMiddleware(RequestDelegate next, IOptions<JwtSettings> appSettings)
        {
            _next = next;
            _appSettings = appSettings.Value;
        }

        public async Task Invoke(HttpContext context, IUserService userService, IJwtUtils jwtUtils)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = jwtUtils.ValidateJwtToken(token);
            if (userId != null)
            {
                // attach user to context on successful jwt validation
                context.Items["User"] = userService.GetUserById(userId.Value);
            }

            await _next(context);
        }
    }
}
