using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EnumFailureTest
{
    public class ProjectedEnumTests
    {
        private AppDbContext Context => new AppDbContext();
        private static bool _isInitialized;

        public ProjectedEnumTests()
        {
            if (_isInitialized) return;

            using (var context = new AppDbContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                context.Animals.Add(new Animal
                {
                    IdentificationMethods = new[]
                    {
                        new AnimalIdentification
                        {
                            Method = IdentificationMethod.EarTag,
                        }
                    }
                });
                context.SaveChanges();
            }

            _isInitialized = true;
        }

        //We can query without a projection, including the child property and it maps correctly.
        [Fact]
        public async Task EnumMapsViaStringCorrectlyWhenQueriedDirectly()
        {
            var animal = await Context.Animals.Include(a => a.IdentificationMethods).FirstOrDefaultAsync();
            Assert.Equal(IdentificationMethod.EarTag, animal.IdentificationMethods.First().Method);
        }

        //Attempting to perform any subquery on IdentificationMethods causes efcore to ignore the .HasConversion
        //So it expects the enum as an integer instead of a string.
        [Fact]
        public async Task EnumMapsViaIntegerWhenProjectedUsingASubquery()
        {
            var query = from animal in Context.Animals
                select new
                {
                    animal.Id,
                    animal.IdentificationMethods.FirstOrDefault().Method
                };
            var result = await query.SingleOrDefaultAsync();
            Assert.Equal(IdentificationMethod.EarTag, result.Method);
        }
    }

    public class AppDbContext : DbContext
    {
        public DbSet<Animal> Animals { get; set; }
        public DbSet<AnimalIdentification> AnimalIdentifiers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                //.UseSqlite("Data Source=test.db");
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EnumTest;Trusted_Connection=True;MultipleActiveResultSets=true;Application Name=EnumTest");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var postModelBuilder = modelBuilder.Entity<AnimalIdentification>();
            postModelBuilder.Property(e => e.Method).IsRequired().HasMaxLength(6).HasConversion(typeof(string));
        }
    }

    public class Animal
    {
        public int Id { get; set; }

        public ICollection<AnimalIdentification> IdentificationMethods { get; set; }
    }

    public class AnimalIdentification
    {
        public int Id { get; set; }

        public IdentificationMethod Method { get; set; }

    }

    public enum IdentificationMethod
    {
        Notch,
        EarTag,
        Rfid
    }
}
