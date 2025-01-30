using Microsoft.EntityFrameworkCore;

namespace PracticeWithC1stApproach.Models
{
    public class EmployeeDBContext : DbContext
    {
        public EmployeeDBContext(DbContextOptions options) : base(options) 
        {
            
        }

        //table
       public  DbSet<EmployeeModel> Employees { get; set; }

    }
}
