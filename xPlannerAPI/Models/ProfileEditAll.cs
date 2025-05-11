using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class ProfileEditAll
    {
        public string old_profile { get; set; }
        public string old_profile_budget { get; set; }
        public bool old_detailed_budget { get; set; }
        public int inventory_id_new_profile { get; set; }
    }
}