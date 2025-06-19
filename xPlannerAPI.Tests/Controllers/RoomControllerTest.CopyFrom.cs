using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Controllers;
using xPlannerAPI.Extensions;
using xPlannerAPI.Models;
using xPlannerCommon.Models;
using static xPlannerAPI.Extensions.ProjectRoomInventoryExtensions;

namespace xPlannerAPI.Tests.Controllers
{
    public partial class RoomsControllerTest
    {

        [TestMethod]
        public void CopyRoomToExistingDepartment()
        {
            var sourceRoom = CurrentDbContext.project_room.Include("project_room_inventory").
                   Where(x => x.domain_id == LastCreatedProjectDomainId && x.project_id == LastCreatedProjectId && x.project_room_inventory.Any()).FirstOrDefault();

            var targetDepartment = CurrentDbContext.project_department.Include("project_phase").
                Where(x => x.domain_id == LastCreatedProjectDomainId && x.project_id == LastCreatedProjectId && x.department_id != sourceRoom.department_id).FirstOrDefault();

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
                }
            };
            
            var copyResult = _controller.PostCopyRoom(LastCreatedProjectDomainId, LastCreatedProjectId, true, false, actualCopy);
            var differingFields = new List<string>();   

            Assert.AreEqual(copyResult.StatusCode, HttpStatusCode.OK, "The copy has not been successful.");

            var targetRoom = CurrentDbContext.project_room.Include("project_room_inventory").
                   Where(x => x.domain_id == LastCreatedProjectDomainId && x.project_id == LastCreatedProjectId &&
                    x.department_id == targetDepartment.department_id && x.drawing_room_name == sourceRoom.drawing_room_name).FirstOrDefault();

