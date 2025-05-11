using System;
using System.Linq;
using System.Diagnostics;
using xPlannerAPI.Models;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;
using System.Data.Entity.Core.Objects;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Collections.Generic;

namespace xPlannerAPI.Services
{
    public class CopyRoomRepository : ICopyRoomRepository
    {


        private readonly audaxwareEntities _db;
        private bool _disposed = false;

        public CopyRoomRepository()
        {
            _db = new audaxwareEntities();
        }

        public HttpResponseMessage Add(int fromDomainId, int fromProjectId, CopyRoom data)
        {
            if (data.to[0].department_id < 0)
            {
                var fromDepartment = _db.project_department.Find(data.from_project_id, data.from_department_id, data.from_phase_id, data.from_domain_id);
                project_department newDepartment = new project_department();
                newDepartment.project_id = data.to[0].project_id;
                newDepartment.department_id = 0;
                newDepartment.description = fromDepartment.description;
                newDepartment.department_type_id = fromDepartment.department_type_id;
                newDepartment.phase_id = data.to[0].phase_id;
                newDepartment.department_type_domain_id = fromDepartment.department_type_domain_id;
                newDepartment.domain_id = fromDepartment.domain_id;

                _db.project_department.Add(newDepartment);
                _db.SaveChanges();
                data.to[0].department_id = newDepartment.department_id;
            };


            var request = new HttpRequestMessage();
            var configuration = new HttpConfiguration();
            request.SetConfiguration(configuration);

            using (var dbTransaction = _db.Database.BeginTransaction())
            {
                try
                {
                    data.from_domain_id = fromDomainId;
                    data.from_project_id = fromProjectId;


                    var room = _db.project_room.Find(data.from_project_id, data.from_department_id, data.from_room_id, data.from_phase_id, data.from_domain_id);
                    var addedRoom = new List<project_room>();

                    foreach (var item in data.to_room_number_name)
                    {
                        foreach (var project in data.to)
                        {
                            ObjectParameter return_var = new ObjectParameter("return_var", "");
                            _db.Database.CommandTimeout = 600000;
                            _db.copy_rooms(project.domain_id, project.project_id, project.phase_id, project.department_id, (short)data.from_domain_id, data.from_project_id, data.from_phase_id, data.from_department_id, data.from_room_id, data.added_by, data.copy_options_colors, item.Item2, item.Item1, null, true, false, data.move, return_var);
                            addedRoom.Add(_db.project_room.Where(x => x.domain_id == project.domain_id && x.project_id == project.project_id && x.phase_id == project.phase_id && x.department_id == project.department_id && x.drawing_room_number == item.Item1 && x.drawing_room_name == item.Item2 && x.added_by == data.added_by).FirstOrDefault());
                        }
                    }

                    if (data.move)
                    {
                        _db.project_room.Remove(room);
                        _db.SaveChanges();
                    }

                    dbTransaction.Commit();
                    return request.CreateResponse(HttpStatusCode.OK, addedRoom.Last());
                }
                catch (Exception e)
                {
                    Trace.TraceError($"Error to copy room. Exception {e.Message}");
                    dbTransaction.Rollback();
                    return request.CreateResponse(HttpStatusCode.Conflict, "Error to copy/move room, please contact technical support");
                }
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _db.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}