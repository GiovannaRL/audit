namespace reportsWebJob.Models
{
    public class SourceLocation
    {
        public int domain_id { get; set; }
        public int room_id { get; set; }
        public string drawing_room_number { get; set; }
        public string drawing_room_name { get; set; }
        public int room_quantity { get; set; }
        public int project_id { get; set; }
        public int phase_id { get; set; }
        public int department_id { get; set; }
    }
}
