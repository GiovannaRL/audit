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
    /// <summary>
    /// Summary description for AssetCodeControllerTest
    /// </summary>
    [TestClass]
    public class AssetCodeControllerTest
    {
       private AssetCodesController controller;

        public AssetCodeControllerTest()
        {
            HttpContext.Current = new HttpContext(new HttpRequest(null, "http://tempuri.org", null),
                    new HttpResponse(null));
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/auth/login");
            var route = config.Routes.MapHttpRoute("Default", "api/{controller}/{id}");
            controller = new AssetCodesController()
            {
                Request = request,
            };
            controller.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;

        }


        //[TestMethod]
        //public void GetAllAssetCodesHappyPath()
        //{
        //    var ret = (HttpResponseMessage)controller.GetAll();
        //    ObjectContent objContent = (ObjectContent)ret.Content;
        //    List<equipment_codes> data = (List<equipment_codes>)objContent.Value;

        //    Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
        //    Assert.AreEqual("AID", data.Find(x => x.domain_id == 1).prefix);
        //}

        //[TestMethod]
        //public void GetAssetCodeHappyPath()
        //{
        //    var ret = (HttpResponseMessage)controller.Get("AID", 1);
        //    ObjectContent objContent = (ObjectContent)ret.Content;
        //    equipment_codes p = (equipment_codes)objContent.Value;
        //    Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
        //}

        //[TestMethod]
        //public void AddPutAndDeleteHappyPath()
        //{
        //    equipment_codes c = new equipment_codes();
        //    c.prefix = "XX1";
        //    c.domain_id = 3;
        //    c.description = "Teste Ju";

        //    var ret = (HttpResponseMessage)controller.Add(c);
        //    ObjectContent objContent = (ObjectContent)ret.Content;
        //    c = (equipment_codes)objContent.Value;

        //    Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);

        //    c.description = "T2";
        //    var c2 = controller.Put(c.prefix, c.domain_id, c);
        //    objContent = (ObjectContent)ret.Content;
        //    var data = (equipment_codes)objContent.Value;

        //    Assert.AreEqual("T2", data.description);

        //    controller.Delete(c.prefix, c.domain_id);
        //    ret = (HttpResponseMessage)controller.Get("XX1", 3);
        //    objContent = (ObjectContent)ret.Content;
        //    data = (equipment_codes)objContent.Value;

        //    Assert.IsNull(data);
        //}

    }
}
