namespace xPlannerCommon.Models.Enum
{
    public class ReportStatus
    {
        public string Category { get; set; }
        public decimal PercentageStart { get; set; }
        public decimal PercentageEnd { get; set; }
    }

    public static class ReportStatusCategory
    {
        public static ReportStatus Waiting { get { return new ReportStatus { Category = "Waiting", PercentageStart = 0, PercentageEnd = 0 }; } }
        public static ReportStatus Error { get { return new ReportStatus { Category = "Error", PercentageStart = -1, PercentageEnd = -1 }; } }
        public static ReportStatus Initializing { get { return new ReportStatus { Category = "Initializing", PercentageStart = 1, PercentageEnd = 1 }; } }
        public static ReportStatus DownloadingPhotos { get { return new ReportStatus { Category = "Downloading Photos", PercentageStart = 2, PercentageEnd = 48 }; } }
        public static ReportStatus Completed { get { return new ReportStatus { Category = "Completed", PercentageStart = 100, PercentageEnd = 100 }; } }
        public static ReportStatus Generating { get { return new ReportStatus { Category = "Generating", PercentageStart = 2, PercentageEnd = 98 }; } }
        public static ReportStatus Uploading { get { return new ReportStatus { Category = "Uploading", PercentageStart = 99, PercentageEnd = 99 }; } }

        public static string GetByValue(decimal value, bool includePhotos = false) {

            if (CheckError(value)) return Error.Category;
            if (CheckWaiting(value)) return Waiting.Category;
            if (CheckInitializing(value)) return Initializing.Category;
            if (CheckCompleted(value)) return Completed.Category;
            if (CheckUploading(value)) return Uploading.Category;
            if (includePhotos && CheckDownloadingPhotos(value)) return DownloadingPhotos.Category;

            return Generating.Category;
        }

        private static bool CheckError(decimal value)
        {
            return value >= Error.PercentageStart && value <= Error.PercentageEnd;
        }

        private static bool CheckWaiting(decimal value)
        {
            return value >= Waiting.PercentageStart && value <= Waiting.PercentageEnd;
        }

        private static bool CheckInitializing(decimal value)
        {
            return value >= Initializing.PercentageStart && value <= Initializing.PercentageEnd;
        }

        private static bool CheckCompleted(decimal value)
        {
            return value >= Completed.PercentageStart && value <= Completed.PercentageEnd;
        }

        private static bool CheckUploading(decimal value)
        {
            return value >= Uploading.PercentageStart && value <= Uploading.PercentageEnd;
        }

        private static bool CheckDownloadingPhotos(decimal value)
        {
            return value >= DownloadingPhotos.PercentageStart && value <= DownloadingPhotos.PercentageEnd;
        }
    }
}
