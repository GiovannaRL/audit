namespace OfflineXPlanner.Domain
{
    public class Room
    {
        public int ProjectId { get; set; }
        public int DepartmentId { get; set; }
        public int Id { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string PhotoFile { get; set; }
        public Department Dpto { get; set; }

        public Room() { }
        public Room(int projectId, int departmentId, int roomId, string number, string name)
        {
            this.ProjectId = projectId;
            this.DepartmentId = departmentId;
            this.Number = number;
            this.Name = name;
            this.Id = roomId;
        }
    }
}
