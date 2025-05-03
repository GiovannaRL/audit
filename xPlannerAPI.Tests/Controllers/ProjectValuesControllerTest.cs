using System;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xPlannerAPI.Controllers;
using xPlannerAPI.Models;
using System.Net;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web;
using xPlannerCommon.Models;

namespace xPlannerAPI.Tests.Controllers
{
    [TestClass]
    public class ProjectValuesControllerTest
    {
        private ProjectValuesController controller;

        public ProjectValuesControllerTest()
        {
            HttpContext.Current = new HttpContext(new HttpRequest(null, "http://tempuri.org", null),
                new HttpResponse(null));
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/auth/login");
            var route = config.Routes.MapHttpRoute("Default", "api/{controller}/{id}");
            controller = new ProjectValuesController()
            {
                Request = request,
            };
            controller.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;

        }

        [TestMethod]
        public void GetByProject()
        {
            var ret = (HttpResponseMessage)controller.Get(1, 115);
            ObjectContent objContent = (ObjectContent)ret.Content;
            matching_values mv = (matching_values)objContent.Value;
            Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
            Assert.AreEqual("$99,702,014.00", mv.total_projected);

            ret = (HttpResponseMessage)controller.Get(1, 1);
            Assert.AreEqual(HttpStatusCode.NotFound, ret.StatusCode);

        }

        [TestMethod]
        public void GetByPhase()
        {
            var ret = (HttpResponseMessage)controller.Get(115, 239);
            ObjectContent objContent = (ObjectContent)ret.Content;
            InventoryPoQtyVTotals mv = (InventoryPoQtyVTotals)objContent.Value;
            Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
            Assert.AreEqual((Decimal)2693560.46, mv.total_budget_amt);
        }

        [TestMethod]
        public void GetByDepartment()
        {
            var ret = (HttpResponseMessage)controller.Get(1, 115, null, 470);
            ObjectContent objContent = (ObjectContent)ret.Content;
            InventoryPoQtyVTotals mv = (InventoryPoQtyVTotals)objContent.Value;
            Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
            Assert.AreEqual((Decimal)3144191.49, mv.total_budget_amt);
        }

        [TestMethod]
        public void GetByRoom()
        {
            var ret = (HttpResponseMessage)controller.Get(1, 115, null, 470, 7187);
            ObjectContent objContent = (ObjectContent)ret.Content;
            InventoryPoQtyVTotals mv = (InventoryPoQtyVTotals)objContent.Value;
            Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
            Assert.AreEqual((Decimal)82300.42, mv.total_budget_amt);

            ret = (HttpResponseMessage)controller.Get(1, 115, null, null, 7187);
            Assert.AreEqual(HttpStatusCode.NotFound, ret.StatusCode);
        }
    }
}
