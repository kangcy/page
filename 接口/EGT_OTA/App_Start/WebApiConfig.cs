using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace EGT_OTA
{
    /// <summary>
    /// Web API 路由
    /// </summary>
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "Api",
                routeTemplate: "Api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
