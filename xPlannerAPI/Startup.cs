using System;
using System.Collections.Generic;
using System.Linq;
using xPlannerCommon;
using Microsoft.Owin;
using Owin;
using xPlannerCommon.Models;
using xPlannerCommon.App_Data;

[assembly: OwinStartup(typeof(xPlannerAPI.Startup))]

namespace xPlannerAPI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            SessionConnectionInterceptor.ContextEvent += SessionConnectoinInterceptor_ContextEvent;
            ConfigureAuth(app);
        }

        private void SessionConnectoinInterceptor_ContextEvent(object sender, ContextEventArgs e)
        {
            if (System.Web.HttpContext.Current == null)
            {
                e.DomainId = -1;
                e.ShowAudaxwareInfo = false;
                return;
            }
            var authorization = System.Web.HttpContext.Current.Request.Headers.Get("Authorization");
            if (authorization != null)
            {
                UserData user_data;
                if (Helper.TokenData.TryGetValue(authorization.Replace("Bearer", "").Trim(), out user_data))
                {
                    if (user_data.loggedDomain != null)
                    {
                        e.DomainId = user_data.loggedDomain.domain_id;
                        e.ShowAudaxwareInfo = user_data.loggedDomain.show_audax_info;
                    }
                }
            }
        }
    }
}
