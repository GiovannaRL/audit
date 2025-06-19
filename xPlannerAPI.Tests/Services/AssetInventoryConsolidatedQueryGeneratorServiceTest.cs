using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using xPlannerAPI.Models;
using xPlannerAPI.Services;
using xPlannerAPI.Tests;
using xPlannerCommon;
using xPlannerCommon.Models;

namespace xPlannerAPI.Tests.Repositories
{
    [TestClass]
    public class AssetInventoryConsolidatedQueryGeneratorServiceTest: BaseTests
    {
        private int _phaseIdAudaxWare;
        private int _departmentIdAudaxWare;
        private int _roomIdAudaxWare;

        public AssetInventoryConsolidatedQueryGeneratorServiceTest()
        {
            CreateProjectOptions.DomainId = 5;
        }

        private void ClearInventory()
        {
            CurrentDbContext.project_room_inventory.RemoveRange(
                CurrentDbContext.project_room_inventory.Where(
                    x => x.project_id == LastCreatedProjectId &&
                    x.domain_id == LastCreatedProjectDomainId
                    ));
        }

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            CreateProject();

            using (var tableRepo = new TableRepository<project_phase>())
            {
                var p = new project_phase();
                p.description = "Unit Test Phase";
                p.comment = "Project used for Unit tests only";
                p.domain_id = LastCreatedProjectDomainId;
                p.project_id = LastCreatedProjectId;
                p.added_by = CreateProjectOptions.AddedByUser;
                p = tableRepo.Add(p);
                _phaseIdAudaxWare = p.phase_id;
                p = new project_phase();
            }

            using (var tableRepo = new TableRepository<project_department>())
            {
                var p = new project_department();
                p.description = "Unit Test Dept";
                p.comment = "Project used for Unit tests only";
                p.domain_id = LastCreatedProjectDomainId;
                p.project_id = LastCreatedProjectId;
                p.added_by = CreateProjectOptions.AddedByUser;
                p.phase_id = _phaseIdAudaxWare;
                p.department_type_id = 26;
                p.department_type_domain_id = 1;
                p = tableRepo.Add(p);
                _departmentIdAudaxWare = p.department_id;
                p = new project_department();
            }

            using (var tableRepo = new TableRepository<project_room>())
            {
                var p = new project_room();
                p.drawing_room_name = "Unit Test Room";
                p.drawing_room_number = "1";
                p.comment = "Project used for Unit tests only";
                p.domain_id = LastCreatedProjectDomainId;
                p.project_id = LastCreatedProjectId;
                p.added_by = CreateProjectOptions.AddedByUser;
                p.phase_id = _phaseIdAudaxWare;
                p.department_id = _departmentIdAudaxWare;
                p = tableRepo.Add(p);
                _roomIdAudaxWare = p.room_id;
                p = new project_room();
            }

            AddInventories();
        }


        private void AddInventories()
        {
            ClearInventory();

            using (var tableRepo = new TableRepository<project_room_inventory>())
            {
                //empty cad id
                var p = new project_room_inventory();
                p.asset_id = 105345; //MAN00016
                p.asset_domain_id = 1;
                p.comment = "Project used for Unit tests only";
                p.domain_id = LastCreatedProjectDomainId;
                p.project_id = LastCreatedProjectId;
                p.added_by = CreateProjectOptions.AddedByUser;
                p.phase_id = _phaseIdAudaxWare;
                p.department_id = _departmentIdAudaxWare;
                p.room_id = _roomIdAudaxWare;
                p.resp = "OFOI";
                p.status = "A";
                p.current_location = "Plan";
                p.tag = null;
                p.cad_id = "";
                p = tableRepo.Add(p);
                
                //filled cad id
                p = new project_room_inventory();
                p.asset_id = 105345; //MAN00016
                p.asset_domain_id = 1;
                p.comment = "Project used for Unit tests only";
                p.domain_id = LastCreatedProjectDomainId;
                p.project_id = LastCreatedProjectId;
                p.added_by = CreateProjectOptions.AddedByUser;
                p.phase_id = _phaseIdAudaxWare;
                p.department_id = _departmentIdAudaxWare;
                p.room_id = _roomIdAudaxWare;
                p.resp = "OFOI";
                p.status = "A";
                p.current_location = "Plan";
                p.tag = null;
                p.cad_id = "ABC";
                p = tableRepo.Add(p);

                //filled tag
                p = new project_room_inventory();
                p.asset_id = 105345; //MAN00016
                p.asset_domain_id = 1;
                p.comment = "Project used for Unit tests only";
                p.domain_id = LastCreatedProjectDomainId;
                p.project_id = LastCreatedProjectId;
                p.added_by = CreateProjectOptions.AddedByUser;
                p.phase_id = _phaseIdAudaxWare;
                p.department_id = _departmentIdAudaxWare;
                p.room_id = _roomIdAudaxWare;
                p.resp = "OFOI";
                p.status = "A";
                p.current_location = "Plan";
                p.tag = "test";
                p.cad_id = "ABC";
                p = tableRepo.Add(p);

                //different resp
                p = new project_room_inventory();
                p.asset_id = 105345; //MAN00016
                p.asset_domain_id = 1;
                p.comment = "Project used for Unit tests only";
                p.domain_id = LastCreatedProjectDomainId;
                p.project_id = LastCreatedProjectId;
                p.added_by = CreateProjectOptions.AddedByUser;
                p.phase_id = _phaseIdAudaxWare;
                p.department_id = _departmentIdAudaxWare;
                p.room_id = _roomIdAudaxWare;
                p.resp = "OFCI";
                p.status = "A";
                p.current_location = "Plan";
                p.tag = "test";
                p.cad_id = "ABC";
                p = tableRepo.Add(p);
            }
        }

