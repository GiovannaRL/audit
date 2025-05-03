using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xPlannerCommon.Models;

namespace xPlannerCommon.Services
{
    public class NotificationRepository : IDisposable
    {
        private audaxwareEntities _db;
        private bool _disposed = false;
        short _domainId;
        string _userId;

        public NotificationRepository()
        {
            this._db = new audaxwareEntities();
            using (var cmd = this._db.Database.Connection.CreateCommand())
            {
                this._db.Database.Connection.Open();
                cmd.CommandText = "EXEC sp_set_session_context 'domain_id', '1';";
                int x = cmd.ExecuteNonQuery();
            }
        }
        public NotificationRepository(short domainId, string userId): this()
        {
            _domainId = domainId;
            _userId = userId;
        }

        public bool Add(user_notification notification)
        {
            this._db.user_notification.Add(notification);
            return this._db.SaveChanges() > 0;
        }

        public void Notify(short domainId, string userId, string message)
        {
            user_notification notification = new user_notification();
            notification.domain_id = domainId;
            notification.userId = userId;
            notification.message = message;
            Add(notification);
        }

        public void Notify(string message)
        {
            Notify(_domainId, _userId, message);
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
