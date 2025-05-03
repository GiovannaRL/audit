using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xPlannerCommon.Models
{
    public class AssetDifferencesStructure
    {
        public string field { get; set; }
        public string customized { get; set; }
        public string original { get; set; }

        public AssetDifferencesStructure(string field, string customized, string original)
        {
            this.field = field;
            this.customized = customized;
            this.original = original;
        }
    }
}
