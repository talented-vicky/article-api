using ArticleApi.Models;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;

namespace ArticleApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options){}

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<PostLike> PostLikes { get; set; } = null!;
    public DbSet<CommentLike> CommentLikes { get; set; } = null!;
    public DbSet<PostImage> PostImages { get; set; } = null!;

    protected override void OnModelCreating (ModelBuilder modBld)    
    {
        base.OnModelCreating(modBld);

        // // configuring location column
        // modBld.Entity<Post>()
        //     .Property(p => p.Location)
        //     .HasColumnType("geometry Point(4326)"); //WGS 84 (standard lat/lng coordinate system, used by GPS).

        // creating GIST spatial index
        modBld.Entity<Post>()
            .HasIndex(p => p.Location)
            .HasMethod("GIST"); // used for spatial queries (other index methods are BTree, Hash, GIN)

    }
}