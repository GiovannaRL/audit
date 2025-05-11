using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerCommon.Models
{
    public class CategorySettingsValue
    {
        public string Gases { get; set; }
        public string Plumbing { get; set; }
        public string HVAC { get; set; }
        public string IT { get; set; }
        public string Electrical { get; set; }
        public string Support { get; set; }
        public string Physical { get; set; }
        public string Environmental { get; set; }

        public CategorySettingsValue(string gases, string plumbing, string hvac, string it, string electrical, 
            string support, string physical, string environmental)
        {
            this.Gases = gases;
            this.Plumbing = plumbing;
            this.HVAC = hvac;
            this.IT = it;
            this.Electrical = electrical;
            this.Support = support;
            this.Physical = physical;
            this.Environmental = environmental;
        }
    }
}