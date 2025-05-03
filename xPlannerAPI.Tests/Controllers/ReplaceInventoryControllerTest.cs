using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xPlannerAPI;
using xPlannerAPI.Controllers;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Services;
using xPlannerAPI.Models;
using System.Net;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Net.Http;
using System.Web;

namespace xPlannerAPI.Tests.Controllers
{
    [TestClass]
    public class ReplaceInventoryControllerTest
    {
        private ReplaceInventoryController controller;
        //private AuthController authController;

        public ReplaceInventoryControllerTest()
        {
            HttpContext.Current = new HttpContext(new HttpRequest(null, "http://tempuri.org", null),
                    new HttpResponse(null));
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/auth/login");
            var route = config.Routes.MapHttpRoute("Default", "api/{controller}/{id}");
            controller = new ReplaceInventoryController()
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


        /*[TestMethod]
        public void ReplaceInventoryByRoomIdHappyPath()
        {
            //TODO(JBM): com autenticacao
            ReplaceInventory data = new ReplaceInventory();
            data.project_id = 129;
            data.phase_id = 255;
            data.department_id = 719;
            data.room_id = 12886;
            data.resp = null;
            data.budget = 0;
            data.cost_col = "default";
            data.old_code = 108259;
            data.old_domain_id = 1;
            data.new_code = 107720;
            data.new_domain_id = 1;

            var ret = controller.Put(data);
            Assert.AreEqual("", ret);

            data.old_code = 107720;
            data.new_code = 108259;

            ret = controller.Put(data);
            Assert.AreEqual("", ret);
        }*/

    }
}
