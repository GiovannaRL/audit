namespace xPlannerCommon.Services
{
    static public class BlobContainersName
    {
        public static string Report = "reports";
        public static string GenericPhoto = "photo";
        public static string GenericCoversheet = "coversheet";
        public static string GenericFullCutSheet = "fullcutsheet";
        public static string GenericProjDocuments = "project-documents";

        public static string ProjDocuments(int domain_id)
        {
            return GenericProjDocuments + domain_id.ToString();
        }

        public static string Coversheet(int domain_id)
        {
            return GenericCoversheet + domain_id.ToString();
        }

        public static string Cutsheet(int domain_id)
        {
            return "cutsheet" + domain_id.ToString();
        }

        public static string FullCutsheet(int domain_id)
        {
            return GenericFullCutSheet + domain_id.ToString();
        }

        public static string POCover(int domain_id)
        {
            return "coverpo" + domain_id.ToString();
        }

        public static string PO(int domain_id)
        {
            return "po" + domain_id.ToString();
        }

        public static string Quote(int domain_id)
        {
            return "quote" + domain_id.ToString();
        }

        public static string Cadblock(int domain_id)
        {
            return "cadblock" + domain_id.ToString();
        }

        public static string Revit(int domain_id)
        {
            return "revit" + domain_id.ToString();
        }

        public static string Photo(int domain_id)
        {
            return GenericPhoto + domain_id.ToString();
        }
    }
}