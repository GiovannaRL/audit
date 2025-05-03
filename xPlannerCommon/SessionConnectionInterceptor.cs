using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;

namespace xPlannerCommon
{
    public class ContextEventArgs
    {
        public ContextEventArgs(DbConnectionInterceptionContext context)
        {
            Context = context;
        }
        public int DomainId { get; set; }
        public bool ShowAudaxwareInfo { get; set; }
        DbConnectionInterceptionContext Context { get; }
    }

    public delegate void ContextEventHandler(object sender, ContextEventArgs e);
    public class SessionConnectionInterceptor : IDbConnectionInterceptor
    {

        static public event ContextEventHandler ContextEvent;
        [ThreadStatic]
        static int DomainId;
        [ThreadStatic]
        static bool ShowAudaxwareInfo;

        public class ThreadSecurityInterceptor : IDisposable
        {
            public ThreadSecurityInterceptor(int domainId, bool showAudaxwareInfo)
            {
                DomainId = domainId;
                ShowAudaxwareInfo = showAudaxwareInfo;
            }
            void IDisposable.Dispose()
            {
                DomainId = 0;
                ShowAudaxwareInfo = false;
            }
        }

        public void Opened(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
        {
            // Set SESSION_CONTEXT to current domain_id whenever EF opens a connection
            try
            {
                if (ContextEvent == null)
                    return;
                var contextArgs = new ContextEventArgs(interceptionContext);
                if (DomainId != 0)
                {
                    contextArgs.DomainId = DomainId;
                    contextArgs.ShowAudaxwareInfo = ShowAudaxwareInfo;
                }
                else
                {
                    ContextEvent(this, contextArgs);
                }
                DbCommand cmd = connection.CreateCommand();
                cmd.CommandText = "EXEC sp_set_session_context @key=N'domain_id', @value=@domain_id";
                DbParameter param = cmd.CreateParameter();
                param.ParameterName = "@domain_id";
                param.Value = contextArgs.DomainId;
                cmd.Parameters.Add(param);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "EXEC sp_set_session_context @key=N'show_audax', @value=@show_audax";
                param = cmd.CreateParameter();
                param.ParameterName = "@show_audax";
                param.Value = contextArgs.ShowAudaxwareInfo;
                cmd.Parameters.Add(param);
                cmd.ExecuteNonQuery();
            }
            catch (System.NullReferenceException)
            {
                // If no user is logged in, leave SESSION_CONTEXT null (all rows will be filtered)
            }
        }

        public void Opening(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
        {
        }

        public void BeganTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
        {
        }

        public void BeginningTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
        {
        }

        public void Closed(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
        {
        }

        public void Closing(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
        {
        }

        public void ConnectionStringGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
        {
        }

        public void ConnectionStringGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
        {
        }

        public void ConnectionStringSet(DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext)
        {
        }

        public void ConnectionStringSetting(DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext)
        {
        }

        public void ConnectionTimeoutGetting(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext)
        {
        }

        public void ConnectionTimeoutGot(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext)
        {
        }

        public void DataSourceGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
        {
        }

        public void DataSourceGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
        {
        }

        public void DatabaseGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
        {
        }

        public void DatabaseGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
        {
        }

        public void Disposed(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
        {
        }

        public void Disposing(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
        {
        }

        public void EnlistedTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext)
        {
        }

        public void EnlistingTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext)
        {
        }

        public void ServerVersionGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
        {
        }

        public void ServerVersionGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
        {
        }

        public void StateGetting(DbConnection connection, DbConnectionInterceptionContext<System.Data.ConnectionState> interceptionContext)
        {
        }

        public void StateGot(DbConnection connection, DbConnectionInterceptionContext<System.Data.ConnectionState> interceptionContext)
        {
        }
    }

    public class SessionContextConfiguration : DbConfiguration
    {
        public SessionContextConfiguration()
        {
            AddInterceptor(new SessionConnectionInterceptor());
        }
    }
}