using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Controllers;

namespace xPlannerAPI.Tests.Controllers
{
    [TestClass]
    public class AssetsInventoryControllerTest : BaseTestsProjectDefault
    {
        private AssetsInventoryController _controller;

        public AssetsInventoryControllerTest()
        {
            _controller = CreateControler<AssetsInventoryController>();
        }

        [TestMethod]
        public void AssetsAvailableForPOShouldBeFilteredByApprovalStatus()
        {
            var assets = _controller.GetAllInventoriesAvailableForPO(LastCreatedProjectDomainId, LastCreatedProjectId, null, null, null, true).ToList();
            Assert.AreEqual(0, assets.Count, "The number of assets with status approved should be 0");

            assets = _controller.GetAllInventoriesAvailableForPO(LastCreatedProjectDomainId, LastCreatedProjectId, null, null, null, false).ToList();
            Assert.AreEqual(8, assets.Count, "Should return all assets without filter status");
        }

        [TestMethod]
        public void AssetsAvailableForPOShouldBeFilteredByOneApprovedStatus()
        {
            var inventories = CurrentDbContext.project_room_inventory.Where(x => x.domain_id == LastCreatedProjectDomainId && x.project_id == LastCreatedProjectId).ToList();
            inventories[0].current_location = "Approved";
            CurrentDbContext.Entry(inventories[0]).State = EntityState.Modified;
            CurrentDbContext.SaveChanges();


            var assets = _controller.GetAllInventoriesAvailableForPO(LastCreatedProjectDomainId, LastCreatedProjectId, null, null, null, true).ToList();
            Assert.AreEqual(1, assets.Count, "The number of assets with status approved should be 1");

            assets = _controller.GetAllInventoriesAvailableForPO(LastCreatedProjectDomainId, LastCreatedProjectId, null, null, null, false).ToList();
            Assert.AreEqual(8, assets.Count, "Should return all assets without filter status");
        }

    }
}
