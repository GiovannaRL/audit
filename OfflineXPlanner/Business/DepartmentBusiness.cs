using OfflineXPlanner.Database;
using OfflineXPlanner.Database.Impl;
using OfflineXPlanner.Domain;
using OfflineXPlanner.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace OfflineXPlanner.Business
{
    public class DepartmentBusiness
    {
        public static DataTable LoadDepartments(int project_id)
        {
            IDepartmentDAO departmentDAO = new DepartmentDAO();

            return departmentDAO.GetDepartmentsAsDataTable(project_id);
        }

        public static List<Department> GetDepartments(int project_id)
        {
            IDepartmentDAO departmentDAO = new DepartmentDAO();

            return departmentDAO.GetDepartments(project_id);
        }

        public static bool InsertDepartment(Department dpt)
        {
            IDepartmentDAO departmentDAO = new DepartmentDAO();

            try
            {
                dpt.department_id = DatabaseUtil.GenerateNewDepartmentID(dpt.project_id);

                if (departmentDAO.InsertDepartment(dpt))
                {
                    return true;
                }

                MessageBox.Show("Error to try added department");
                return false;
            }
            catch (Exception)
            {
                MessageBox.Show("Error to try added department");
                return false;
            }
        }

        public static bool UpdateDepartment(Department dpt)
        {
            IDepartmentDAO departmentDAO = new DepartmentDAO();

            if (departmentDAO.UpdateDepartment(dpt))
            {
                return true;
            } else
            {
                MessageBox.Show("Was not possible to update department");
                return false;
            }
        }
    }
}
