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
    public class ProjectsControllerTest
    {
        private ProjectsController controller;

        public ProjectsControllerTest()
        {
            HttpContext.Current = new HttpContext(new HttpRequest(null, "http://tempuri.org", null),
                new HttpResponse(null));
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/auth/login");
            var route = config.Routes.MapHttpRoute("Default", "api/{controller}/{id}");
            controller = new ProjectsController()
            {
                Request = request,
            };
            controller.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;

        }

        //[TestMethod]
        //public void GetByProjectIdHappyPath()
        //{
        //    var ret = (HttpResponseMessage)controller.Get(115);

        //    Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);

        //}

        //[TestMethod]
        //public void AddAndDeleteProjectHappyPath()
        //{
        //    project p = CreateNewProject();

        //    Assert.AreNotEqual(null, p.project_id);

        //    var ret2 = (HttpResponseMessage)controller.Delete(p.project_id);

        //    Assert.AreEqual(HttpStatusCode.OK, ret2.StatusCode);
        //}

        //[TestMethod]
        //[ExpectedException(typeof(ApplicationException))]
        //public void DeleteProjectNegativePath()
        //{
        //    var ret2 = (HttpResponseMessage)controller.Delete(3);
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void UpdateProjectDetailHappyPath()
        //{
        //    var ret = (HttpResponseMessage)controller.Get(115);
        //    ObjectContent objContent = (ObjectContent)ret.Content;
        //    ProjectDataReturn projectData = (ProjectDataReturn)objContent.Value;
        //    project p = (project)projectData.details;
        //    string number = new Random(100).Next().ToString();
        //    p.address1 = number;

        //    var updateReturn = (HttpResponseMessage)controller.Put(115, p);
        //    Assert.AreEqual(HttpStatusCode.OK, updateReturn.StatusCode);
        //    objContent = (ObjectContent)ret.Content;
        //    projectData = (ProjectDataReturn)objContent.Value;
        //    p = (project)projectData.details;

        //    Assert.AreEqual(number, p.address1);
        //}


        //private project CreateNewProject()
        //{
        //    audaxwareDB db = new audaxwareDB();
        //    client c = db.clients.Find(1,1);
        //    facility f = db.facilities.Find(1, 1);
        //    project p = new project();
            
        //    p.project_description = "Project Test JU";
        //    p.client = c;
        //    p.facility = f;

        //    var ret = (HttpResponseMessage)controller.Add(p);
        //    ObjectContent objContent = (ObjectContent)ret.Content;
        //    project projectData = (project)objContent.Value;

        //    return projectData;
        //}
    }
}
