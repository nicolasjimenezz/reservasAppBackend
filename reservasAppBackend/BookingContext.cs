using Microsoft.EntityFrameworkCore;
using reservasAppBackend;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace reservasAppBackend
{
    class BookingContext : DbContext
    {
        public BookingContext(DbContextOptions<BookingContext> options) : base(options) { }

        public DbSet<RegisteredUser> RegisteredUser { get; set; }
        public DbSet<BookedDate> BookedDate { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RegisteredUser>()
                .HasMany(u => u.BookedDates)
                .WithOne(d => d.RegisteredUser)
                .HasForeignKey(d => d.IdRegisteredUser);
        }
    }
}