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
    public class PhasesControllerTest
    {

        private PhasesController controller;

        public PhasesControllerTest()
        {
            HttpContext.Current = new HttpContext(new HttpRequest(null, "http://tempuri.org", null),
                    new HttpResponse(null));
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/auth/login");
            var route = config.Routes.MapHttpRoute("Default", "api/{controller}/{id}");
            controller = new PhasesController()
            {
                Request = request,
            };
            controller.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;

        }

        //[TestMethod]
        //public void GetByPhaseIdHappyPath()
        //{
        //    var ret = (HttpResponseMessage)controller.Get(239, 115);

        //    Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);

        //}

        //[TestMethod]
        //public void AddAndDeletePhaseHappyPath()
        //{
        //    project_phase pp = new project_phase();
        //    pp.description = "Phase test";
        //    pp.start_date = DateTime.Now;
        //    pp.end_date = DateTime.Now.AddDays(30);
        //    pp.project_id = 115;

        //    var ret = (HttpResponseMessage)controller.Add(pp);
        //    ObjectContent objContent = (ObjectContent)ret.Content;
        //    pp = (project_phase)objContent.Value;

        //    Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
        //    Assert.IsNotNull(pp.phase_id);

        //    ret = (HttpResponseMessage)controller.Delete(pp.phase_id, pp.project_id);

        //    Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
            

        //}

        //[TestMethod]
        //[ExpectedException(typeof(ApplicationException))]
        //public void DeletePhaseNegativePath()
        //{
        //    var ret2 = (HttpResponseMessage)controller.Delete(239, 115);
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void UpdatePhaseDetailHappyPath()
        //{
        //    var ret = (HttpResponseMessage)controller.Get(239, 115);
        //    ObjectContent objContent = (ObjectContent)ret.Content;
        //    ProjectDataReturn projectData = (ProjectDataReturn)objContent.Value;
        //    project_phase p = (project_phase)projectData.details;
        //    string number = new Random(100).Next().ToString();
        //    p.comment = number;

        //    var updateReturn = (HttpResponseMessage)controller.Put(239, 115, p);
        //    Assert.AreEqual(HttpStatusCode.OK, updateReturn.StatusCode);
        //    objContent = (ObjectContent)ret.Content;
        //    projectData = (ProjectDataReturn)objContent.Value;
        //    p = (project_phase)projectData.details;

        //    Assert.AreEqual(HttpStatusCode.OK, updateReturn.StatusCode);
        //    Assert.AreEqual(number, p.comment);
        //}

    }
}
