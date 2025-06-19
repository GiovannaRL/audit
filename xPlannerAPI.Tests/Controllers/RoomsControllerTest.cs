using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Hosting;
using xPlannerAPI.Controllers;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Models;
using xPlannerAPI.Services;
using xPlannerCommon;
using xPlannerCommon.Models;

namespace xPlannerAPI.Tests.Controllers
{
    [TestClass]
    public partial class RoomsControllerTest : BaseTestsProjectDefault
    {
        private RoomsController _controller;

        public RoomsControllerTest()
        {
           _controller = CreateControler<RoomsController>(); 
        }        
    }
}
