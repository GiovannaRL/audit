using System;
using System.Collections.Generic;
using System.Linq;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;
using xPlannerAPI.Models;
using System.Data.Entity;
using xPlannerCommon.Services;
using System.Diagnostics;

namespace xPlannerAPI.Services
{
    public class RoomRepository : IRoomRepository
    {
        private readonly audaxwareEntities _db;
        private bool _disposed = false;

        public RoomRepository()
        {
            _db = new audaxwareEntities();
        }

        public List<project_room> GetMIS(int domain_id, int project_id, int? phase_id)
        {
            return _db.project_room.Where(pr => pr.domain_id == domain_id && pr.project_id == project_id &&
                (phase_id == null || phase_id == pr.phase_id) && pr.project_department.department_type.description.ToUpper()
                    .Equals("MIS")).OrderBy(pr => pr.drawing_room_number).ToList();
        }

        public List<room_inventory_po_Result> GetWithInventoryPO(int domain_id, int project_id, int phase_id, int department_id)
        {
            return _db.room_inventory_po(domain_id, project_id, phase_id, department_id).ToList();
        }

        public void SplitRoom(IEnumerable<SplitRoomData> data, short domain_id, int project_id, int phase_id, int department_id, int room_id, bool is_linked_template, string added_by, string template_name = "")
        {
            foreach (var item in data)
            {
                //var teste2 = "exec split_rooms " + domain_id + "," + item.project_id + "," + item.phase_id + "," + item.department_id + "," + domain_id + "," + project_id + "," + phase_id + "," + department_id + "," + room_id + "," + added_by + "," + is_linked_template + "," + template_name + "," + item.room_name + "," + item.room_number;
                _db.split_rooms(domain_id, item.project_id, item.phase_id, item.department_id, domain_id, project_id, phase_id, department_id, room_id, added_by, is_linked_template, template_name, item.room_name, item.room_number);
            }
        }

        public List<project_room> AddMultiRoom(IEnumerable<SplitRoomData> data, short domain_id, int project_id, int phase_id, int department_id, bool is_linked_template, string added_by, int? template_id)
        {
            var rooms = new List<project_room>();

            foreach (var item in data)
            {
                var room = new project_room
                {
                    domain_id = domain_id,
                    project_id = item.project_id,
                    phase_id = item.phase_id,
                    department_id = item.department_id,
                    drawing_room_name = item.room_name,
                    drawing_room_number = item.room_number,
                    final_room_name = item.room_name,
                    final_room_number = item.room_number,
                    date_added = DateTime.Now,
                    added_by = added_by,
                    applied_id_template = template_id,
                    linked_template = is_linked_template,
                    room_quantity = 1
                };

                if (is_linked_template)
                {
                    room.project_id_template = project_id;
                    room.project_domain_id_template = domain_id;
                }

                _db.project_room.Add(room);
                _db.SaveChanges();

                //apply templates
                if (template_id > 0)
                {
                    using (var rep = new TemplateRepository())
                    {
                        var template = new ApplyTemplateData
                        {
                            cost_field = "default",
                            delete_assets = true,
                            link_template = is_linked_template,
                            template_id = (int)template_id
                        };

                        rep.ApplyTemplate(template, domain_id, room.project_id, room.phase_id, room.department_id, room.room_id, room.added_by);
                    }
                }
                rooms.Add(room);
            }

            return rooms;

        }

        public void MoveAsset(IEnumerable<int> data, short domain_id, int project_id, int phase_id, int department_id, int room_id)
        {

            var inventories = _db.project_room_inventory.Where(x => data.Contains(x.inventory_id)).ToList();
            foreach (var item in inventories)
            {
                item.phase_id = phase_id;
                item.department_id = department_id;
                item.room_id = room_id;

                _db.Entry(item).State = EntityState.Modified;
            }
            _db.SaveChanges();

        }

        public void DeleteRoom(short domain_id, int project_id, int phase_id, int deparment_id, int room_id)
        {
            _db.delete_room(domain_id, project_id, phase_id, deparment_id, room_id);
        }

        private void InsertRoomPicutureAssociation(project_room room, FileData picture)
        {
            var userData = new AspNetUsersRepository();
            this._db.add_room_picture(room.domain_id, room.project_id, room.phase_id,
                room.department_id, room.room_id, picture.fileName,
                $"{picture.fileName}.{picture.fileExtension}", picture.fileExtension, userData.GetLoggedUserEmail());
        }

