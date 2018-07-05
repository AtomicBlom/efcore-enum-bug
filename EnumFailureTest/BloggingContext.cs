using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace EnumFailureTest
{
    public class BloggingContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EnumTest;Trusted_Connection=True;MultipleActiveResultSets=true;Application Name=EnumTest");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var postModelBuilder = modelBuilder.Entity<Post>();
            postModelBuilder.Property(e => e.Type).IsRequired().HasMaxLength(9).HasConversion(typeof(string));
        }
    }

    public class Blog
    {
        public int BlogId { get; set; }
        public string Url { get; set; }

        public List<Post> Posts { get; set; }
    }

    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }

        public BlogType Type { get; set; }

    }

    public enum BlogType
    {
        Technical,
        MyLife,
        Recipe
    }
}
