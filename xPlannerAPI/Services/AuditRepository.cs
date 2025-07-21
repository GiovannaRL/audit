using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using xPlannerCommon.Models;
using xPlannerAPI.Models;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;


namespace xPlannerAPI.Services
{
    public class AuditRepository : IDisposable
    {
        private readonly audaxwareEntities _db;
        private bool _disposed = false;
        private Dictionary<string, string> tableName = new Dictionary<string, string>();


        public AuditRepository()
        {
            _db = new audaxwareEntities();
            LoadTableName();
        }

        private void LoadTableName() {

            tableName.Add("asset", "ASSET");
            tableName.Add("assets_category", "CATEGORY");
            tableName.Add("assets_subcategory", "SUBCATEGORY");
            tableName.Add("assets_options", "ASSET OPTION");
            tableName.Add("assets_vendor", "VENDOR(ASSET)");
            tableName.Add("cost_center", "COST CENTER");
            tableName.Add("inventory_documents", "DOCUMENT");
            tableName.Add("inventory_options", "INVENTORY OPTION");
            tableName.Add("inventory_purchase_order", "INVENTORY PO");
            tableName.Add("manufacturer", "MANUFACTURER");
            tableName.Add("project", "PROJECT");
            tableName.Add("project_addresses", "ADDRESS");
            tableName.Add("project_documents", "DOCUMENT");
            tableName.Add("project_phase", "PHASE");
            tableName.Add("project_room_inventory", "INVENTORY"); 
            tableName.Add("project_department", "DEPARTMENT");
            tableName.Add("project_room", "ROOM");
            tableName.Add("purchase_order", "PO");
            tableName.Add("vendor", "VENDOR");
            tableName.Add("vendor_contact", "CONTACT(VENDOR)");
        }

        public IEnumerable<get_all_audit_data_Result> AllAuditData(int domainId)
        {
            try
            {
                var auditedData = _db.get_all_audit_data(domainId, null, null).ToList();
                foreach (var item in auditedData)
                {
                    if (tableName.ContainsKey(item.table_name))
                    {
                        item.table_name = tableName[item.table_name];
                    }
                }
                return auditedData;
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error loading audit log. Exception {e.Message}");
                return null;
            }
        }

        public IEnumerable<AuditData> GetAuditData(int auditLogId)
        {
            var auditLog = _db.audit_log.Include("project")
                                        .FirstOrDefault(x => x.audit_log_id == auditLogId);

            if (auditLog == null)
                return new List<AuditData>();

            var originalData = JsonConvert.DeserializeObject<Dictionary<string, string>>(auditLog.original ?? "{}");
            var modifiedData = JsonConvert.DeserializeObject<Dictionary<string, string>>(auditLog.modified ?? "{}");

            var allKeys = originalData.Keys.Union(modifiedData.Keys).Distinct();
            var result = new List<AuditData>();

            foreach (var key in allKeys)
            {
                string originalValue = originalData.ContainsKey(key) ? originalData[key] : null;
                string modifiedValue = modifiedData.ContainsKey(key) ? modifiedData[key] : null;

                // add modified values
                if (originalValue != modifiedValue)
                {
                    result.Add(new AuditData
                    {
                        column = key,
                        original = originalValue,
                        modified = modifiedValue
                    });
                }
            }

            return result;
        }

        public IEnumerable<AuditWithChanges> AllAuditDataWithChanges(int domainId)
        {
            var all = _db.get_all_audit_data(domainId, null, null).ToList();


            var result = new List<AuditWithChanges>();

            foreach (var audit in all)
            {
                var changes = GetAuditData(audit.audit_log_id)
                                .Select(c => c.column)
                                .ToList();

                result.Add(new AuditWithChanges
                {
                    audit_log_id = audit.audit_log_id,
                    username = audit.username,
                    operation = audit.operation,
                    table_name = audit.table_name,
                    table_pk = audit.table_pk,
                    comment = audit.comment,
                    modified_date = audit.modified_date,
                    project_description = audit.project_description,
                    asset_code = audit.asset_code,
                    // Concatenates the modified fields into a string
                    changed_fields = string.Join(", ", changes)
                });
            }

            return result;
        }



