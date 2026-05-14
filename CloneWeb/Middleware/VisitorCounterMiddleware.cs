using EntityDataModel.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
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
            int Value = 0;
            string visitorId = context.Request.Cookies["VisitorId"];
            if (visitorId == null)
            {
                Value++;
                //don the necessary staffs here to save the count by one
                context.Response.Cookies.Append("VisitorId", Guid.NewGuid().ToString(), new CookieOptions()
                {
                    Path = "/",
                    HttpOnly = true,
                    Secure = true,
                });
                context.Items.Add("Visitor", Value);
            }


            await _requestDelegate(context);
        }

    }
}
