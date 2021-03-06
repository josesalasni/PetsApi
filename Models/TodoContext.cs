using petsapi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace petsapi.Models 
{     
    public class TodoContext : IdentityDbContext     
    {         
        public TodoContext(DbContextOptions<TodoContext> options) : base(options)         
        {         
            
        }                     
    
        public DbSet<Publication> Publications { get; set; }     
        public DbSet<Comment> Comments {get; set;}
        public DbSet<Category> Categories {get; set;}
        public DbSet<Picture> Pictures {get; set;}
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasData(
                new Category() {CategoryId = 1, CategoryName = "Perros"},
                new Category() {CategoryId = 2, CategoryName = "Gatos"},
                new Category() {CategoryId = 3, CategoryName = "Caballos"}
            );

            modelBuilder.Entity<Comment>()
                .HasOne(i => i.Publication)
                .WithMany(c => c.Comments)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade
            );
            
        }

    } 
}