        private void Add2Inventories()
        {
            ClearInventory();

            using (var tableRepo = new TableRepository<project_room_inventory>())
            {
                //empty cad id
                var p = new project_room_inventory();
                p.asset_id = 105346; //MAN00017
                p.asset_domain_id = 1;
                p.comment = "Project used for Unit tests only";
                p.domain_id = LastCreatedProjectDomainId;
                p.project_id = LastCreatedProjectId;
                p.added_by = CreateProjectOptions.AddedByUser;
                p.phase_id = _phaseIdAudaxWare;
                p.department_id = _departmentIdAudaxWare;
                p.room_id = _roomIdAudaxWare;
                p.resp = "OFOI";
                p.status = "A";
                p.current_location = "Plan";
                p.tag = null;
                p.cad_id = "";
                p = tableRepo.Add(p);

                //filled cad id
                p = new project_room_inventory();
                p.asset_id = 105346; //MAN00017
                p.asset_domain_id = 1;
                p.comment = "Project used for Unit tests only";
                p.domain_id = LastCreatedProjectDomainId;
                p.project_id = LastCreatedProjectId;
                p.added_by = CreateProjectOptions.AddedByUser;
                p.phase_id = _phaseIdAudaxWare;
                p.department_id = _departmentIdAudaxWare;
                p.room_id = _roomIdAudaxWare;
                p.resp = "OFOI";
                p.status = "A";
                p.current_location = "Plan";
                p.tag = null;
                p.cad_id = "ABC";
                p = tableRepo.Add(p);
            }
        }

        [TestMethod]
        public void CreateQuery()
        {
            var repo = new AssetInventoryConsolidatedQueryGeneratorService();
            var result = repo.CreateQuery(5, 746, null, null, null, ConsolidatedQueryResults.groupBy1);
            Assert.AreEqual(ConsolidatedQueryResults.expectedResult1.Replace(" ", "").Replace("\r\n", ""), result.Replace(" ", ""));
        }

        [TestMethod]
        public void ValidateGrouping()
        {
            var repo = new AssetInventoryConsolidatedRepository();
            var result = repo.GetAll(LastCreatedProjectDomainId, LastCreatedProjectId, null, null, null, ConsolidatedQueryResults.groupBy1, false, false);
            Assert.IsTrue(result.Count() == 4);
        }

        [TestMethod]
        public void ValidateGroupingWithoutTag()
        {
            var repo = new AssetInventoryConsolidatedRepository();
            var result = repo.GetAll(LastCreatedProjectDomainId, LastCreatedProjectId, null, null, null, ConsolidatedQueryResults.groupByWithoutTag, false, false);
            Assert.IsTrue(result.Count() == 3);
        }

        [TestMethod]
        public void ValidateGroupingWithoutTagResp()
        {
            var repo = new AssetInventoryConsolidatedRepository();
            var result = repo.GetAll(LastCreatedProjectDomainId, LastCreatedProjectId, null, null, null, ConsolidatedQueryResults.groupByWithoutTagResp, false, false);
            Assert.IsTrue(result.Count() == 2);
        }

        [TestMethod]
        public void ValidateGroupingWithoutTagRespCadId()
        {
            var repo = new AssetInventoryConsolidatedRepository();
            var result = repo.GetAll(LastCreatedProjectDomainId, LastCreatedProjectId, null, null, null, ConsolidatedQueryResults.groupByWithoutTagRespCadId, false, false);
            Assert.IsTrue(result.Count() == 1);
        }


        [TestMethod]
        public void ComparedInventories()
        {
            Add2Inventories();
            var inventoryRepo = new AssetInventoryConsolidatedRepository();
            var inventories = inventoryRepo.GetAll(LastCreatedProjectDomainId, LastCreatedProjectId, null, null, null, ConsolidatedQueryResults.groupBy1, false, false).Where(x=> x.asset_code == "MAN00017").ToList();
            Assert.AreEqual(inventories.Count(), 2);
            var repo = new AssetInventoryConsolidatedQueryGeneratorService();
            var compareResult = repo.CompareInventories(ConsolidatedQueryResults.groupBy1, inventories[0], inventories[1]);
            Assert.AreEqual(compareResult.Count(), 1);
            Assert.AreEqual(compareResult[0].columnName, "cad_id");
        }
    }
}
