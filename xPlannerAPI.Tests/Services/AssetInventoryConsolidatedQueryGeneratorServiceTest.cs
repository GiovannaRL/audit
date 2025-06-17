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
    public class AssetInventoryConsolidatedQueryGeneratorServiceTest
    {
        private int _projectIdAudaxWare;
        private int _phaseIdAudaxWare;
        private int _departmentIdAudaxWare;
        private int _roomIdAudaxWare;
        const string _projectDescription = "AssetInventoryConsolidatedTests-F0635B72-447F-4CE5-AB9F-BEF20105FDC2";
        const string _importUser = "utconsolidated@audaxware.com";
        int _awDomain = 1;
        int _currentDomain = 3;
        audaxwareEntities _db;
        void ContextEvent(object sender, ContextEventArgs args)
        {
            args.DomainId = _awDomain;
            args.ShowAudaxwareInfo = true;
        }

        void ClearProject()
        {
            ClearProject(false);
        }

        void ClearProject(bool assetsOnly)
        {
            // If project already exists, then we delete it. Cannot use the controller to delete
            // as the controller only marks the project status as deleted
            var testProjects = _db.projects.Where(x => x.project_description == _projectDescription).ToList();
            foreach (var p in testProjects)
            {
                _db.project_room_inventory.RemoveRange(_db.project_room_inventory.Where(x => x.project_id == p.project_id));
                if (!assetsOnly)
                {
                    _db.projects.Remove(p);
                }
            }

            _db.assets.RemoveRange(_db.assets.Where(x => x.added_by == _importUser));
            _db.assets_category.RemoveRange(_db.assets_category.Where(x => x.added_by == _importUser));
            _db.assets_subcategory.RemoveRange(_db.assets_subcategory.Where(x => x.added_by == _importUser));
            _db.project_room.RemoveRange(_db.project_room.Where(x => x.added_by == _importUser));
            _db.project_department.RemoveRange(_db.project_department.Where(x => x.added_by == _importUser));
            _db.project_phase.RemoveRange(_db.project_phase.Where(x => x.added_by == _importUser));
            _db.manufacturers.RemoveRange(_db.manufacturers.Where(x => x.added_by == _importUser));

            if (testProjects.Count > 0)
            {
                _db.SaveChanges();
            }

        }

        private void ClearInventory()
        {
            _db.project_room_inventory.RemoveRange(_db.project_room_inventory.Where(x => x.project_id == _projectIdAudaxWare));
        }

        [TestInitialize]
        public void Initialize()
        {
            xPlannerCommon.SessionConnectionInterceptor.ContextEvent += ContextEvent;
            _db = new audaxwareEntities();
            _db.Database.CommandTimeout = 20000000;


            //create project, phase, departmnet, room, asset, inventories
            //aí nos inventories colocar os campos importantes que podem dar problema diferentes e iguais, pra conseguir agrupar ou nao.
            //E ai criar testes validando quantidade de itens agrupados 
            ClearProject();
            using (var tableRepo = new TableRepository<project>())
            {
                var p = new project();
                p.project_description = _projectDescription;
                p.comment = "Project used for Unit tests only";
                p.domain_id = 3;
                p.added_by = _importUser;
                p = tableRepo.Add(p);
                _projectIdAudaxWare = p.project_id;
                p = new project();
            }

            using (var tableRepo = new TableRepository<project_phase>())
            {
                var p = new project_phase();
                p.description = "Unit Test Phase";
                p.comment = "Project used for Unit tests only";
                p.domain_id = 3;
                p.project_id = _projectIdAudaxWare;
                p.added_by = _importUser;
                p = tableRepo.Add(p);
                _phaseIdAudaxWare = p.phase_id;
                p = new project_phase();
            }

            using (var tableRepo = new TableRepository<project_department>())
            {
                var p = new project_department();
                p.description = "Unit Test Dept";
                p.comment = "Project used for Unit tests only";
                p.domain_id = 3;
                p.project_id = _projectIdAudaxWare;
                p.phase_id = _phaseIdAudaxWare;
                p.department_type_id = 26;
                p.department_type_domain_id = 1;
                p.added_by = _importUser;
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
                p.domain_id = 3;
                p.project_id = _projectIdAudaxWare;
                p.phase_id = _phaseIdAudaxWare;
                p.department_id = _departmentIdAudaxWare;
                p.added_by = _importUser;
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
                p.domain_id = 3;
                p.project_id = _projectIdAudaxWare;
                p.phase_id = _phaseIdAudaxWare;
                p.department_id = _departmentIdAudaxWare;
                p.room_id = _roomIdAudaxWare;
                p.resp = "OFOI";
                p.status = "A";
                p.current_location = "Plan";
                p.tag = null;
                p.cad_id = "";
                p.added_by = _importUser;
                p = tableRepo.Add(p);
                
                //filled cad id
                p = new project_room_inventory();
                p.asset_id = 105345; //MAN00016
                p.asset_domain_id = 1;
                p.comment = "Project used for Unit tests only";
                p.domain_id = 3;
                p.project_id = _projectIdAudaxWare;
                p.phase_id = _phaseIdAudaxWare;
                p.department_id = _departmentIdAudaxWare;
                p.room_id = _roomIdAudaxWare;
                p.resp = "OFOI";
                p.status = "A";
                p.current_location = "Plan";
                p.tag = null;
                p.cad_id = "ABC";
                p.added_by = _importUser;
                p = tableRepo.Add(p);

                //filled tag
                p = new project_room_inventory();
                p.asset_id = 105345; //MAN00016
                p.asset_domain_id = 1;
                p.comment = "Project used for Unit tests only";
                p.domain_id = 3;
                p.project_id = _projectIdAudaxWare;
                p.phase_id = _phaseIdAudaxWare;
                p.department_id = _departmentIdAudaxWare;
                p.room_id = _roomIdAudaxWare;
                p.resp = "OFOI";
                p.status = "A";
                p.current_location = "Plan";
                p.tag = "test";
                p.cad_id = "ABC";
                p.added_by = _importUser;
                p = tableRepo.Add(p);

                //different resp
                p = new project_room_inventory();
                p.asset_id = 105345; //MAN00016
                p.asset_domain_id = 1;
                p.comment = "Project used for Unit tests only";
                p.domain_id = 3;
                p.project_id = _projectIdAudaxWare;
                p.phase_id = _phaseIdAudaxWare;
                p.department_id = _departmentIdAudaxWare;
                p.room_id = _roomIdAudaxWare;
                p.resp = "OFCI";
                p.status = "A";
                p.current_location = "Plan";
                p.tag = "test";
                p.cad_id = "ABC";
                p.added_by = _importUser;
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
                p.domain_id = 3;
                p.project_id = _projectIdAudaxWare;
                p.phase_id = _phaseIdAudaxWare;
                p.department_id = _departmentIdAudaxWare;
                p.room_id = _roomIdAudaxWare;
                p.resp = "OFOI";
                p.status = "A";
                p.current_location = "Plan";
                p.tag = null;
                p.cad_id = "";
                p.added_by = _importUser;
                p = tableRepo.Add(p);

                //filled cad id
                p = new project_room_inventory();
                p.asset_id = 105346; //MAN00017
                p.asset_domain_id = 1;
                p.comment = "Project used for Unit tests only";
                p.domain_id = 3;
                p.project_id = _projectIdAudaxWare;
                p.phase_id = _phaseIdAudaxWare;
                p.department_id = _departmentIdAudaxWare;
                p.room_id = _roomIdAudaxWare;
                p.resp = "OFOI";
                p.status = "A";
                p.current_location = "Plan";
                p.tag = null;
                p.cad_id = "ABC";
                p.added_by = _importUser;
                p = tableRepo.Add(p);
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            xPlannerCommon.SessionConnectionInterceptor.ContextEvent -= ContextEvent;
            ClearProject();
            _db.Dispose();
        }


        [TestMethod]
        public void CreateQuery()
        {
            var repo = new AssetInventoryConsolidatedQueryGeneratorService();
            var result = repo.CreateQuery(5, 746, null, null, null, ConsolidatedQueryResults.groupBy1);
            Assert.AreEqual(ConsolidatedQueryResults.expectedResult1.Replace(" ", "").Replace("\r\n", ""), result.Replace(" ", ""));

        }


        [TestMethod]
        public void ReducedGroupBy()
        {
            var repo = new AssetInventoryConsolidatedRepository();
            var newProcResult = repo.GetAll(3, 1182, null, null, null, ConsolidatedQueryResults.groupByReduced, false, false);

            Assert.IsTrue(newProcResult.Count > 0);

        }

        [TestMethod]
        public void GroupByWithColumnsToIgnoreAndRequired()
        {
            var repo = new AssetInventoryConsolidatedRepository();
            var newProcResult = repo.GetAll(5, 746, null, null, null, ConsolidatedQueryResults.groupByWithColumnsToIgnoreAndRequired, false, false);

            Assert.AreEqual(newProcResult.First().consolidated_view, 1);
            Assert.AreEqual(newProcResult.First().inventory_id, 0);

        }


        [TestMethod]
        public void ValidateGrouping()
        {
            var repo = new AssetInventoryConsolidatedRepository();
            var result = repo.GetAll(_currentDomain, _projectIdAudaxWare, null, null, null, ConsolidatedQueryResults.groupBy1, false, false);
            Assert.IsTrue(result.Count() == 4);
        }

        [TestMethod]
        public void ValidateGroupingWithoutTag()
        {
            var repo = new AssetInventoryConsolidatedRepository();
            var result = repo.GetAll(_currentDomain, _projectIdAudaxWare, null, null, null, ConsolidatedQueryResults.groupByWithoutTag, false, false);
            Assert.IsTrue(result.Count() == 3);
        }

        [TestMethod]
        public void ValidateGroupingWithoutTagResp()
        {
            var repo = new AssetInventoryConsolidatedRepository();
            var result = repo.GetAll(_currentDomain, _projectIdAudaxWare, null, null, null, ConsolidatedQueryResults.groupByWithoutTagResp, false, false);
            Assert.IsTrue(result.Count() == 2);
        }

        [TestMethod]
        public void ValidateGroupingWithoutTagRespCadId()
        {
            var repo = new AssetInventoryConsolidatedRepository();
            var result = repo.GetAll(_currentDomain, _projectIdAudaxWare, null, null, null, ConsolidatedQueryResults.groupByWithoutTagRespCadId, false, false);
            Assert.IsTrue(result.Count() == 1);
        }


        [TestMethod]
        public void ComparedInventories()
        {
            Add2Inventories();
            var inventoryRepo = new AssetInventoryConsolidatedRepository();
            var inventories = inventoryRepo.GetAll(_currentDomain, _projectIdAudaxWare, null, null, null, ConsolidatedQueryResults.groupBy1, false, false).Where(x=> x.asset_code == "MAN00017").ToList();
            Assert.AreEqual(inventories.Count(), 2);
            var repo = new AssetInventoryConsolidatedQueryGeneratorService();
            var compareResult = repo.CompareInventories(ConsolidatedQueryResults.groupBy1, inventories[0], inventories[1]);
            Assert.AreEqual(compareResult.Count(), 1);
            Assert.AreEqual(compareResult[0].columnName, "cad_id");
        }

        
    }
}
