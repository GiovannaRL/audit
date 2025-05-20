using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;
using xPlannerCommon.App_Data;
using xPlannerAPI.Security.Extensions;
using xPlannerAPI.Models;
using System.Text;

namespace xPlannerAPI.Services
{
    public class TreeViewRepository : ITreeViewRepository
    {
        private readonly audaxwareEntities _db;
        private bool _disposed = false;

        public TreeViewRepository()
        {
            this._db = new audaxwareEntities();
        }

        public List<ProjectTreeItems> MountTree(int domainId, ClaimsIdentity identity, String UserId)
        {
            try
            {
                var allTreeItems = GetAll(domainId, identity);
                var mineProject = new ProjectMineRepository();
                var myProjects = mineProject.GetMyProjects(domainId, UserId);
                var myProjectsSet = new SortedSet<int>();
                List<ProjectTreeItems> allProjects = new List<ProjectTreeItems>();
                var lastRowAdded = new get_project_treeview_Result();
                ProjectTreeItems currentProject = null;
                ProjectTreeItems currentPhase = null;
                ProjectTreeItems currentDepartment = null;
                ProjectTreeItems currentRoom = null;

                foreach (var p in myProjects)
                {
                    myProjectsSet.Add(p.project_id);
                }

                foreach (var p in allTreeItems)
                {
                    if (lastRowAdded.project_id != p.project_id)
                    {                       
                        currentProject = new ProjectTreeItems(p.project_id, p.domain_id, p.project_name, myProjectsSet.Contains(p.project_id), p.status);
                        allProjects.Add(currentProject);
                    }


                    if (p.phase_id != null && (lastRowAdded.phase_id != p.phase_id))
                    {
                        currentPhase = new ProjectTreeItems((int)p.phase_id, p.phase_description);
                        currentProject.items.Add(currentPhase);
                    }

                    if (p.department_id != null && lastRowAdded.department_id != p.department_id)
                    {
                        currentDepartment = new ProjectTreeItems((int)p.department_id, p.department_description);
                        currentPhase.items.Add(currentDepartment);
                    }

                    if (p.room_id != null)
                    {
                        currentRoom = new ProjectTreeItems((int)p.room_id, p.room_name);
                        currentDepartment.items.Add(currentRoom);
                    }

                    lastRowAdded = p;
                }

                return allProjects;

            }
            catch (Exception ex)
            {
                Helper.RecordLog("TreeViewRepository", "MountTree", ex);
                throw new ApplicationException(ex.Message);
            }       
        }


        public List<get_project_treeview_Result> GetAll(int domainId, ClaimsIdentity identity)
        {
            try
            {
                var domainProjects = _db.get_project_treeview(domainId).ToList();
                var userProjects =  from p in domainProjects where identity.CheckProjectAccess(domainId, p.project_id) select p;

                return userProjects.ToList();
            }
            catch (Exception ex)
            {
                Helper.RecordLog("TreeViewRepository", "GetAll", ex);
                throw new ApplicationException(ex.Message);
            }
        }


        protected int GetQtyEquipment(int assetId, int domainId, int projectId, int? phaseId = null, int? departmentId = null, int? roomId = null, string room_name = null)
        {
            var total = _db.project_room_inventory.Include("project_room")
                .Where(x => x.project_id == projectId && x.asset_id == assetId && x.domain_id == domainId);

            if (phaseId != null)
                total = total.Where(x => x.phase_id == phaseId);
            if (departmentId != null)
                total = total.Where(x => x.department_id == departmentId);
            if (roomId != null)
                total = total.Where(x => x.room_id == roomId);
            if (room_name != null)
                total = total.Where(x => x.project_room.drawing_room_name == room_name);

            var total2 = total.Sum(x => x.budget_qty).Value;

            return total2;
        }


       /* protected TreeViewItem2 AddItemToTreeView(string id, string text, int total, string text2 = "")
        {
            TreeViewItem2 tree = new TreeViewItem2();
            tree.id = id;
            tree.text = text;
            tree.text2 = text2;
            tree.total = total;

            return tree;
        }*/

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _db.Dispose();
                }
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}