        public project_room GetRoomByNames(int domain_id, int project_id, string phase, string department, string room_number, string room_name)
        {
            // TODO(JLT): Define how we will manage phases
            return _db.project_room.Where(r => r.domain_id == domain_id && r.project_id == project_id
                    && r.drawing_room_number.ToLower() == room_number
                    && r.drawing_room_name.ToLower() == room_name
                    && r.project_department.description.ToLower() == department.ToLower())
                .FirstOrDefault();
        }

        public project_room InsertRoomByNames(int domain_id, int project_id, string phase, string department, string room_number, string room_name, string user)
        {
            project_phase phaseDB = null;
            project_department dptoDB = null;
            project_room roomDB = null;
            if (!string.IsNullOrEmpty(phase))
            {
                phaseDB = _db.project_phase.Include("project_department.project_room").FirstOrDefault(pp => pp.domain_id == domain_id && pp.project_id == project_id &&
                    pp.description.ToLower().Equals(phase.ToLower()));
            }

            // Check if the project already has a phase named "Unassigned'
            if (phaseDB == null)
            {

                dptoDB = _db.project_department.Include("project_room").FirstOrDefault(
                    pd => pd.domain_id == domain_id && pd.project_id == project_id &&
                          pd.description.ToLower().Equals(department.Trim().ToLower())
                );

                if (dptoDB == null)
                {
                    string description = "Unassigned";

                    phaseDB = _db.project_phase.FirstOrDefault(pp => pp.domain_id == domain_id && pp.project_id == project_id &&
                        pp.description.ToLower().Equals(description.ToLower()));

                    if (phaseDB == null)
                    {
                        // Insert phase
                        phaseDB = new project_phase()
                        {
                            domain_id = (short)domain_id,
                            project_id = project_id,
                            description = description,
                            start_date = DateTime.Now,
                            end_date = DateTime.Now,
                            added_by = user,
                            date_added = DateTime.Now
                        };

                        _db.Entry(phaseDB).State = EntityState.Added;
                        _db.SaveChanges();
                    }
                }
            }
            else
            {
                dptoDB = phaseDB.project_department.FirstOrDefault(
                    pd => pd.description.ToLower().Equals(department.Trim().ToLower())
                );
            }

            if (dptoDB == null)
            {
                // Insert department
                dptoDB = new project_department
                {
                    domain_id = (short)domain_id,
                    project_id = project_id,
                    phase_id = phaseDB.phase_id,
                    description = department,
                    department_type_id = 53,
                    department_type_domain_id = 1,
                    added_by = user,
                    date_added = DateTime.Now
                };

                _db.Entry(dptoDB).State = EntityState.Added;
                _db.SaveChanges();
            } else
            {
                roomDB = dptoDB.project_room.FirstOrDefault(pr => 
                    pr.drawing_room_number?.ToLower().Equals(room_number?.ToLower()) == true
                    && pr.drawing_room_name?.ToLower().Equals(room_name?.ToLower()) == true
                );
            }

            if (roomDB == null)
            {
                // Insert room
                roomDB = new project_room()
                {
                    domain_id = (short)domain_id,
                    project_id = project_id,
                    phase_id = dptoDB.phase_id,
                    department_id = dptoDB.department_id,
                    drawing_room_number = room_number,
                    drawing_room_name = room_name,
                    room_quantity = 1,
                    added_by = user,
                    date_added = DateTime.Now
                };
                _db.Entry(roomDB).State = EntityState.Added;
                _db.SaveChanges();
            }

            return roomDB;
        }

        public void UploadRoomPictures(project_room room, List<FileData> pictures)
        {
            using (var repositoryBlob = new FileStreamRepository())
            {
                string container = $"photo{room.domain_id}";
                if (pictures != null)
                {
                    foreach (FileData picture in pictures)
                    {
                        try
                        {
                            picture.fileName = repositoryBlob.UploadBase64Hashed(container, $"room_pic_{room.domain_id}_{room.project_id}",
                                picture.base64File, picture.fileExtension);
                            InsertRoomPicutureAssociation(room, picture);
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError($"Error to save room's picture " + picture.fileName, e);
                            throw e;
                        }
                    }
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