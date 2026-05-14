using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace CloneWeb.Middleware
{
    public class VisitorCounterMiddleware
    {
        private readonly RequestDelegate _requestDelegate;

        public VisitorCounterMiddleware(RequestDelegate requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Cookies["VisitorId"] == null)
            {
                context.Response.Cookies.Append("VisitorId", Guid.NewGuid().ToString(), new CookieOptions
                {
                    Path = "/",
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                });
                context.Items.Add("Visitor", 1);
            }

            await _requestDelegate(context);
        }
    }
}
