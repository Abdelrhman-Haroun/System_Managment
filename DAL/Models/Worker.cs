namespace DAL.Models 
{ 
    public class Worker
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Incentives { get; set; }
        public int Holidays { get; set; }
        public decimal Penalties { get; set; }
    }
}
