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
    public class ClientsControllerTest
    {
        private ClientsController controller;
        //private AuthController authController;

        public ClientsControllerTest()
        {
            HttpContext.Current = new HttpContext(new HttpRequest(null, "http://tempuri.org", null),
                new HttpResponse(null));
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/auth/login");
            var route = config.Routes.MapHttpRoute("Default", "api/{controller}/{id}");
            controller = new ClientsController()
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
        public void GetAllClientsHappyPath()
        {
            var ret = (HttpResponseMessage)controller.GetAll();
            var objContent = (ObjectContent)ret.Content;
            List<client> p = (List<client>)objContent.Value;

            Assert.AreEqual("Hospitla Corporation of America (HCA)", p.Find(x => x.id == 1).name);
        }

        [TestMethod]
        public void GetAllClientsByDomainsHappyPath()
        {
            var ret = (HttpResponseMessage)controller.GetAll();
            ObjectContent objContent = (ObjectContent)ret.Content;
            List<client> p = (List<client>)objContent.Value;

            Assert.AreEqual("Beck", p.Find(x => x.id == 8).name);
            Assert.AreEqual("Seton Network Facilities", p.Find(x => x.id == 2).name);
        }

        [TestMethod]
        public void GetClientHappyPath()
        {
            //var ret = (HttpResponseMessage)controller.Get(4, 2);
            //ObjectContent objContent = (ObjectContent)ret.Content;
            //client p = (client)objContent.Value;
            //Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
            //Assert.AreEqual("Seton Network Facilities", p.name);
        }

        [TestMethod]
        public void GetClientNegativePath()
        {
            //var ret = (HttpResponseMessage)controller.Get(1, 2);
            //ObjectContent objContent = (ObjectContent)ret.Content;
            //client p = (client)objContent.Value;

            //Assert.IsNull(p);
        }

        [TestMethod]
        public void AddAndDeleteHappyPath()
        {
            //client c = new client();
            //c.name = "Client 1";
            //c.domain_id = 1;

            //var ret = (HttpResponseMessage)controller.Add(c);
            //ObjectContent objContent = (ObjectContent)ret.Content;
            //c = (client)objContent.Value;

            //Assert.IsNotNull(c.id);

            //controller.Delete(c.id, c.domain_id);
            //ret = (HttpResponseMessage)controller.Get(1, 2);
            //objContent = (ObjectContent)ret.Content;
            //client c2 = (client)objContent.Value;

            //Assert.IsNull(c2);
        }

        [TestMethod]
        public void PutHappyPath()
        {
            //var ret = (HttpResponseMessage)controller.Get(28, 3);
            //ObjectContent objContent = (ObjectContent)ret.Content;
            //client c = (client)objContent.Value;

            //c.name = "Test 1";

            //ret = (HttpResponseMessage)controller.Put(c.id, c.domain_id, c);

            //Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
        }
    }
}
