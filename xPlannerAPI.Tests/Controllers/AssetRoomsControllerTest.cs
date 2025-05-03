using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xPlannerAPI.Controllers;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Net.Http;
using System.Web;
using xPlannerCommon.Models;

namespace xPlannerAPI.Tests.Controllers
{
    [TestClass]
    public class AssetRoomsControllerTest
    {
        private AssetRoomsController controller;
        //private AuthController authController;

        public AssetRoomsControllerTest()
        {
            HttpContext.Current = new HttpContext(new HttpRequest(null, "http://tempuri.org", null),
                new HttpResponse(null));
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/auth/login");
            var route = config.Routes.MapHttpRoute("Default", "api/{controller}/{id}");
            controller = new AssetRoomsController()
            {
                Request = request,
            };
            controller.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;

            //authController = new AuthController()
            //{
            //    Request = request2,
            //};
            //authController.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            //authController.Get("test@hsginc.org", "wendy1!");
        }

        [TestMethod]
        public void GetAllRoomsHappyPath()
        {
            const string equipment = "1082411,1059751";
            var ret = controller.Get(1, 115, -1, -1, -1, equipment);

            Assert.IsTrue(ret.Count > 0);
        }

    }
}
