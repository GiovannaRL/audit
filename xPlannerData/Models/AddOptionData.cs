using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerData.Models
{
    public class AddOptionData
    {
        public List<int> phase_ids { get; set; }
        public List<int> department_ids { get; set; }
        public List<int> room_ids { get; set; }
        public List<int> option_ids { get; set; }
        public bool none_selected { get; set; }
        public bool erase_selected { get; set; }

        public AddOptionData()
        {
            this.phase_ids = new List<int>();
            this.department_ids = new List<int>();
            this.room_ids = new List<int>();
            this.option_ids = new List<int>();
            this.none_selected = false;
            this.erase_selected = false;
        }
    }
}