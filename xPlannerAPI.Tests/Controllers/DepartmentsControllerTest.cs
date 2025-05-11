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
    public class DepartmentsControllerTest
    {
        private DepartmentsController controller;

        public DepartmentsControllerTest()
        {
            HttpContext.Current = new HttpContext(new HttpRequest(null, "http://tempuri.org", null),
                new HttpResponse(null));
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/auth/login");
            var route = config.Routes.MapHttpRoute("Default", "api/{controller}/{id}");
            controller = new DepartmentsController()
            {
                Request = request,
            };
            controller.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;

        }


        [TestMethod]
        public void GetByDepartmentIdHappyPath()
        {
            //var ret = (HttpResponseMessage)controller.Get(494, 115, 239);
            //ObjectContent objContent = (ObjectContent)ret.Content;
            //ProjectDataReturn data = (ProjectDataReturn)objContent.Value;
            //project_department pd = (project_department)data.details;

            //Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
            //Assert.IsNotNull(pd.phase_id);

        }

        [TestMethod]
        public void AddAndDeleteDepartmentHappyPath()
        {
            //audaxwareDB db = new audaxwareDB();
            //department_type dt = db.department_type.Find(22, 1);
            //project_department pd = new project_department();
            //pd.description = "Department test";
            //pd.project_id = 115;
            //pd.phase_id = 239;
            //pd.department_type = dt;

            //var ret = (HttpResponseMessage)controller.Add(pd);
            //ObjectContent objContent = (ObjectContent)ret.Content;
            //pd = (project_department)objContent.Value;

            //Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
            //Assert.IsNotNull(pd.department_id);

            //ret = (HttpResponseMessage)controller.Delete(pd.department_id, pd.project_id, pd.phase_id);

            //Assert.AreEqual(HttpStatusCode.OK, ret.StatusCode);
            

        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void DeleteDepartmentNegativePath()
        {
            //var ret2 = (HttpResponseMessage)controller.Delete(494, 115, 239);
            //Assert.Fail();
        }

        [TestMethod]
        public void UpdateDepartmentDetailHappyPath()
        {
            //var ret = (HttpResponseMessage)controller.Get(494, 115, 239);
            //ObjectContent objContent = (ObjectContent)ret.Content;
            //ProjectDataReturn projectData = (ProjectDataReturn)objContent.Value;
            //project_department p = (project_department)projectData.details;
            //string number = new Random(100).Next().ToString();
            //p.comment = number;

            //var updateReturn = (HttpResponseMessage)controller.Put(494, 115, 239, p);
            //Assert.AreEqual(HttpStatusCode.OK, updateReturn.StatusCode);
            //objContent = (ObjectContent)ret.Content;
            //projectData = (ProjectDataReturn)objContent.Value;
            //p = (project_department)projectData.details;

            //Assert.AreEqual(HttpStatusCode.OK, updateReturn.StatusCode);
            //Assert.AreEqual(number, p.comment);
        }
    }
}
