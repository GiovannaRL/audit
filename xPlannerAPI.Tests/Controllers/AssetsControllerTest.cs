using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xPlannerAPI.Controllers;
using System.Net;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Net.Http;
using System.Web;
using xPlannerCommon.Models;
using System.Linq;

namespace xPlannerAPI.Tests.Controllers
{
    [TestClass]
    public class AssetsControllerTest : BaseTests
    {
        private AssetsController _controller;

        public AssetsControllerTest()
        {
            _controller = CreateControler<AssetsController>();
        }

        [TestMethod]
        public void GetAllAssetsHappyPath()
        {
            var assets = _controller.GetAll(AudaxWareDomainId);
            Assert.IsTrue(assets.Count() > 0);
        }


        [TestMethod]
        public void GetAssetNegativePath()
        {
            var assets = _controller.GetAll(short.MaxValue);
            Assert.AreEqual(0, assets.Count());
        }
    }
}

