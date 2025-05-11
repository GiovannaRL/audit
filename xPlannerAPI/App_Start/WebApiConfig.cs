using System.Web.Http;
using Newtonsoft.Json.Serialization;

namespace xPlannerAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApiAction",
                routeTemplate: "api/{controller}/{action}/{id1}",
                defaults: new { id1 = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "GenericTableQuery",
                routeTemplate: "api/{controller}/{action}/{id1}/{id2}/{id3}/{id4}/{id5}/{id6}",
                defaults: new { id2 = RouteParameter.Optional, id3 = RouteParameter.Optional, id4 = RouteParameter.Optional, id5 = RouteParameter.Optional, id6 = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "MoreParams",
                routeTemplate: "api/{controller}/{action}/{id1}/{id2}/{id3}/{id4}/{id5}/{id6}/{id7}/{id8}/{id9}/{id10}/{id11}",
                defaults: new { id8 = RouteParameter.Optional, id9 = RouteParameter.Optional, id10 = RouteParameter.Optional, id11 = RouteParameter.Optional }
            );

            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            //config.Filters.Add(new AuthorizeAttribute());
            //jsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        }
    }
}
