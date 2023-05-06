using System.Security.Claims;

namespace VolumeControl.SignalR.Server
{
    public class CheckHubMiddleware
    {
        private readonly RequestDelegate _next;

        public CheckHubMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var requestPath = context.Request.Path.Value;
            if (requestPath.Equals("/devices"))
            {
                var uid = context.Request.Query["uid"].ToString();
                if (string.IsNullOrEmpty(uid))
                {
                    context.Response.StatusCode = 401;
                    return;
                }
                //校验token的正确性

                //表示校验通过,直接跳过，继续运行即可

                context.User.AddIdentity(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, uid) }));

                await _next.Invoke(context);
            }
            else
            {
                //表示非SignalR连接，直接跳过，继续运行即可
                await _next.Invoke(context);
            }
        }
    }
}
