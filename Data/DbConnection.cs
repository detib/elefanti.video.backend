using elefanti.video.backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace elefanti.video.backend.Data;
public class DbConnection : DbContext {
    public DbConnection(DbContextOptions<DbConnection> options) : base(options) {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Video> Videos { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Comment> Comments { get; set; }
}

