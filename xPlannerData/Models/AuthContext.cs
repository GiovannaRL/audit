using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerData.Models
{
    public class AuthContext : IdentityDbContext<ApplicationUser>
    {
        public AuthContext()
            : base("AuthContext", throwIfV1Schema: false)
        {

        }

        public static AuthContext Create()
        {
            return new AuthContext();
        }
    }
}