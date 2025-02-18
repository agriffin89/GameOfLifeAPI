using Microsoft.EntityFrameworkCore;
using GameOfLifeAPI.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace GameOfLifeAPI.Data
{
    public class GameOfLifeContext : DbContext
    {
        public GameOfLifeContext(DbContextOptions<GameOfLifeContext> options) : base(options) { }

        public DbSet<Board> Boards { get; set; } //  Add DbSet for Board

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=gameoflife.db");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Board>().HasKey(b => b.Id);
        }
    }
}
