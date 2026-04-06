using AppCompras.Models;
using AppCompras.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace AppCompras.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<SystemLog> Logs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Configura o banco de dados SQLite para utilizar o arquivo Db.db na raiz do projeto
            optionsBuilder.UseSqlite("Data Source=Db.db");
        }
    }
}