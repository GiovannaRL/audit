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
using System.Threading.Tasks;
using System.Web;
using System.IO;

namespace xPlannerData.Tests.Controllers
{
    [TestClass]
    public class PurchaseOrdersControllerTest
    {
        private PurchaseOrdersController controller;
        //private AuthController authController;

        public PurchaseOrdersControllerTest()
        {
            HttpContext.Current = new HttpContext(new HttpRequest(null, "http://tempuri.org", null),
                    new HttpResponse(null));
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/auth/login");
            var route = config.Routes.MapHttpRoute("Default", "api/{controller}/{id}");
            controller = new PurchaseOrdersController()
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
        public void GetAllPurchaseOrdersByProjectHappyPath()
        {
            var data = controller.GetAll(3, 115, null, null, null);

            Assert.IsTrue(data.Count > 0);
        }

        [TestMethod]
        public void GetAllPurchaseOrdersByProjectAndPhaseHappyPath()
        {
            var data = controller.GetAll(3, 115, 263, null, null);

            Assert.IsTrue(data.Count > 0);
        }

        [TestMethod]
        public void GetAllPurchaseOrdersByProjectAndDepartmentHappyPath()
        {
            var data = controller.GetAll(3, 115, 263, 470, null);

            Assert.IsTrue(data.Count > 0);
        }

        [TestMethod]
        public void GetAllPurchaseOrdersByProjectAndRoomHappyPath()
        {
            var data = controller.GetAll(3, 115, 263, 470, 7187);

            Assert.IsTrue(data.Count > 0);
        }

        /*[TestMethod]
        public void GetPurchaseOrderHappyPath()
        {
            var data = controller.Get(2256, 115);

            Assert.AreEqual("30050-00000-22209", data.po_number);
        }*/


    }
}
