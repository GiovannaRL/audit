using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class ProjectMatchingValue
    {
        //private project tabProject;
        //private MatchingValuesView tabMVV;

        //public ProjectMatchingValue(project project, MatchingValuesView matchingValue)
        //{
        //    tabProject = project;
        //    tabMVV = matchingValue;
        public int project_id { get; set; }
        public string project_description { get; set; }
        public DateTime? project_start { get; set; }
        public DateTime? project_end { get; set; }
        public string status { get; set; }
        public string client { get; set; }
        public string hospital { get; set; }
        public string client_project_number { get; set; }
        public string hospital_project_number { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string default_cost_field { get; set; }
        public string comment { get; set; }
        public double medial_budget { get; set; }
        public double freight_budget { get; set; }
        public double warehouse_budget { get; set; }
        public double tax_budget { get; set; }
        public double warranty_budget { get; set; }
        public double misc_budget { get; set; }
        public DateTime date_added { get; set; }
        public string added_by { get; set; }
        public string total_projected { get; set; }
        public string project_budget { get; set; }
        public string projected_delta { get; set; }
        public string total_planned { get; set; }
        public string cliente_budget_delta { get; set; }
        //}

    }
}