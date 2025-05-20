using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xPlannerAPI.Controllers;
using xPlannerAPI.Models;
using xPlannerAPI.Services;
using xPlannerAPI.Tests.Repositories;
using xPlannerCommon;
using xPlannerCommon.Models;
using System.Data.Entity;
using System.Threading;

namespace xPlannerAPI.Tests.Controllers
{
    [TestClass]
    public class CopyFromControllerTest
    {
        private RoomsController _controller;
        private int _projectIdAudaxWare;
        private const string _testUser = "unittests@audaxware.com";
        private int _currentDomain = 1;
        private project _currentProject = null;
        private audaxwareEntities _db;
        private List<string> _fieldsToCheck = new List<string>();


        private void ContextEvent(object sender, ContextEventArgs args)
        {
            args.DomainId = _currentDomain;
            args.ShowAudaxwareInfo = true;
        }

        public CopyFromControllerTest()
        {
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/ws/api/");
            _controller = new RoomsController
            {
                Request = request
            };
            _controller.Request.Properties[System.Web.Http.Hosting.HttpPropertyKeys.HttpConfigurationKey] = config;
        }

        [TestInitialize]
        public void TestInitialize()
        {
            xPlannerCommon.SessionConnectionInterceptor.ContextEvent += ContextEvent;
            _db = new audaxwareEntities();
            _currentProject = CreateDefaultProject();
            ImportDefaultProject();
            GetFieldToCompare();
            _projectIdAudaxWare = _currentProject.project_id;
        }
               

        [TestCleanup]
        public void TestCleanup()
        {
            xPlannerCommon.SessionConnectionInterceptor.ContextEvent -= ContextEvent;
            ClearProject();
            _db.Dispose();
        }

        private project CreateDefaultProject()
        {
            using (var tableRepository = new TableRepository<project>())
            {
                var newProject = new project
                {
                    domain_id = (short)_currentDomain,
                    project_description = "Project for tests",
                    comment = "Project used for Unit tests only",
                    added_by = _testUser
                };

                tableRepository.Add(newProject);
                return newProject;
            }
        }

        private void ImportDefaultProject()
        {
            using (var repository = new AssetsInventoryImporterRepository())
            {
                var filePath = "xPlannerAPI.Tests.Repositories.TestData.ProjectDefault.xlsx";
                var file = EmbeddedResourceHelper.GetTempFilePathFromResource(filePath);
                var analysisResult = repository.Analyze(_currentDomain, _currentProject.project_id, file, Interfaces.ImportColumnsFormat.AudaxWare);

                if (analysisResult.First().Status == ImportAnalysisResultStatus.Ok)
                {
                    var validItems = analysisResult.First().Items.Where(x => x.Status != ImportItemStatus.Error).ToArray();
                    var importsResult = repository.Import(validItems, _currentDomain, _currentProject.project_id, _testUser, null);
                }
            }
        }

        private void ClearProject()
        {
            if (_currentProject == null)
                return;

            var projectId = _currentProject.project_id;
            var inventories = _db.project_room_inventory.Where(x => x.project_id == projectId).ToList();
            var rooms = _db.project_room.Where(x =>  x.domain_id == _currentDomain && x.project_id == projectId).ToList();
            var phases = _db.project_phase.Where(x => x.domain_id == _currentDomain && x.project_id == projectId).ToList();
            var departments = _db.project_department.Where(x => x.domain_id == _currentDomain && x.project_id == projectId).ToList();

            _db.project_room_inventory.RemoveRange(inventories);
            _db.project_room.RemoveRange(rooms);            
            _db.project_department.RemoveRange(departments);
            _db.SaveChanges();

            _db.project_phase.RemoveRange(phases);

            var projectToDelete = _db.projects
            .Where(x => x.domain_id == _currentDomain && x.project_id == projectId)
            .FirstOrDefault();

            if (projectToDelete != null)
            {
                _db.projects.Remove(projectToDelete);
                _db.SaveChanges(); 
            }

        }

        private void CompareAssets(project_room_inventory sourceAsset, project_room_inventory targetAsset)
        {                 
            foreach (var field in _fieldsToCheck)
            {
                var sourceValue = sourceAsset.GetType().GetProperty(field)?.GetValue(sourceAsset);
                var targetValue = targetAsset.GetType().GetProperty(field)?.GetValue(targetAsset);

                Assert.AreEqual(sourceValue, targetValue, $"Mismatch in property '{field}'");
            }
        }

