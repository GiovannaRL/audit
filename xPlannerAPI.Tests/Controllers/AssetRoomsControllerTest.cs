using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xPlannerAPI.Controllers;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Net.Http;
using System.Web;
using xPlannerCommon.Models;
using System.Linq;

namespace xPlannerAPI.Tests.Controllers
{
    [TestClass]
    public class AssetRoomsControllerTest: BaseTestsProjectDefault
    {
        public AssetRoomsControllerTest()
        {
        }

        [TestMethod]
        public void GetAllRoomsHappyPath()
        {
            var inventory_ids = string.Join(", ", GetLastCreatedProjectInventory().Select(x => x.asset_id.ToString() + x.asset_domain_id.ToString()).ToArray());
            var controller = CreateControler<AssetRoomsController>();
            var ret = controller.Get(LastCreatedProjectDomainId, LastCreatedProjectId, -1, -1, -1, inventory_ids);
            Assert.IsTrue(ret.Count > 0);
        }

    }
}
