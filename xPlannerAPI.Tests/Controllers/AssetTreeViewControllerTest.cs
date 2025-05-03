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
    public class AssetTreeViewControllerTest
    {
        /*private AssetTreeViewsController controller;

        public AssetTreeViewControllerTest()
        {
            HttpContext.Current = new HttpContext(new HttpRequest(null, "http://tempuri.org", null),
                    new HttpResponse(null));
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/auth/login");
            var route = config.Routes.MapHttpRoute("Default", "api/{controller}/{id}");
            controller = new AssetTreeViewsController()
            {
                Request = request,
            };
            controller.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;

            
        }*/


        //[TestMethod]
        //public void GetAllAssetTreeViewByHSGUserHappyPath()
        //{
        //    var ret = (HttpResponseMessage)controller.GetAll();
        //    ObjectContent objContent = (ObjectContent)ret.Content;
        //    List<TreeViewItem> data = (List<TreeViewItem>)objContent.Value;

        //    Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
        //    Assert.IsNotNull(data.Find(x => x.id == 1).id);
        //    Assert.IsNotNull(data.Find(x => x.id == 3).id);
        //    Assert.IsTrue(data.Count == 2);
        //}

        //[TestMethod]
        //public void GetAllAssetTreeViewByAudaxwareUserHappyPath()
        //{
        //    authController.Get("juliana.barros@audaxware.com", "wendy1!");

        //    var ret = (HttpResponseMessage)controller.GetAll();
        //    ObjectContent objContent = (ObjectContent)ret.Content;
        //    List<TreeViewItem> data = (List<TreeViewItem>)objContent.Value;

        //    Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
        //    Assert.IsNotNull(data.Find(x => x.id == 1).id);
        //    Assert.IsNotNull(data.Find(x => x.id == 2).id);
        //    Assert.IsNotNull(data.Find(x => x.id == 3).id);
        //    Assert.IsTrue(data.Count > 3);
        //}

        //[TestMethod]
        //public void GetAllAssetTreeViewByDellUserHappyPath()
        //{
        //    authController.Get("test@dell.com", "preview1!");

        //    var ret = (HttpResponseMessage)controller.GetAll();
        //    ObjectContent objContent = (ObjectContent)ret.Content;
        //    List<TreeViewItem> data = (List<TreeViewItem>)objContent.Value;

        //    Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
        //    Assert.IsNotNull(data.Find(x => x.id == 2).id);
        //    Assert.IsTrue(data.Count == 1);
        //}

    }
}