            foreach (var item in sourceRoom.project_room_inventory)
            {
                var targetItem = targetRoom.project_room_inventory.Where(x => x.asset_id == item.asset_id).FirstOrDefault();
                if (targetItem != null)
                {
                    item.AssertEqual(targetItem, IgnoreLocation.Project);
                }
            }
        }

        [TestMethod]
        public void CopyRoomToNewLocation()
        {
            var sourceRoom = CurrentDbContext.project_room.Include("project_room_inventory").
                   Where(x => x.domain_id == LastCreatedProjectDomainId && x.project_id == LastCreatedProjectId && x.project_room_inventory.Any()).FirstOrDefault();

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
                    added_by = CreateProjectOptions.AddedByUser
                }
            };

            var copyResult = _controller.PostCopyRoom(LastCreatedProjectDomainId, LastCreatedProjectId, true, false, actualCopy);

            Assert.AreEqual(copyResult.StatusCode, HttpStatusCode.OK, "The copy has not been successful.");

            var targetRoom = CurrentDbContext.project_room.Include("project_room_inventory").
                  Where(x => x.domain_id == LastCreatedProjectDomainId && x.project_id == LastCreatedProjectId &&
                   x.drawing_room_name == "CopiedRoom").FirstOrDefault();

            foreach (var item in sourceRoom.project_room_inventory)
            {
                var targetItem = targetRoom.project_room_inventory.Where(x => x.asset_id == item.asset_id).FirstOrDefault();
                Assert.IsNotNull(targetItem, "Asset not found.");
                if (targetItem != null)
                {
                    item.AssertEqual(targetItem, IgnoreLocation.Phase);
                }
            }
        }

        [TestMethod]
        public void MoveRoomToExistingDepartment()
        {
            var sourceRoom = CurrentDbContext.project_room.Include("project_room_inventory.cost_center").
                   Where(x => x.domain_id == LastCreatedProjectDomainId && x.project_id == LastCreatedProjectId && x.project_room_inventory.Any()).FirstOrDefault();



            var targetDepartment = CurrentDbContext.project_department.Include("project_phase").
                Where(x => x.domain_id == LastCreatedProjectDomainId && x.project_id == LastCreatedProjectId && x.department_id != sourceRoom.department_id).FirstOrDefault();

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
                    added_by = CreateProjectOptions.AddedByUser
                }
            };

            var moveResult = _controller.PostCopyRoom(LastCreatedProjectDomainId, LastCreatedProjectId, false, false, actualMove);
            Assert.AreEqual(moveResult.StatusCode, HttpStatusCode.OK, "The move has not been successful.");

            var movedRoom = CurrentDbContext.project_room.Include("project_room_inventory").
                   Where(x => x.domain_id == LastCreatedProjectDomainId && x.project_id == LastCreatedProjectId && 
                   x.drawing_room_name == "movedRoom" && x.drawing_room_number == "MR222").FirstOrDefault();

            foreach (var item in sourceRoom.project_room_inventory)
            {
                var targetItem = movedRoom.project_room_inventory.Where(x => x.asset_id == item.asset_id).FirstOrDefault();
                Assert.IsNotNull(targetItem, "Asset not found.");
                if (targetItem != null)
                {
                    item.AssertEqual(targetItem, IgnoreLocation.Project);
                }
            }
            
            var stillExisting = CurrentDbContext.project_room.Include("project_room_inventory").
                   Where(x => x.domain_id == LastCreatedProjectDomainId && x.project_id == LastCreatedProjectId && x.room_id == sourceRoom.room_id).Any();

            CurrentDbContext.Entry(sourceRoom).State = EntityState.Detached;
        }

        [TestMethod]
        public void CopyRoomToInvalidDepartment()
        {
            var sourceRoom = CurrentDbContext.project_room.Include("project_room_inventory").
                   Where(x => x.domain_id == LastCreatedProjectDomainId && x.project_id == LastCreatedProjectId && x.project_room_inventory.Any()).FirstOrDefault();

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
                    added_by = CreateProjectOptions.AddedByUser
                }
            };

            var copyResult = _controller.PostCopyRoom(LastCreatedProjectDomainId, LastCreatedProjectId, true, false, actualCopy);
            Assert.AreNotEqual(copyResult.StatusCode, HttpStatusCode.OK, "The copy has been successful.");

        }

        [TestMethod]
        public void CopyRoomToAnotherProjectWithCostCenter()
        {
            var sourceRoom = CurrentDbContext.project_room
            .Include("project_room_inventory")  
                .Include("project_room_inventory.cost_center")  
                    .Where(x => x.domain_id == LastCreatedProjectDomainId && x.project_id == LastCreatedProjectId && x.project_room_inventory.Any())
                        .FirstOrDefault();


            Assert.IsNotNull(sourceRoom, "Source room not found.");

            var newCostCenter = new cost_center
            {
                code = "CCTEST",
                description = "Cost Center Test",
                project_id = LastCreatedProjectId,
                domain_id = (short)LastCreatedProjectDomainId
            };

            base.CreateCostCenter(newCostCenter);
            var test = newCostCenter.id;

            foreach (var item in sourceRoom.project_room_inventory)
            {
                item.cost_center_id = newCostCenter.id;
                base.UpdateInventory(item);
            }

            CurrentDbContext.Entry(sourceRoom).Collection(r => r.project_room_inventory).Query()
            .Include(i => i.cost_center).Load();

            //NEW TARGET PROJECT
            var newProject = new CreateProjectOptions
            {
                DomainId = 5,
                AddedByUser = "unittests@audaxware.com",
                ProjectDescription = "Project for tests 02",
                ProjectComment = "Project 02 used for Unit tests only"
            };

            base.CreateProject(newProject);

            newCostCenter.id = 0;
            newCostCenter.project_id = LastCreatedProjectId;
            base.CreateCostCenter(newCostCenter);

            List<CopyRoom> actualCopy = new List<CopyRoom>
            {
                new CopyRoom
                {
                    source_project_id = sourceRoom.project_id,
                    source_phase_id = sourceRoom.phase_id,
                    source_department_id = sourceRoom.department_id,
                    source_room_id = sourceRoom.room_id,
                    phase_id = -1,
                    phase_description = "New Phase",
                    department_id = -1,
                    department_description = "New Department",
                    room_name = "New Room",
                    room_number = "11111",
                    added_by = CreateProjectOptions.AddedByUser
                }
            };

            var copyResult = _controller.PostCopyRoom(LastCreatedProjectDomainId, LastCreatedProjectId, true, false, actualCopy);
            var differingFields = new List<string>();

            Assert.AreEqual(copyResult.StatusCode, HttpStatusCode.OK, "The copy has not been successful.");

            var targetRoom = CurrentDbContext.project_room.Include("project_room_inventory.cost_center").
                   Where(x => x.domain_id == LastCreatedProjectDomainId && x.project_id == LastCreatedProjectId && x.project_room_inventory.Any()).FirstOrDefault();

            foreach (var item in sourceRoom.project_room_inventory)
            {
                var targetItem = targetRoom.project_room_inventory.Where(x => x.asset_id == item.asset_id).FirstOrDefault();
                Assert.IsNotNull(targetItem, "Asset not found.");
                if (targetItem != null)
                {

                    item.AssertEqual(targetItem, IgnoreLocation.Project);
                    Assert.AreNotEqual(item.cost_center_id, targetItem.cost_center_id, "The asset has been copied incorrectly.");
                    Assert.AreEqual(item.cost_center.code, targetItem.cost_center.code, "The asset has been copied incorrectly.");
                    
                }
            }
           
            base.ClearProject(LastCreatedProjectDomainId , LastCreatedProjectId);
        }

    }
}
