using OfflineXPlanner.Domain.Enums;
using System;
using System.Collections.Generic;

namespace OfflineXPlanner.Domain
{
    public class ExportAnalysisResult
    {
        public ExportAnalysisResultStatusEnum Status { get; set; }
        public string ErrorMessage { get; set; }
        public int TotalNewCatalog { get; set; }
        public int TotalNew { get; set; }
        public int TotalChanged { get; set; }
        public int TotalErrors { get; set; }
        public List<Inventory> Items { get; set; }
        public IEnumerable<string> UsedColumns { get; set; }
        public IEnumerable<string> UnusedColumns { get; set; }

        public IEnumerable<string> StatusLabels
        {
            get
            {
                var ret = new List<string>();
                foreach (var status in Enum.GetValues(typeof(ExportItemStatusEnum)))
                {
                    ret.Add(Enum.GetName(typeof(ExportItemStatusEnum), status));
                }
                return ret;
            }
        }
    }
}
