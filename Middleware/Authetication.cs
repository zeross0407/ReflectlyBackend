using DocumentFormat.OpenXml.Spreadsheet;
using Reflectly.Entity;
using Reflectly.Service;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Reflectly.Middleware
{
    public class DeviceValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly TokenService _TokenService;

        public DeviceValidationMiddleware(RequestDelegate next, IServiceProvider serviceProvider,
            TokenService tokenService)
        {
            _next = next;
            _serviceProvider = serviceProvider;
            _TokenService = tokenService;
        }

        public async Task Invoke(HttpContext context)
        {
            //User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var token = context.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");


            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(token))
            {
                AccessToken tk = await _TokenService.Get_by_token_Async(token);
                if( tk == null || tk.userId != userId)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid device.");
                    return;
                }
            }

            await _next(context); // Tiếp tục middleware tiếp theo
        }
    }
}