        public void GetFieldToCompare()
        {
            var fieldNames = typeof(project_room_inventory)
                .GetProperties()
                .Select(p => p.Name)
                .ToList();

            var fieldsToIgnore = new List<string>
            {
                "project_id", "department_id", "room_id", "added_by", "cost_center_id", "phase_id", "inventory_id",
                "asset_it_connectivity", "asset_it_connectivity1", "asset", "bundle_inventory", "cost_center",
                "documents_associations", "inventory_documents", "inventory_options", "inventory_purchase_order",
                "project_documents", "project_room"
            };

            foreach (var field in fieldNames)
            {
                if (!fieldsToIgnore.Contains(field))
                {
                    _fieldsToCheck.Add(field);                    
                }
            }
        }

        [TestMethod]
        public void CopyRoomToExistingDepartment()
        {
            var sourceRoom = _db.project_room.Include("project_room_inventory").
                   Where(x => x.domain_id == _currentDomain && x.project_id == _currentProject.project_id && x.project_room_inventory.Count() > 0).FirstOrDefault();

            var targetDepartment = _db.project_department.Include("project_phase").
                Where(x => x.domain_id == _currentDomain && x.project_id == _currentProject.project_id && x.department_id != sourceRoom.department_id).FirstOrDefault();

            Assert.IsNotNull(sourceRoom, "Source room not found.");
            Assert.IsNotNull(targetDepartment, "Target department not found.");

            List<CopyRoom> actualCopy = new List<CopyRoom>
            {
                new CopyRoom
                {
                    source_project_id = sourceRoom.project_id,
                    source_phase_id = sourceRoom.phase_id,
                    source_department_id = sourceRoom.department_id,
                    source_room_id = sourceRoom.room_id,
                    phase_id = targetDepartment.phase_id,
                    phase_description = targetDepartment.project_phase.description,
                    department_id = targetDepartment.department_id,
                    department_description = targetDepartment.description,
                    room_name = sourceRoom.drawing_room_name,
                    room_number = sourceRoom.drawing_room_number,
                    added_by = _testUser
                }
            };
            
            var copyResult = _controller.PostCopyRoom(_currentDomain, _currentProject.project_id, true, false, actualCopy);

            Assert.AreEqual(copyResult.StatusCode, HttpStatusCode.OK, "The copy has not been successful.");

            var targetRoom = _db.project_room.Include("project_room_inventory").
                   Where(x => x.domain_id == _currentDomain && x.project_id == _currentProject.project_id &&
                    x.department_id == targetDepartment.department_id && x.drawing_room_name == sourceRoom.drawing_room_name).FirstOrDefault();

            foreach (var item in sourceRoom.project_room_inventory)
            {
                var targetItem = targetRoom.project_room_inventory.Where(x => x.asset_id == item.asset_id).FirstOrDefault();
                Assert.IsNotNull(targetItem, "Asset not found.");
                if (targetItem != null)
                {
                    CompareAssets(item, targetItem);
                }
            }
        }

        [TestMethod]
        public void CopyRoomToNewLocation()
        {
            var sourceRoom = _db.project_room.Include("project_room_inventory").
                   Where(x => x.domain_id == _currentDomain && x.project_id == _currentProject.project_id && x.project_room_inventory.Count() > 0).FirstOrDefault();

            Assert.IsNotNull(sourceRoom, "Source room not found.");

            List<CopyRoom> actualCopy = new List<CopyRoom>
            {
                new CopyRoom
                {
                    source_project_id = sourceRoom.project_id,
                    source_phase_id = sourceRoom.phase_id,
                    source_department_id = sourceRoom.department_id,
                    source_room_id = sourceRoom.room_id,
                    phase_id = -1,
                    phase_description = "Newphase",
                    department_id = -1,
                    department_description = "NewDepartment",
                    room_name = "CopiedRoom",
                    room_number = sourceRoom.drawing_room_number,
                    added_by = _testUser
                }
            };

            var copyResult = _controller.PostCopyRoom(_currentDomain, _currentProject.project_id, true, false, actualCopy);

            Assert.AreEqual(copyResult.StatusCode, HttpStatusCode.OK, "The copy has not been successful.");

            var targetRoom = _db.project_room.Include("project_room_inventory").
                  Where(x => x.domain_id == _currentDomain && x.project_id == _currentProject.project_id &&
                   x.drawing_room_name == "CopiedRoom").FirstOrDefault();

            foreach (var item in sourceRoom.project_room_inventory)
            {
                var targetItem = targetRoom.project_room_inventory.Where(x => x.asset_id == item.asset_id).FirstOrDefault();
                Assert.IsNotNull(targetItem, "Asset not found.");
                if (targetItem != null)
                {
                    CompareAssets(item, targetItem);
                }
            }
        }

