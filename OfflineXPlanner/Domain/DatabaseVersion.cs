namespace OfflineXPlanner.Domain
{
    public class DatabaseVersion
    {
        public int Number { get; set; }
        public bool isNewInventoryStructure { get; set; }

        public  DatabaseVersion()
        {
            this.Number = 0;
            this.isNewInventoryStructure = false;
        }
    }
}
