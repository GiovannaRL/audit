using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportsWebJob.Models
{
    class ProcurementTOCItem
    {
        public int po_id { get; set; }
        public string po_number { get; set; }
        public string po_description { get; set; }
        public DateTime? status_date { get; set; }
        public int? aging_days { get; set; }
        public Decimal amount { get; set; }
        public string status { get; set; }
        public DateTime date_added { get; set; }
        public string added_by { get; set; }
        public string comment { get; set; }
        public string quote_number { get; set; }
        public string po_requested_number { get; set; }
        public string vendor { get; set; }
        public string ship_to { get; set; }
    }
}
