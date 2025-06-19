using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Services;
using xPlannerCommon.Models;
using xPlannerAPI.Models;
using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using System.Diagnostics;
using System;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsDetails)]
    public class RoomsController : TableGenericController<project_room>
    {
        public RoomsController() : base(new[] { "domain_id", "project_id", "phase_id", "department_id", "room_id" },
            new[] { "domain_id", "project_id", "phase_id", "department_id" }, new[] { "project_department.project_phase.project", "department_type" })
        { }

        protected override bool UpdateReferences(project_room item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {

            if (id5 == null || item.room_id <= 0)
            {
                item.copy_link = Guid.NewGuid();
            }

            return base.UpdateReferences(item, id1, id2, id3, id4, id5);

        }

        [ActionName("All")]
        public override IEnumerable<project_room> GetAll(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            return base.GetAll(id1, id2, id3, id4, id5).Where(pr => pr.is_template != true);
        }

        [ActionName("MIS")]
        public List<project_room> Get(int id1, int id2, int? id3 = null)
        {
            using (IRoomRepository repository = new RoomRepository())
            {
                return repository.GetMIS(id1, id2, id3);
            }
        }

        [ActionName("AllWithFinancials")]
        public IEnumerable<room_inventory_po_Result> GetAllWithFinancials(int id1, int id2, int id3, int id4)
        {
            using (IRoomRepository repository = new RoomRepository())
            {
                return repository.GetWithFinancials(id1, id2, id3, id4);
            }
        }

        [ActionName("SplitRoom")]
        public void SplitRoom([FromBody] IEnumerable<SplitRoomData> data, int id1, int id2, int id3, int id4, int id5, bool id6, string id7 = "")
        {
            using (IRoomRepository repository = new RoomRepository())
            {
                repository.SplitRoom(data, (short)id1, id2, id3, id4, id5, id6, UserName, id7);
            }
        }

        [ActionName("AddMultiRoom")]
        public UniqueString AddMultiRoom([FromBody] IEnumerable<SplitRoomData> data, int id1, int id2, int id3, int id4, bool id5, int? id6 = null)
        {
            using (IRoomRepository repository = new RoomRepository())
            {
                var rooms = repository.AddMultiRoom(data, (short)id1, id2, id3, id4, id5, UserName, id6);
                var retRooms = "";
                foreach (var item in rooms)
                {
                    retRooms = retRooms + "{ \"domain_id\": " + item.domain_id + ", \"room_id\": " + item.room_id + ", \"drawing_room_number\": \"" + item.drawing_room_number + "\", \"drawing_room_name\":  \"" + item.drawing_room_name + "\", \"room_quantity\": " + item.room_quantity + ", \"project_id\": " + item.project_id + ", \"phase_id\": " + item.phase_id + ", \"department_id\": " + item.department_id + "}||";
                }
                retRooms = retRooms.Substring(0, retRooms.Length - 2);
                var str = new UniqueString { text = retRooms };
                return str;
            }
        }

        [ActionName("Item")]
        public async Task<HttpResponseMessage> Delete(int id1, int id2, int id3, int id4, int id5)
        {

            using (var roomRepo = new TableRepository<project_room>())
            {
                var room = roomRepo.GetAll(new[] { "domain_id", "project_id", "phase_id", "department_id", "room_id" },
                    GetIds(id1, id2, id3, id4, id5), new[] { "project_room_inventory.inventory_purchase_order" });

                if (room == null) return Request.CreateResponse(HttpStatusCode.NotFound);

                if (room.Any(pr => pr.project_room_inventory.Any(pri => pri.inventory_purchase_order.Any(ipo => ipo.po_qty >= 1))))
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict,
                        "The room could not be deleted because purchase orders have been issued.");
                }
            }

            /* Get quote and po files name */
            var poFiles = ProjectFilesRepository.GetQuotePONames(id1, id2, id3, id4, id5);

            using (var roomRepo = new RoomRepository())
            {
                var oldRoom = base.GetItem(id1, id2, id3, id4, id5);

                roomRepo.DeleteRoom((short)id1, id2, id3, id4, id5);

                //AUDIT
                using (var auditRep = new AuditRepository())
                {
                    auditRep.CompareAndSaveAuditedData(oldRoom, null, "DELETE", new project_room());
                }
            }

            using (var reportRepository = new TableRepository<project_report>())
            {
                var reports = reportRepository.GetAll(new[] { "project_domain_id", "project_id", "phase_id", "department_id", "room_id" },
                    new[] { id1, id2, id3, id4, id5 }, new[] { "report_type" });

                var reportsFiles = reports.Select(pr =>
                        string.Format("{0}_{1}_{2}.xlsx", pr.name, pr.report_type.name.Replace(" ", "_"), pr.id));
                reportRepository.Delete(reports);

                // Delete files
                var webJobRepository = new WebjobRepository<Dictionary<int, Dictionary<string, List<string>>>>();
                await webJobRepository.SendMessage("delete-pos-and-quotes-files", new Dictionary<int, Dictionary<string, List<string>>> { { id1, poFiles } });

                var webJobRepository1 = new WebjobRepository<IEnumerable<string>>();
                await webJobRepository1.SendMessage("delete-report-files", reportsFiles);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [ActionName("MoveAsset")]
        public void MoveAsset([FromBody] IEnumerable<int> data, int id1, int id2, int id3, int id4, int id5)
        {
            using (IRoomRepository repository = new RoomRepository())
            {
                repository.MoveAsset(data, (short)id1, id2, id3, id4, id5);
            }
        }

        [ActionName("Pictures")]
        public List<PictureInfo> GetPictures(int id1, int id2, int id3, int id4, int id5)
        {
            using (IDocumentRepository docRepository = new DocumentRepository())
            {
                return docRepository.GetRoomPictures(id1, id2, id3, id4, id5);
            }
        }

        [HttpDelete]
        [ActionName("Picture")]
        //id6 = picture_id
        public HttpResponseMessage DeletePicture(int id1, int id2, int id3, int id4, int id5, int id6)
        {
            try
            {
                using (IDocumentRepository docRepository = new DocumentRepository())
                {
                    docRepository.DeleteRoomPicture(id1, id2, id3, id4, id5, id6);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            }
            catch (Exception)
            {
                Trace.TraceError($"Error to delete room's picture. Domain = {id1}, Project = {id2}, Phase = {id3}, Department = {id4}, Room = {id5}, Picture = {id6}");
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }

        [ActionName("Pictures")]
        public HttpResponseMessage PostPictures([FromBody] UploadRoomPicturesReq pictures, int id1, int id2, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (IRoomRepository roomRepository = new RoomRepository())
            {

                project_room room;

                if (id3 == null || id4 == null || id5 == null)
                {
                    room = roomRepository.GetRoomByNames(id1, id2, pictures.phaseName, pictures.departmentName, pictures.roomNumber, pictures.roomName);
                }
                else
                {
                    room = base.GetItem(id1, id2, id3, id4, id5);
                }

                if (room != null)
                {
                    roomRepository.UploadRoomPictures(room, pictures.pictures);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                else if (id3 == null || id4 == null || id5 == null) {
                    // Create the room and then upload de image
                    room = roomRepository.InsertRoomByNames(id1, id2, pictures.phaseName, pictures.departmentName, pictures.roomNumber, pictures.roomName, UserName);
                    roomRepository.UploadRoomPictures(room, pictures.pictures);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                } else
                {
                    Trace.TraceError($"No room found. Domain = {id1}, Project = {id2}, Phase = {pictures.phaseName}, Department = {pictures.departmentName}, Room = {pictures.roomNumber} - {pictures.roomName}");
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }
            }
        }


        [ActionName("SourceRoom")]
        public HttpResponseMessage GetSourceRoom(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (IRoomRepository repository = new RoomRepository())
            {
                var tableData = repository.GetRoomsAsTable(AudaxWareIdentity, id1, id2, id3, id4, id5);
                tableData.Select(r => new
                {
                    domain_id = id1,
                    project_id = id2,
                    phase_description = r.project_department.project_phase.description,
                    department_desc = r.department_id,
                    department_description = r.project_department.description,
                    room_name = r.drawing_room_name,
                    room_number = r.drawing_room_number
                }).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, tableData);
            }
        }


        [ActionName("LocationsTable")]
        public HttpResponseMessage GetRoomsTable(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (IRoomRepository repository = new RoomRepository())
            {
                var tableData = repository.GetRoomsAsTable(AudaxWareIdentity, id1, id2, id3, id4, id5);

                if (id4 != null)
                    return Request.CreateResponse(HttpStatusCode.OK, tableData.Select(r => new
                    {
                        domain_id = id1,
                        project_id = id2,
                        phase_id = id3,
                        department_id = id4,
                        room_id = r.room_id,
                        room_desc = r.drawing_room_name + (string.IsNullOrEmpty(r.drawing_room_number) ? "" : " - " + r.drawing_room_number) + (r.room_quantity > 1 ? " (" + r.room_quantity + ")" : "")
                    }));


                if (id3 != null)
                    return Request.CreateResponse(HttpStatusCode.OK, tableData.Select(r => new
                    {
                        domain_id = id1,
                        project_id = id2,
                        phase_id = id3,
                        department_id = r.department_id,
                        department_desc = r.project_department.description,
                        room_id = r.room_id,
                        room_desc = r.drawing_room_name + (string.IsNullOrEmpty(r.drawing_room_number) ? "" : " - " + r.drawing_room_number) + (r.room_quantity > 1 ? " (" + r.room_quantity + ")" : "")
                    }));

                if (id2 != null)
                    return Request.CreateResponse(HttpStatusCode.OK, tableData.Select(r => new
                    {
                        domain_id = id1,
                        project_id = id2,
                        phase_id = r.project_department.phase_id,
                        phase_desc = r.project_department.project_phase.description,
                        department_id = r.department_id,
                        department_desc = r.project_department.description,
                        room_id = r.room_id,
                        room_desc = r.drawing_room_name + (string.IsNullOrEmpty(r.drawing_room_number) ? "" : " - " + r.drawing_room_number) + (r.room_quantity > 1 ? " (" + r.room_quantity + ")" : "")
                    }));

                return Request.CreateResponse(HttpStatusCode.OK, tableData.Select(r => new
                {
                    domain_id = id1,
                    project_id = r.project_id,
                    project_desc = r.project_department.project_phase.project.project_description,
                    phase_id = r.phase_id,
                    phase_Desc = r.project_department.project_phase.description,
                    department_id = r.department_id,
                    department_desc = r.project_department.description,
                    room_id = r.room_id,
                    room_desc = r.drawing_room_name + (string.IsNullOrEmpty(r.drawing_room_number) ? "" : " - " + r.drawing_room_number) + (r.room_quantity > 1 ? " (" + r.room_quantity + ")" : "")
                }));

            }
        }

        [ActionName("unlinkedRooms")]
        public HttpResponseMessage GetUnlikedRooms(int id1, int id2, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (IRoomRepository repository = new RoomRepository())
            {
                var tableData = repository.GetRoomsAsTable(AudaxWareIdentity, id1, id2, id3, id4, id5).Where(r => r.linked_template != true);

                return Request.CreateResponse(HttpStatusCode.OK, tableData.Select(r => new
                {
                    domain_id = id1,
                    project_id = id2,
                    phase_id = r.phase_id,
                    phase_desc = r.project_department.project_phase.description,
                    department_id = r.department_id,
                    department_desc = r.project_department.description,
                    room_id = r.room_id,
                    room_desc = r.drawing_room_name + (string.IsNullOrEmpty(r.drawing_room_number) ? "" : " - " + r.drawing_room_number) + (r.room_quantity > 1 ? " (" + r.room_quantity + ")" : "")
                }));
            }
        }

        [ActionName("CopyRoom")]
        public HttpResponseMessage PostCopyRoom(int id1, int id2, bool id3, bool id4, [FromBody] List<CopyRoom> rooms)
        {
            using (IRoomRepository repository = new RoomRepository())
            {
                string addedBy = UserName;
                var success = repository.CopyRoom(id1, id2, id3, id4, addedBy, rooms);

                if (success)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, $"Rooms successfully {(id3 ? "copied" : "moved")}.");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, $"Error {(id3 ? "copying" : "moving")} room. Please contact technical support.");
                }
            }
        }

    }
}
