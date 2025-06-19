using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xPlannerCommon.Models;

namespace xPlannerAPI.Extensions
{
    public static class ProjectRoomInventoryExtensions
    {
        public enum IgnoreLocation
        {
            None,
            Department,
            Phase,
            Project
        }

        private static HashSet<string> _defaultFieldsToIgnore = new HashSet<string>()
        {
            "inventory_id", // Primary key from project_room_inventory  
            "room_id",
            "cost_center_id", // A method will be implemented to compare cost_center  
            "added_by", // Set by the current user context  
            "asset_it_connectivity", // Not a copyable field  
            "asset_it_connectivity1", // Duplicate IT connectivity, not copied  
            "bundle_inventory", // Not a copyable field  
            "documents_associations", // Not a copyable field  
            "inventory_options", // A method will be implemented for inventory_options  
            "cost_center", // A method will be implemented for cost_center  
            "inventory_documents", // Document list tested separately  
            "inventory_purchase_order", // Not a copyable field  
            "project_documents", // Associated document from the source project  
            "project_room", // Location property  
            "copy_link" // Analyze copy_link methods  
            
        };

        public static void AssertEqual(this project_room_inventory current, project_room_inventory other, IgnoreLocation options)
        {
            Assert.IsNotNull(current, "The source project_room_inventory is null.");
            Assert.IsNotNull(other, "The target project_room_inventory is null.");

            var fieldsToCompare = GetFieldsToCompare();
            var ignoredFields = new HashSet<string>(_defaultFieldsToIgnore);

            var optionIgnoredFields = GetIgnoredFieldsFromOption(options);
            foreach (var field in optionIgnoredFields)
            {
                ignoredFields.Add(field);
            }

            foreach (var fieldName in fieldsToCompare)
            {
                if (ignoredFields.Contains(fieldName))
                    continue;

                var propInfo = typeof(project_room_inventory).GetProperty(fieldName);
                if (propInfo == null || !propInfo.CanRead)
                    continue;

                var value1 = propInfo.GetValue(current);
                var value2 = propInfo.GetValue(other);

                if (!object.Equals(value1, value2))
                {
                    Assert.Fail($"Field '{fieldName}' does not match. Expected: '{value1}' but was: '{value2}'");
                }
            }
        }

        private static List<string> GetFieldsToCompare()
        {
            return typeof(project_room_inventory)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name)
                .ToList();
        }

        private static HashSet<string> GetIgnoredFieldsFromOption(IgnoreLocation options = IgnoreLocation.None)
        {
            var ignored = new HashSet<string>();

            switch(options)
            {
                case IgnoreLocation.Project:
                    ignored.Add("project_id");
                    goto case IgnoreLocation.Phase;

                case IgnoreLocation.Phase:
                    ignored.Add("phase_id");
                    goto case IgnoreLocation.Department;

                case IgnoreLocation.Department:
                    ignored.Add("department_id");
                    break;

                case IgnoreLocation.None:
                default:
                    break;                     
            }

            if (ignored.Count > 0) {
                foreach (var field in ignored) 
                {
                    ignored.Add(field);
                }
            }

            return ignored;
        }
    }
}