        [TestMethod]
        public void MoveRoomToExistingDepartment()
        {
            var sourceRoom = _db.project_room.Include("project_room_inventory").
                   Where(x => x.domain_id == _currentDomain && x.project_id == _currentProject.project_id && x.project_room_inventory.Count() > 0).FirstOrDefault();

            var targetDepartment = _db.project_department.Include("project_phase").
                Where(x => x.domain_id == _currentDomain && x.project_id == _currentProject.project_id && x.department_id != sourceRoom.department_id).FirstOrDefault();

            Assert.IsNotNull(sourceRoom, "Source room not found.");

            List<CopyRoom> actualMove = new List<CopyRoom>
            {
                new CopyRoom
                {
                    source_project_id = sourceRoom.project_id,
                    source_phase_id = sourceRoom.phase_id,
                    source_department_id = sourceRoom.department_id,
                    source_room_id = sourceRoom.room_id,
                    phase_id = targetDepartment.phase_id,
                    phase_description = targetDepartment.project_phase.description,
                    department_id = targetDepartment.department_id,
                    department_description = targetDepartment.description,
                    room_name = "movedRoom",
                    room_number = "MR222",
                    added_by = _testUser
                }
            };

            var moveResult = _controller.PostCopyRoom(_currentDomain, _currentProject.project_id, false, false, actualMove);
            Assert.AreEqual(moveResult.StatusCode, HttpStatusCode.OK, "The move has not been successful.");

            var movedRoom = _db.project_room.Include("project_room_inventory").
                   Where(x => x.domain_id == _currentDomain && x.project_id == _currentProject.project_id && 
                   x.drawing_room_name == "movedRoom" && x.drawing_room_number == "MR222").FirstOrDefault();

            foreach (var item in sourceRoom.project_room_inventory)
            {
                var targetItem = movedRoom.project_room_inventory.Where(x => x.asset_id == item.asset_id).FirstOrDefault();
                Assert.IsNotNull(targetItem, "Asset not found.");
                if (targetItem != null)
                {
                    CompareAssets(item, targetItem);
                }
            }
            
            var stillExisting = _db.project_room.Include("project_room_inventory").
                   Where(x => x.domain_id == _currentDomain && x.project_id == _currentProject.project_id && x.room_id == sourceRoom.room_id).Any();

            _db.Entry(sourceRoom).State = EntityState.Detached;
        }

        [TestMethod]
        public void CopyRoomToInvalidDepartment()
        {
            var sourceRoom = _db.project_room.Include("project_room_inventory").
                   Where(x => x.domain_id == _currentDomain && x.project_id == _currentProject.project_id && x.project_room_inventory.Count() > 0).FirstOrDefault();

            Assert.IsNotNull(sourceRoom, "Source room not found.");

            List<CopyRoom> actualCopy = new List<CopyRoom>
            {
                new CopyRoom
                {
                    source_project_id = sourceRoom.project_id,
                    source_phase_id = sourceRoom.phase_id,
                    source_department_id = sourceRoom.department_id,
                    source_room_id = sourceRoom.room_id,
                    phase_id = 63645,
                    phase_description = "InvalidPhase",
                    department_id = 63784,
                    department_description = "InvalidDepartment",
                    room_name = "CopiedRoom",
                    room_number = sourceRoom.drawing_room_number,
                    added_by = _testUser
                }
            };

            var copyResult = _controller.PostCopyRoom(_currentDomain, _currentProject.project_id, true, false, actualCopy);
            Assert.AreNotEqual(copyResult.StatusCode, HttpStatusCode.OK, "The copy has been successful.");

        }
       


        
       


    }
}
