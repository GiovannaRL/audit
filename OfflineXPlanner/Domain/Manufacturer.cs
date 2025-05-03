namespace OfflineXPlanner.Domain
{
    public class Manufacturer
    {
        public int ID { get; set; }
        public int manufacturer_id { get; set; }
        public string description { get; set; }

        public Manufacturer() { }
        public Manufacturer(int manufacturer_id, string description)
        {
            this.manufacturer_id = manufacturer_id;
            this.description = description;
        }
    }
}
