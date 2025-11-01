namespace DAL.Models
{
    public class Store
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public List<Resource> Resources { get; set; }
        public List<Product> Products { get; set; }
    }

}
