using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Data.Entity;
using System.Text;
using System.Data.Entity.Core;
using xPlannerCommon.Models;
using xPlannerCommon.App_Data;

namespace xPlannerAPI.Services
{
    public class TableRepository<T> : IDisposable where T : class
    {
        public TableRepository()
        {
            _db = new audaxwareEntities();
            _db.Configuration.ProxyCreationEnabled = false;
            _db.Database.CommandTimeout = 600000;
            _dbType = _db.GetType();
            var p = _dbType.GetProperty(typeof(T).Name);
            // The entity property for the type project is projects, this is why we add the suffix s here
            if (p == null)
                p = _dbType.GetProperty(typeof(T).Name + "s");
            // This is a special case for facility. If more special cases arise we might have
            // to create a map to avoid too many if statements
            if (p == null && (typeof(T) == typeof(facility)))
                p = _dbType.GetProperty("facilities");
            else if (p == null && (typeof(T) == typeof(responsability)))
                p = _dbType.GetProperty("responsabilities");
            _tableProperty = p.GetValue(_db) as System.Data.Entity.DbSet<T>;
        }


        private static string CreateWhereClause<TValues>(IReadOnlyList<string> fields, IReadOnlyList<TValues> values)
        {
            return CreateWhereClause(fields, values, false);
        }

        private static string CreateWhereClause<TValues>(IReadOnlyList<string> fields, IReadOnlyList<TValues> values, bool showAudaxware)
        {
            var whereClause = new StringBuilder();
            if (fields != null)
            {
                for (var i = 0; i < fields.Count; ++i)
                {
                    if (values[i] == null) continue;

                    if (whereClause.Length > 0)
                        whereClause.Append(" AND ");
                    if (fields[i] == "domain_id")
                    {
                        if (showAudaxware && Helper.ShowAudaxWareInfo(int.Parse(
                               $"{values[i]}")))
                        {
                            whereClause.Append($"(domain_id = {values[i]} OR domain_id = 1)");
                        }
                        else
                        {
                            whereClause.Append($"(domain_id = {values[i]})");
                        }
                    }
                    else
                    {
                        whereClause.Append(values[i] is string
                            ? $"{fields[i]} = \"{values[i]}\""
                            : $"{fields[i]} = {values[i]}");
                    }
                }
            }
            return whereClause.ToString();
        }

        public List<T> GetAll<TGetAll>(string[] getAllFields, TGetAll[] ids, string[] includeFields, bool showAudaxware = false)
        {
            var whereClause = CreateWhereClause(getAllFields, ids, showAudaxware);
            var registers = !string.IsNullOrEmpty(whereClause) ? _tableProperty.Where(whereClause) : _tableProperty;

            if (includeFields != null)
            {
                foreach (var field in includeFields)
                {
                    registers = registers.Include(field);
                }
            }

            return registers.ToList();
        }

        public T Get<TKey>(string[] primaryKeys, TKey[] ids, string[] includeFields)
        {
            var whereClause = CreateWhereClause(primaryKeys, ids);
            var registers = _tableProperty.Where(whereClause);

            if (includeFields != null)
            {
                foreach (var field in includeFields)
                {
                    registers = registers.Include(field);
                }
            }

            if (registers.Count() != 1)
                throw new ObjectNotFoundException();
            return registers.First();
        }

        public T GetFirst<TKey>(string[] primaryKeys, TKey[] ids, string[] includeFields)
        {
            var whereClause = CreateWhereClause(primaryKeys, ids);
            var registers = _tableProperty.Where(whereClause);

            if (includeFields != null)
            {
                foreach (var field in includeFields)
                {
                    registers = registers.Include(field);
                }
            }

            if (!registers.Any())
                throw new ObjectNotFoundException();
            return registers.First();
        }

        public T GetByKey<TKey>(string keyName, TKey keyValue)
        {
            return Get(new string[] { keyName }, new TKey[] { keyValue }, null);
        }

        public void AttachItem<F>(F item) where F : class
        {
            var p = _dbType.GetProperty(typeof(F).Name);
            var context = p.GetValue(_db) as DbSet<F>;
            context.Attach(item);
        }

        public T Add(T item)
        {
            _tableProperty.Add(item);
            return _db.SaveChanges() <= 0 ? null : item;
        }

        public bool Update(T item)
        {
            _db.Entry(item).State = EntityState.Modified;
            return _db.SaveChanges() > 0;
        }

        public void Delete<TKey>(string[] primaryKeys, TKey[] ids)
        {
            var item = Get(primaryKeys, ids, null);
            _tableProperty.Remove(item);
            _db.SaveChanges();
        }

        public void Delete(T item)
        {
            _db.Entry(item).State = EntityState.Deleted;
            _db.SaveChanges();
        }

        public void Delete(IEnumerable<T> items)
        {
            if (items == null) return;

            foreach (var item in items)
            {
                _tableProperty.Remove(item);
            }
            _db.SaveChanges();
        }

        public DbContextTransaction BeginTransaction()
        {
            return _db.Database.BeginTransaction();
        }

        public void Commit(DbContextTransaction transaction)
        {
            transaction.Commit();
        }

        public void Roolback(DbContextTransaction transaction)
        {
            transaction.Rollback();
        }

        public void DeleteAll<TKey>(string[] getAllFields, TKey[] ids)
        {
            var items = GetAll(getAllFields, ids, null);
            Delete(items);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _tableProperty = null;
                _dbType = null;
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

        private readonly audaxwareEntities _db;
        private Type _dbType;
        private bool _disposed = false;
        private DbSet<T> _tableProperty;
    }
}