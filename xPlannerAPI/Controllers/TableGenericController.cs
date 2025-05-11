using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Reflection;
using xPlannerAPI.Services;
using System.Net.Http;
using System.Net;

namespace xPlannerAPI.Controllers
{
    public class TableGenericController<T> : AudaxWareController where T : class
    {
        public TableGenericController(string[] keyFields, string[] getAllFields)
        {
            _keyFields = keyFields;
            _getAllFields = getAllFields;
            _includeFields = new string[] { };
            _showAudaxware = false;
            _type = typeof(T);
        }

        public TableGenericController(string[] keyFields, string[] getAllFields, bool showAudaxware, bool duplicate = false)
        {
            _keyFields = keyFields;
            _getAllFields = getAllFields;
            _includeFields = new string[] { };
            _showAudaxware = showAudaxware;
            _duplicate = duplicate;
            _type = typeof(T);
        }

        public TableGenericController(string[] keyFields, string[] getAllFields, string[] includeFields, bool showAudaxware = false, bool duplicate = false)
        {
            _keyFields = keyFields;
            _getAllFields = getAllFields;
            _includeFields = includeFields;
            _showAudaxware = showAudaxware;
            _duplicate = duplicate;
            _type = typeof(T);
        }

        /// <summary>
        /// Overload this method in order to update the item prior
        /// to doing the update on the database
        /// </summary>
        /// <param name="item"></param>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        /// <param name="id3"></param>
        /// <param name="id4"></param>
        /// <param name="id5"></param>
        /// <returns></returns>
        protected virtual bool UpdateReferences(T item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            return true;
        }

        protected virtual T DuplicateItem(T item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            return Add(item, id1, id2, id3, id4, id5);
        }

        protected int?[] GetIds(int? id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            return new int?[] { id1, id2, id3, id4, id5 };
        }

        [ActionName("All")]
        public virtual IEnumerable<T> GetAll(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using ( var repository = new TableRepository<T>())
            {
                return repository.GetAll(_getAllFields, GetIds(id1, id2, id3, id4, id5), _includeFields, _showAudaxware);
            }
        }

        [ActionName("All")]
        public virtual IEnumerable<T> GetAll()
        {
            using (var repository = new TableRepository<T>())
            {
                return repository.GetAll(_getAllFields, GetIds(null), _includeFields);
            }
        }

        [ActionName("Item")]
        public virtual T GetItem(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (var repository = new TableRepository<T>())
            {
                return repository.Get(_keyFields, GetIds(id1, id2, id3, id4, id5), _includeFields);
            }
        }

        [ActionName("Item")]
        virtual public T Add([FromBody] T item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (var repository = new TableRepository<T>())
            {
                int?[] ids = GetIds(id1, id2, id3, id4, id5);
                for (int i = 0; i < _getAllFields.Length; ++i)
                {
                    PropertyInfo prop = _type.GetProperty(_getAllFields[i]);
                    if (prop != null)
                    {
                        if (prop.PropertyType.FullName.Contains("System.Int16"))
                        {
                            short id = (short)ids[i];
                            prop.SetValue(item, id);
                        }
                        else
                            prop.SetValue(item, ids[i]);
                    }
                }
                PropertyInfo p = _type.GetProperty("date_added");
                if (p != null)
                    p.SetValue(item, DateTime.Now);
                p = _type.GetProperty("added_by");
                if (p != null)
                    p.SetValue(item, UserName);
                if (!UpdateReferences(item, id1, id2, id3, id4, id5))
                    return null;
                return repository.Add(item);
            }
        }

        [ActionName("Item")]
        public virtual T Put(T item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            var oldItem = GetItem(id1, id2, id3, id4, id5); //AUDIT: GET OLD DATA TO COMPARE

            using (var repository = new TableRepository<T>())
            {
                if (_duplicate == true && id1 != 1)
                {
                    if (Convert.ToInt32(item.GetType().GetProperty("domain_id").GetValue(item)) == 1)
                    {
                        return DuplicateItem(item, id1, id2, id3, id4, id5);
                    }
                }

                if (!UpdateReferences(item, id1, id2, id3, id4, id5))
                    return null;
                // TODO(JLT): We return false but we did not identify
                // how angular returns the result of this operation
                if (repository.Update(item)) {
                    //AUDIT
                    using (var auditRep = new AuditRepository())
                    {
                        auditRep.CompareAndSaveAuditedData(oldItem, item, "UPDATE", null);
                    }
                    return item; 
                }

                return null;
            }
        }

        [ActionName("Item")]
        public virtual HttpResponseMessage Delete(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            if (!_showAudaxware || id1 != 1 || HasDomainAccess(1))
            {
                var oldItem = GetItem(id1, id2, id3, id4, id5); //AUDIT: GET OLD DATA TO COMPARE
                using (var repository = new TableRepository<T>())
                {
                    repository.Delete(_keyFields, GetIds(id1, id2, id3, id4, id5));
                    //AUDIT
                    using (var auditRep = new AuditRepository())
                    {
                        auditRep.CompareAndSaveAuditedData(oldItem, null, "DELETE");
                    }

                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [ActionName("All")]
        public void DeleteAll(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            var items = GetAll(id1, id2, id3, id4, id5);
            using (var repository = new TableRepository<T>())
            {
                repository.DeleteAll(_getAllFields, GetIds(id1, id2, id3, id4, id5));
                //AUDIT
                using (var auditRep = new AuditRepository())
                {
                    foreach (var item in items)
                    {
                        auditRep.CompareAndSaveAuditedData(item, null, "DELETE");
                    }
                }
            }
        }

        string[] _keyFields;
        string[] _getAllFields;
        string[] _includeFields;
        bool _showAudaxware;
        bool _duplicate;
        Type _type;
    }
}
