using System;
using System.Collections.Generic;

namespace xPlannerAPI.Models
{
    public enum ImportAnalysisResultStatus { Ok, Invalid, RequiredColumnsMissing, AssetHintColumnMissing, InternalError, InvalidFile}
    // TODO(JLT): Rename the filee
    public class ImportAnalysisResult
    {
        public ImportAnalysisResultStatus Status { get; set; }
        public string WorkSheetName { get; set; }
        public string ErrorMessage { get; set; }
        public int TotalNewCatalog { get; set; }
        public int TotalNew { get; set; }
        public int TotalChanged { get; set; }
        public int TotalErrors { get; set; }
        public List<ImportItem> Items { get; set; }
        public IEnumerable<string> UsedColumns { get; set; }
        public IEnumerable<string> UnusedColumns { get; set; }

        public IEnumerable<string> StatusLabels
        {
            get
            {
                var ret = new List<string>();
                foreach (var status in Enum.GetValues(typeof(ImportItemStatus)))
                {
                    ret.Add(Enum.GetName(typeof(ImportItemStatus), status));
                }
                return ret;
            }
        }
    }
}
