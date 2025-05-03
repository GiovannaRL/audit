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
    public class RoomsControllerTest
    {
        private RoomsController controller;

        public RoomsControllerTest()
        {
            HttpContext.Current = new HttpContext(new HttpRequest(null, "http://tempuri.org", null),
                    new HttpResponse(null));
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/auth/login");
            var route = config.Routes.MapHttpRoute("Default", "api/{controller}/{id}");
            controller = new RoomsController()
            {
                Request = request,
            };
            controller.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;

        }

        //[TestMethod]
        //public void GetByRoomIdHappyPath()
        //{
        //    var ret = (HttpResponseMessage)controller.Get(7590, 115, 239, 494);
        //    ObjectContent objContent = (ObjectContent)ret.Content;
        //    ProjectDataReturn data = (ProjectDataReturn)objContent.Value;
        //    project_room pd = (project_room)data.details;

        //    Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
        //    Assert.AreEqual("CLEAN / STORAGE", pd.drawing_room_name);

        //}

        //[TestMethod]
        //public void AddAndDeleteRoomHappyPath()
        //{
        //    project_room pd = new project_room();
        //    pd.drawing_room_name = "Room test";
        //    pd.drawing_room_number = "123";
        //    pd.project_id = 115;
        //    pd.phase_id = 239;
        //    pd.department_id = 494;

        //    var ret = (HttpResponseMessage)controller.Add(pd);
        //    ObjectContent objContent = (ObjectContent)ret.Content;
        //    pd = (project_room)objContent.Value;

        //    Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
        //    Assert.IsNotNull(pd.room_id);

        //    ret = (HttpResponseMessage)controller.Delete(pd.room_id, pd.project_id, pd.phase_id, pd.department_id);

        //    Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);


        //}

        //[TestMethod]
        //public void UpdateRoomDetailHappyPath()
        //{
        //    var ret = (HttpResponseMessage)controller.Get(7590, 115, 239, 494);
        //    ObjectContent objContent = (ObjectContent)ret.Content;
        //    ProjectDataReturn projectData = (ProjectDataReturn)objContent.Value;
        //    project_room p = (project_room)projectData.details;
        //    string number = new Random(100).Next().ToString();
        //    p.final_room_name = number;

        //    var updateReturn = (HttpResponseMessage)controller.Put(7590, 115, 239, 494, p);
        //    Assert.AreEqual(HttpStatusCode.OK, updateReturn.StatusCode);
        //    objContent = (ObjectContent)ret.Content;
        //    projectData = (ProjectDataReturn)objContent.Value;
        //    p = (project_room)projectData.details;

        //    Assert.AreEqual(HttpStatusCode.OK, updateReturn.StatusCode);
        //    Assert.AreEqual(number, p.final_room_name);
        //}
    }
}
