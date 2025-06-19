using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Controllers;
using xPlannerCommon.Models;

namespace xPlannerAPI.Tests.Controllers
{
    [TestClass]
    public class AssetsInventoryConsolidatedControllerTest : BaseTestsProjectDefault
    {
        private AssetsInventoryConsolidatedController _controller;

        public AssetsInventoryConsolidatedControllerTest()
        {
            _controller = CreateControler<AssetsInventoryConsolidatedController>();
        }

        [TestMethod]
        public void ValidateApprovedAndUnapprovedAssetsForPO()
        {
            var assets = _controller.GetAll(LastCreatedProjectDomainId, LastCreatedProjectId, null, null, null, true, true, null).ToList();
            Assert.AreEqual(0, assets.Count, "The number of assets with status approved should be 0");

            assets = _controller.GetAll(LastCreatedProjectDomainId, LastCreatedProjectId, null, null, null, true, false, null).ToList();
            Assert.AreEqual(7, assets.Count, "The number of assets is incorrect");
        }
    }
}
