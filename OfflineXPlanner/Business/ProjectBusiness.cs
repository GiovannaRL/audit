using OfflineXPlanner.Database;
using OfflineXPlanner.Database.Impl;
using OfflineXPlanner.Domain;
using OfflineXPlanner.Domain.Enums;
using OfflineXPlanner.Facade;
using OfflineXPlanner.Facade.Domain;
using OfflineXPlanner.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OfflineXPlanner.Business
{
    public class ProjectBusiness
    {
        public static List<Project> Import()
        {
            var projects = ProjectFacade.ImportProjects();
            if (!ListUtil.isEmptyOrNull(projects))
            {
                return projects.Select(p => new Project(p.project_id, p.project_description)).ToList();
            }

            return new List<Project>();
        }

        public static int DeleteProject(int projectId)
        {
            IProjectDAO projectDAO = new ProjectDAO();

            return projectDAO.DeleteProject(projectId);
        }



        public static void ImportData(Project project, ImportMode importMode)
        {
            var data = ProjectFacade.ImportData(project);
            if (data != null)
            {
                ProgressBarForm progressBarForm = new ProgressBarForm("Importing data from audaxware", CalculateMaxStepsProgressBar(data));
                progressBarForm.Show();

                if (importMode == ImportMode.overrideData)
                {
                    AddProjectDataOverriding(data, progressBarForm);
                } else
                {
                    AddProjectOnlyNew(data, progressBarForm);
                }

                progressBarForm.Close();
            }
        }

        public static int GetNextInventoryId(int projectId) {
            IInventoryDAO inventoryDAO = new InventoryDAO();

            return inventoryDAO.GetNextInventoryId(projectId);
        }

        private static int CalculateMaxStepsProgressBar(ImportProjectDataResponse data)
        {
            if (data.project == null)
            {
                return 0;
            }

            int max = 1;
            if (!ListUtil.isNull(data.costCenters))
            {
                max += data.costCenters.Count();
            }

            if (!ListUtil.isNull(data.departments))
            {
                max += data.departments.Count();
            }

            if (!ListUtil.isNull(data.rooms))
            {
                max += data.rooms.Count();
            }

            if (!ListUtil.isNull(data.assets))
            {
                max += data.assets.Count();
            }

            return max;
        }

        private static void AddProjectOnlyNew(ImportProjectDataResponse data, ProgressBarForm progressBarForm)
        {
            if (data != null && data.project != null)
            {
                IProjectDAO projectDAO = new ProjectDAO();
                IDepartmentDAO departmentDAO = new DepartmentDAO();
                IRoomDAO roomDAO = new RoomDAO();
                IInventoryDAO inventoryDAO = new InventoryDAO();

                projectDAO.InsertIfNotExists(data.project);
                progressBarForm.PerformStep();

                // Insert cost centers
                if (!ListUtil.isEmptyOrNull(data.costCenters))
                {
                    foreach (var costCenter in data.costCenters)
                    {
                        projectDAO.InsertCostCenter(new CostCenter
                        {
                            code = costCenter.code,
                            description = costCenter.description,
                            is_default = costCenter.is_default ?? false,
                            project_id = data.project.project_id
                        });
                        progressBarForm.PerformStep();
                    }
                }

                if (!ListUtil.isEmptyOrNull(data.departments))
                {
                    // Insert departments
                    foreach (var dpt in data.departments)
                    {
                        departmentDAO.InsertIfNotExists(new Department(dpt.department_id, dpt.description, dpt.department_type.description, data.project.project_id));
                        progressBarForm.PerformStep();
                    }

                    // Insert rooms
                    if (!ListUtil.isEmptyOrNull(data.rooms))
                    {
                        foreach (var room in data.rooms)
                        {
                            roomDAO.InsertIfNotExists(new Room(data.project.project_id, room.department_id, room.room_id, room.drawing_room_number, room.drawing_room_name));
                            progressBarForm.PerformStep();
                        }

                        // Insert assets
                        if (!ListUtil.isEmptyOrNull(data.assets))
                        {
                            /* Obtém JSNs do banco para verificar e adicionar os novos */
                            ICatalogDAO catalogDAO = new CatalogDAO();
                            List<JSN> databaseJSNs = catalogDAO.GetAllJSNAslist();

                            var inventory = data.assets.Select(asset => new {
                                project_id = data.project.project_id,
                                department_id = asset.department_id,
                                room_id = asset.room_id,
                                inventory_item = new Inventory(asset)
                            });
                            foreach (var item in inventory)
                            {
                                if (inventoryDAO.InsertIfNotExists(item.project_id, item.department_id, item.room_id, item.inventory_item))
                                {
                                    /* Adiciona JSN caso não exista */
                                    databaseJSNs = DatabaseUtil.CheckAndAddJSN(databaseJSNs, item.inventory_item);
                                }

                                progressBarForm.PerformStep();
                            }
                        }
                    }
                }
            }
        }

        private static void AddProjectDataOverriding(ImportProjectDataResponse data, ProgressBarForm progressBarForm)
        {
            if (data != null && data.project != null)
            {
                IProjectDAO projectDAO = new ProjectDAO();
                IDepartmentDAO departmentDAO = new DepartmentDAO();
                IRoomDAO roomDAO = new RoomDAO();
                IInventoryDAO inventoryDAO = new InventoryDAO();
                
                // Delete the project if exists
                projectDAO.DeleteProject(data.project);

                // Insert the project with the new data
                projectDAO.InsertProject(data.project);
                progressBarForm.PerformStep();

                // Insert cost centers
                if (!ListUtil.isEmptyOrNull(data.costCenters))
                {
                    foreach (var costCenter in data.costCenters)
                    {
                        projectDAO.InsertCostCenter(new CostCenter
                        {
                            code = costCenter.code,
                            description = costCenter.description,
                            is_default = costCenter.is_default ?? false,
                            project_id = data.project.project_id
                        });
                        progressBarForm.PerformStep();
                    }
                }

                if (!ListUtil.isEmptyOrNull(data.departments))
                {
                    // Insert departments
                    foreach (var dpt in data.departments)
                    {
                        departmentDAO.InsertDepartment(new Department(dpt.department_id, dpt.description, dpt.department_type.description, data.project.project_id));
                        progressBarForm.PerformStep();
                    }

                    // Insert rooms
                    if (!ListUtil.isEmptyOrNull(data.rooms))
                    {
                        foreach (var room in data.rooms)
                        {
                            roomDAO.Insert(new Room(data.project.project_id, room.department_id, room.room_id, room.drawing_room_number, room.drawing_room_name));
                            progressBarForm.PerformStep();
                        }

                        // Insert assets
                        if (!ListUtil.isEmptyOrNull(data.assets)) {

                            /* Obtém JSNs do banco para verificar e adicionar os novos */
                            ICatalogDAO catalogDAO = new CatalogDAO();
                            List<JSN> databaseJSNs = catalogDAO.GetAllJSNAslist();

                            var inventory = data.assets.Select(asset => new {
                                project_id = data.project.project_id,
                                department_id = asset.department_id,
                                room_id = asset.room_id,
                                inventory_item = new Inventory(asset)
                            } );

                            foreach (var item in inventory) {

                                inventoryDAO.InsertInventory(item.project_id, item.department_id, item.room_id, item.inventory_item);

                                /* Adiciona JSN caso não exista */
                                databaseJSNs = DatabaseUtil.CheckAndAddJSN(databaseJSNs, item.inventory_item);

                                progressBarForm.PerformStep();
                            }
                        }
                    }
                }
            }
        }
    }
}
