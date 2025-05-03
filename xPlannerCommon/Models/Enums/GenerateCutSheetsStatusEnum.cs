namespace xPlannerCommon.Models.Enums
{
    public class GenerateCutSheetsStatus
    {
        public decimal Percentage { get; set; }
        public string Status { get; set; }
    }
    public static class GenerateCutSheetsStatusEnum
    {
        public static GenerateCutSheetsStatus InProgress { get { return new GenerateCutSheetsStatus { Percentage = 0, Status = "P" }; } }
        public static GenerateCutSheetsStatus Error { get { return new GenerateCutSheetsStatus { Percentage = -1, Status = "E" }; } }
        public static GenerateCutSheetsStatus Finished { get { return new GenerateCutSheetsStatus { Percentage = 100, Status = "F" }; } }
    }
}
