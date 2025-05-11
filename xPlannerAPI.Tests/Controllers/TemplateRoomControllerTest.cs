using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xPlannerAPI.Controllers;
using xPlannerCommon.Models;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Services;
using xPlannerAPI.Tests;
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
    public class TemplateRoomControllerTest: BaseTests
    {
        TemplateRoomController _controller;
        public TemplateRoomControllerTest()
        {
            _controller = CreateControler<TemplateRoomController>();

        }

        [TestMethod]
        public void TemplateControllerAddNegativeNull()
        {
            //Assert.AreEqual(_controller.Add(null, DomainId).StatusCode, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void TemplateControllerAddNegativeInvalidName()
        {
            var template = new project_room();
            //Assert.AreEqual(HttpStatusCode.BadRequest, _controller.Add(template, DomainId).StatusCode);
        }

        [TestMethod]
        public void TemplateControllerAddNegativeNoTemplateSpecified()
        {
            var template = new project_room();
            template.drawing_room_name = Guid.NewGuid().ToString();
            // This should cause it to fail, as the controller must enforce the item specified to be a template
            template.is_template = false;
            //Assert.AreEqual(HttpStatusCode.BadRequest, _controller.Add(template, DomainId).StatusCode);
        }

        [TestMethod]
        public void TemplateControllerAddSimple()
        {
            var template = new project_room();
            template.drawing_room_name = Guid.NewGuid().ToString();
            template.is_template = true;
            //Assert.AreEqual(_controller.Add(template, DomainId).StatusCode, HttpStatusCode.OK);
            var added = _db.project_room.Where(x => x.drawing_room_name == template.drawing_room_name);
            Assert.AreEqual(1, added.Count(), "Invalid count returned");
            Assert.AreEqual(true, added.FirstOrDefault().is_template, "Item added must be set as a template");
            _db.project_room.RemoveRange(_db.project_room.Where(x => x.drawing_room_name == template.drawing_room_name));
        }

    }
}
