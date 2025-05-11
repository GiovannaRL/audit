using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace xPlannerData.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Comment { get; set; }
        [Required]
        public string Password { get; set; }
        public Nullable<System.DateTime> LastActivityDate { get; set; }
        public Nullable<System.DateTime> LastLoginDate { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public Nullable<bool> IsOnLine { get; set; }
        public Nullable<bool> IsPasswordTemporary { get; set; }
        public bool accept_user_license { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string Hometown { get; set; }
        public List<string> roleNames { get; set; }
    }
}