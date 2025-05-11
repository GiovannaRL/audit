using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xPlannerAPI.Controllers;
using System.Net;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Net.Http;
using System.Web;
using xPlannerCommon.Models;

namespace xPlannerAPI.Tests.Controllers
{
    [TestClass]
    public class AssetsControllerTest
    {
        private AssetsController controller;
        //private AuthController authController;

        public AssetsControllerTest()
        {
            HttpContext.Current = new HttpContext(new HttpRequest(null, "http://tempuri.org", null),
                new HttpResponse(null));
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/auth/login");
            var route = config.Routes.MapHttpRoute("Default", "api/{controller}/{id}");
            controller = new AssetsController()
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
        public void GetAllAssetsHappyPath()
        {
            var ret = (HttpResponseMessage)controller.GetAll(3);
            ObjectContent objContent = (ObjectContent)ret.Content;
            List<asset> eq = (List<asset>)objContent.Value;

            Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
            Assert.IsTrue(eq.Count>0);
        }


        [TestMethod]
        public void GetAssetHappyPath()
        {
            //var ret = (HttpResponseMessage)controller.GetItem(105345, 1);
            //ObjectContent objContent = (ObjectContent)ret.Content;
            //equipment data = (equipment)objContent.Value;
            //Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
            //Assert.AreEqual("MAN0016", data.equipment_code);
        }

        [TestMethod]
        public void GetAssetNegativePath()
        {
            //var ret = (HttpResponseMessage)controller.Get(110164, 3);
            //ObjectContent objContent = (ObjectContent)ret.Content;
            //equipment p = (equipment)objContent.Value;

            //Assert.IsNull(p);
        }

        [TestMethod]
        public void AddAndDeleteHappyPath()
        {
            //equipment c = new equipment();
            //c.subcategory_id = 3015;
            //c.manufacturer_id = 640;
            //c.manufacturer_domain_id = 1;
            //c.equipment_desc = "Test 1";
            //c.domain_id = 1;

            //var ret = (HttpResponseMessage)controller.Add(c);
            //ObjectContent objContent = (ObjectContent)ret.Content;
            //c = (equipment)objContent.Value;

            //Assert.IsNotNull(c.equipment_id);

            //controller.Delete(c.equipment_id, c.domain_id);
            //ret = (HttpResponseMessage)controller.Get(1, 2);
            //objContent = (ObjectContent)ret.Content;
            //equipment c2 = (equipment)objContent.Value;

            //Assert.IsNull(c2);
        }

        [TestMethod]
        public void PutHappyPath()
        {
            //var ret = (HttpResponseMessage)controller.Get(1, 3);
            //ObjectContent objContent = (ObjectContent)ret.Content;
            //equipment c = (equipment)objContent.Value;

            //c.comment = "Test 1";

            //ret = (HttpResponseMessage)controller.Put(c.equipment_id, c.domain_id, c);

            //Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
        }
    }
}

