using Jwt.Auth.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jwt.Auth.API.Data
{
    public class MyWorldDbContext:DbContext
    {
        public MyWorldDbContext(DbContextOptions<MyWorldDbContext> options):base(options)
        {
            
        }

        public DbSet<User> User { get; set; }
    }
}
