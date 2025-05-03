namespace OfflineXPlanner.Domain
{
    public class Department
    {
        public int department_id { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public int project_id { get; set; }

        public Department(int department_id, string description, string type, int project_id)
        {
            this.department_id = department_id;
            this.description = description;
            this.type = type;
            this.project_id = project_id;
        }

        public Department() { }
    }
}
