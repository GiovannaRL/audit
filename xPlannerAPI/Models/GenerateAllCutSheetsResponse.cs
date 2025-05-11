using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class GenerateAllCutSheetsResponse
    {
        public bool IsNewGeneration { get; set; }
        public decimal Percentage { get; set; }
    }
}