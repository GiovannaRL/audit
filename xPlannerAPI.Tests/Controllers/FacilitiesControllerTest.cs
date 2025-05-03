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
    public class FacilitiesControllerTest
    {
        private FacilitiesController controller;
        //private AuthController authController;

        public FacilitiesControllerTest()
        {
            HttpContext.Current = new HttpContext(new HttpRequest(null, "http://tempuri.org", null),
                    new HttpResponse(null));
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/auth/login");
            var route = config.Routes.MapHttpRoute("Default", "api/{controller}/{id}");
            controller = new FacilitiesController()
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
        public void GetAllFacilitiesByOneDomainHappyPath()
        {
            var ret = (HttpResponseMessage)controller.GetAll();
            ObjectContent objContent = (ObjectContent)ret.Content;
            List<facility> p = (List<facility>)objContent.Value;

            Assert.AreEqual(1, p.Find(x => x.id == 1).id);
        }

        [TestMethod]
        public void GetAllFacilitiesByDomainsHappyPath()
        {
            var ret = (HttpResponseMessage)controller.GetAll(3);
            ObjectContent objContent = (ObjectContent)ret.Content;
            List<facility> p = (List<facility>)objContent.Value;

            Assert.AreEqual("Seton Medical Center", p.Find(x => x.id == 1).name);
            Assert.AreEqual("Central Health", p.Find(x => x.id == 7).name);
        }

        [TestMethod]
        public void GetFacilityHappyPath()
        {
            //var ret = (HttpResponseMessage)controller.Get(4, 2);
            //ObjectContent objContent = (ObjectContent)ret.Content;
            //facility p = (facility)objContent.Value;
            //Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
            //Assert.AreEqual("Chevy Chase", p.name);
        }

        [TestMethod]
        public void GetFacilityNegativePath()
        {
            //var ret = (HttpResponseMessage)controller.Get(1, 2);
            //ObjectContent objContent = (ObjectContent)ret.Content;
            //facility p = (facility)objContent.Value;
            //Assert.IsNull(p);
        }

        [TestMethod]
        public void AddAndDeleteHappyPath()
        {
            //facility c = new facility();
            //c.name = "Facility 1";
            //c.domain_id = 1;

            //var ret = (HttpResponseMessage)controller.Add(c);
            //ObjectContent objContent = (ObjectContent)ret.Content;
            //c = (facility)objContent.Value;

            //Assert.IsNotNull(c.id);

            //controller.Delete(c.id, c.domain_id);
            //ret = (HttpResponseMessage)controller.Get(1, 2);
            //objContent = (ObjectContent)ret.Content;
            //facility c2 = (facility)objContent.Value;

            //Assert.IsNull(c2);
        }

        [TestMethod]
        public void PutHappyPath()
        {
            //var ret = (HttpResponseMessage)controller.Get(28, 3);
            //ObjectContent objContent = (ObjectContent)ret.Content;
            //facility c = (facility)objContent.Value;

            //c.name = "Test 1";

            //ret = (HttpResponseMessage)controller.Put(c.id, c.domain_id, c);

            //Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
        }
    }
}
