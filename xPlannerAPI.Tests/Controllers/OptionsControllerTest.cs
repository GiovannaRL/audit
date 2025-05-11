using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public class OptionsControllerTest
    {
        //private AssetOptionsController controller;

        //public OptionsControllerTest()
        //{
        //    HttpContext.Current = new HttpContext(new HttpRequest(null, "http://tempuri.org", null),
        //        new HttpResponse(null));
        //    var config = new HttpConfiguration();
        //    var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/");
        //    var request2 = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/auth/login");
        //    var route = config.Routes.MapHttpRoute("Default", "api/{controller}/{id}");
        //    controller = new AssetOptionsController()
        //    {
        //        Request = request,
        //    };
        //    controller.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;

        //}

        //[TestMethod]
        //public void GetAllOptionsHappyPath()
        //{
        //    OptionStructure ret = controller.GetInventory(105425, 1, 115, -1, -1, -1);

        //    Assert.IsTrue(ret.tree.Count > 0);
        //}

    }
}
