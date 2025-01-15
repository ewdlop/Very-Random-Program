using Microsoft.EntityFrameworkCore;

namespace 亂七八糟.DataAccessLayer;

/// <summary>
/// <seealso href="https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli""/>
/// </summary>
public class BloggingContext : DbContext
{
    public static readonly BloggingContext Default = new BloggingContext();
    public DbSet<Blog>? Blogs { get; set; } = Default.Blogs;
    public DbSet<Post>? Posts { get; set; } = Default.Posts;
    public string? DbPath { get; } = string.Empty;

    public BloggingContext()
    {
        Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        string? path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "blogging.db");
    }

    public BloggingContext(DbContextOptions options) : base(options)
    {
        Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        string? path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "blogging.db");
    }

    public BloggingContext(BloggingContext context)
    {
        Blogs = context.Set<Blog>();
        Posts = context.Set<Post>();
        DbPath = context.DbPath;
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}
