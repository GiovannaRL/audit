using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class ProjectTreeItems
    {
        public int id { get; set; }
        public int domain_id { get; set; }
        public string text { get; set; }
        public bool mine { get; set; }
        public string status { get; set; }

        public List<ProjectTreeItems> items { get; set; }

        public ProjectTreeItems(int itemId, int itemDomainId, string itemDescription, bool mineProjects, string projectStatus )
        {
            id = itemId;
            domain_id = itemDomainId;
            text = itemDescription;
            mine = mineProjects;
            status = projectStatus;
            items = new List<ProjectTreeItems>();
        }

        public ProjectTreeItems(int itemId, string itemDescription)
        {
            id = itemId;
            text = itemDescription;
            items = new List<ProjectTreeItems>();
        }
    }
}