        public bool CompareAndSaveAuditedData(Object oldData, Object newData, string operation, Object emptyObject = null, string header = null)
        {
            try
            {
                var table_name = oldData.GetType().BaseType.Name == "Object" ? oldData.GetType().Name : oldData.GetType().BaseType.Name;

                if (table_name == "user_notification") return true;

                //COMPARE DATA
                Dictionary<string, object> oldValues = new Dictionary<string, object>();
                Dictionary<string, object> newValues = new Dictionary<string, object>();
                var properties = new string[] { typeof(int).Name, typeof(int?).Name, typeof(string).Name, typeof(decimal).Name, typeof(decimal?).Name, typeof(bool).Name, typeof(bool?).Name, typeof(DateTime).Name, typeof(DateTime?).Name, typeof(short).Name, typeof(short?).Name };
                PropertyInfo[] props = oldData.GetType().GetProperties();
                var tablePK = new Dictionary<string, string>();
            
                if (emptyObject == null)
                    emptyObject = oldData;

                //BEGIN GET PRIMARY KEY
                var objectContext = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)_db).ObjectContext;
                //Get the approperiate overload of CreateObjectSet method
                var methodInfo = typeof(ObjectContext).GetMethods()
                                 .Where(m => m.Name == "CreateObjectSet").First();

                //Supply the generic type of the method
                var genericMethodInfo = methodInfo.MakeGenericMethod(emptyObject.GetType());

                //Invoke the method and get the ObjectSet<?> as an object          
                var set = genericMethodInfo.Invoke(objectContext, new object[] { });

                //Retrieve EntitySet of the set
                var entityProp = set.GetType().GetProperty("EntitySet");
                var entitySet = (EntitySet)entityProp.GetValue(set);
                EntitySet entSet = entitySet;
                IEnumerable<string> keyNames = entSet.ElementType.KeyMembers.Select(k => k.Name);

                foreach (var key in keyNames)
                {
                    tablePK.Add(key, oldData.GetType().GetProperty(key).GetValue(oldData, null).ToString());
                }
                //END GET PRIMARY KEY

                //COMPARE OLD AND NEW DATA
                foreach (PropertyInfo prop in props)
                {
                    if (properties.Contains(prop.PropertyType.Name))
                    {
                        string propName = prop.Name;
                        var oldValue = prop.GetValue(oldData)?.ToString().Trim();
                        if (newData == null)
                        {
                            oldValues.Add(propName, oldValue);
                        }
                        else
                        {
                            var newValue = prop.GetValue(newData)?.ToString().Trim();
                            if (prop.PropertyType.FullName.Contains("Decimal") && !String.IsNullOrEmpty(oldValue) && !String.IsNullOrEmpty(newValue))
                            {
                                oldValue = string.Format("{0:0.00}", Decimal.Parse(oldValue));
                                newValue = string.Format("{0:0.00}", Decimal.Parse(newValue));
                            }
                            if (oldValue != newValue)
                            {
                                
                                oldValues.Add(propName, oldValue);
                                newValues.Add(propName, newValue);
                            }
                        }
                    }
                }

                if (oldValues.Count() > 0)
                {
                    var jsonOld = JsonConvert.SerializeObject(oldValues);
                    var jsonNew = JsonConvert.SerializeObject(newValues);
                    var userRepository = new AspNetUsersRepository();
                    var userId = userRepository.GetLoggedUserId();

                    //SAVE DATA 
                    var auditLog = new audit_log();
                    auditLog.domain_id = oldData.GetType().GetProperty("domain_id") != null ? (short)oldData.GetType().GetProperty("domain_id").GetValue(oldData, null) : (oldData.GetType().GetProperty("project_domain_id") != null ? (short)oldData.GetType().GetProperty("project_domain_id")?.GetValue(oldData, null) : (short)oldData.GetType().GetProperty("po_domain_id")?.GetValue(oldData, null));
                    auditLog.project_id = (int?)oldData.GetType().GetProperty("project_id")?.GetValue(oldData, null);
                    auditLog.asset_id = (int?)oldData.GetType().GetProperty("asset_id")?.GetValue(oldData, null);
                    auditLog.asset_domain_id = table_name == "asset" ? (short?)oldData.GetType().GetProperty("domain_id")?.GetValue(oldData, null) : (short?)oldData.GetType().GetProperty("asset_domain_id")?.GetValue(oldData, null);
                    auditLog.user_id = userId;
                    auditLog.operation = operation;
                    auditLog.table_name = table_name;
                    auditLog.table_pk = JsonConvert.SerializeObject(tablePK);
                    auditLog.header = header;
                    auditLog.original = jsonOld;
                    auditLog.modified = jsonNew;
                    auditLog.modified_date = DateTime.Now;
                    _db.audit_log.Add(auditLog);
                    _db.SaveChanges();
                }

                return true;

            }
            catch (Exception e)
            {
                Trace.TraceError($"Error saving audit log. Exception {e.Message}");
                return false;
            }
        }


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