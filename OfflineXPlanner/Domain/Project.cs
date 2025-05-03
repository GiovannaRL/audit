namespace OfflineXPlanner.Domain
{
    public class Project
    {
        public int project_id { get; set; }
        public string description { get; set; }
        public int id { get; set; }

        public Project(int project_id, string description)
        {
            this.project_id = project_id;
            this.description = description;
        }

        public Project(int id, int project_id, string description)
        {
            this.id = id;
            this.project_id = project_id;
            this.description = description;
        }
    }
}
